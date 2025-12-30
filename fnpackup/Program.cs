using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System.Collections.Frozen;
using System.Net;
using System.Text.Json.Serialization;

namespace fnpackup
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddHttpClient();
            builder.Services.AddControllers();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });
            builder.Services.AddDynamicStaticFile();

            var app = builder.Build();

            app.MapControllers();
            app.UseCors("AllowAll");
            app.UseRouting();

            app.UseDynamicStaticFile();

            app.Run();
        }
    }

    public static class StaticFileExtends
    {
        public static IServiceCollection AddDynamicStaticFile(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddSingleton<DynamicFileProvider>();

            return services;
        }
        public static WebApplication UseDynamicStaticFile(this WebApplication app)
        {
            DynamicFileProvider dfp = app.Services.GetService<DynamicFileProvider>();
            dfp.SetDefault(Path.Combine(app.Environment.ContentRootPath, "web"));
            FileServerOptions options = new FileServerOptions
            {
                FileProvider = dfp,
                EnableDefaultFiles = true,
                EnableDirectoryBrowsing = false,
            };
            options.DefaultFilesOptions.DefaultFileNames = ["index.html", "index.htm", "default.html", "default.htm"];
            app.UseFileServer(options);

            return app;
        }
    }

    public class DynamicFileProvider : IFileProvider
    {
        private FrozenDictionary<string, FileProviderInfo> host2path = new Dictionary<string, FileProviderInfo>().ToFrozenDictionary();

        private PhysicalFileProvider defaultFileProvider;
        private readonly IHttpContextAccessor httpContextAccessor;
        public DynamicFileProvider(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
            SearchTask();
        }

        public void SetDefault(string defaultPath)
        {
            if (Directory.Exists(defaultPath) == false)
            {
                Directory.CreateDirectory(defaultPath);
            }
            this.defaultFileProvider = new PhysicalFileProvider(defaultPath);
        }
        public List<FileProviderInfo> List()
        {
            return host2path.Values.ToList();
        }

        public void SearchTask()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Search();
                    await Task.Delay(60000);
                }
            });
        }
        public async Task Search()
        {
            host2path = Statics().Concat(Apps()).ToDictionary(c => c.Name, d => d).ToFrozenDictionary();
        }
        private List<FileProviderInfo> Statics()
        {
            try
            {
                string root = Path.GetFullPath("./statics");
                if (Directory.Exists(root) == false)
                {
                    Directory.CreateDirectory(root);
                }
                var result = Directory.GetDirectories(root).Select(c => new FileProviderInfo
                {
                    Name = Path.GetFileName(c),
                    Root = c,
                    FileProvider = new PhysicalFileProvider(c)
                }).ToList();

                return result;
            }
            catch (Exception)
            {
            }
            return [];
        }
        private List<FileProviderInfo> Apps()
        {
            try
            {
                string root = Path.GetFullPath("./apps");
                if (Directory.Exists(root) == false)
                {
                    return [];
                }

                return Directory.GetDirectories(root).Select(dir =>
                {
                    string path = System.IO.File.ReadAllText(Path.Join(dir, "manifest"))
                     .Split(Environment.NewLine)
                     .Select(line =>
                     {
                         string key = string.Empty, value = string.Empty;
                         if (string.IsNullOrWhiteSpace(line) == false)
                         {
                             int index = line.IndexOf('=');
                             if (index > 0)
                             {
                                 key = line.Substring(0, index).Trim();
                                 value = line.Substring(index + 1).Trim();

                             }
                         }
                         return (key, value);
                     })
                     .Where(c => c.key == "fnpackup").Select(c => c.value).FirstOrDefault();

                    return (dir, path);

                }).Where(c => string.IsNullOrWhiteSpace(c.path) == false).Select(c => new FileProviderInfo
                {
                    Name = Path.GetFileName(c.dir),
                    Root = Path.Join(c.dir, "target", c.path),
                    FileProvider = new PhysicalFileProvider(Path.Join(c.dir, "target", c.path))
                }).ToList();
            }
            catch (Exception)
            {
            }
            return [];
        }


        public IFileInfo GetFileInfo(string subpath)
        {
            (IFileProvider fileProvider, bool query) = GetFileProvider();
            if (query && subpath.IndexOf('/', 1) > 0)
            {
                subpath = subpath.Substring(subpath.IndexOf('/', 1));
            }
            return fileProvider.GetFileInfo(subpath);
        }
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            (IFileProvider fileProvider, bool query) = GetFileProvider();
            if (query && subpath.IndexOf('/', 1) > 0)
            {
                subpath = subpath.Substring(subpath.IndexOf('/', 1));
            }
            return fileProvider.GetDirectoryContents(subpath);
        }
        public IChangeToken Watch(string filter)
        {
            (IFileProvider fileProvider, bool query) = GetFileProvider();
            return fileProvider.Watch(filter);
        }

        private (IFileProvider fileProvider, bool query) GetFileProvider()
        {
            HttpContext httpContext = httpContextAccessor.HttpContext;
            string host = httpContext?.Request.Host.Host ?? string.Empty;
            string path = httpContext?.Request.Path ?? string.Empty;
            if (IPAddress.TryParse(host, out _) == false)
            {
                if (host2path.TryGetValue(host.Split('.')[0], out FileProviderInfo provider))
                {
                    return (provider.FileProvider, false);
                }
                Console.WriteLine(path);
                if (string.IsNullOrWhiteSpace(path) == false && host2path.TryGetValue(path.Split('/')[1], out provider))
                {
                    return (provider.FileProvider, true);
                }
            }
            return (defaultFileProvider, false);
        }


    }
    public sealed class FileProviderInfo
    {
        public string Name { get; set; }
        public string Root { get; set; }
        [JsonIgnore]
        public IFileProvider FileProvider { get; set; }
    }
}
