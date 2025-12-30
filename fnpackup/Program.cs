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
            string root = "./statics";
            if(Directory.Exists(root) == false)
            {
                Directory.CreateDirectory(root);
            }

            return Directory.GetDirectories(root).Select(c => new FileProviderInfo
            {
                Name = c,
                Root = Path.GetFullPath(Path.Join(root, c)),
                FileProvider = new PhysicalFileProvider(Path.GetFullPath(Path.Join(root, c)))
            }).ToList();
        }
        private List<FileProviderInfo> Apps()
        {
            string root = "./apps";
            if (Directory.Exists(root) == false)
            {
                return [];
            }

            return Directory.GetDirectories(root).Select(dirName =>
            {
                string path = System.IO.File.ReadAllText(Path.GetFullPath(Path.Join(root, dirName, "manifest")))
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

                return (dirName, path);

            }).Where(c => string.IsNullOrWhiteSpace(c.path) == false).Select(c => new FileProviderInfo
            {
                Name = c.dirName,
                Root = Path.GetFullPath(Path.Join(root, "target", c.path)),
                FileProvider = new PhysicalFileProvider(Path.GetFullPath(Path.Join(root, "target", c.path)))
            }).ToList();
        }


        public IFileInfo GetFileInfo(string subpath)
        {
            return GetFileProvider().GetFileInfo(subpath);
        }
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return GetFileProvider().GetDirectoryContents(subpath);
        }
        public IChangeToken Watch(string filter)
        {
            return GetFileProvider().Watch(filter);
        }

        private IFileProvider GetFileProvider()
        {
            HttpContext httpContext = httpContextAccessor.HttpContext;
            string host = httpContext?.Request.Host.Host ?? string.Empty;
            string path = httpContext?.Request.Path ?? string.Empty;
            if (IPAddress.TryParse(host, out _) == false)
            {
                if (host2path.TryGetValue(host.Split('.')[0], out FileProviderInfo provider))
                {
                    return provider.FileProvider;
                }
                if (string.IsNullOrWhiteSpace(path) == false && host2path.TryGetValue(path.Split('/')[0], out provider))
                {
                    return provider.FileProvider;
                }
            }
            return defaultFileProvider;
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
