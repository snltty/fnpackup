using Microsoft.AspNetCore.HttpOverrides;
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

#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            builder.Services.AddControllers();
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            builder.Services.AddHttpClient();
            builder.Services.AddEndpointsApiExplorer();
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


            app.UseCors("AllowAll");
            app.UseRouting();

            app.UseDynamicStaticFile();
            app.MapControllers();

            app.Run();
        }
    }

    public static class StaticFileExtends
    {
        public static IServiceCollection AddDynamicStaticFile(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddSingleton<DynamicFileProvider>();

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;
                options.ForwardLimit = null;
                options.KnownProxies.Clear();
            });

            return services;
        }
        public static WebApplication UseDynamicStaticFile(this WebApplication app)
        {
            app.UseForwardedHeaders();

            DynamicFileProvider dfp = app.Services.GetService<DynamicFileProvider>();
            dfp.SetDefault(Path.Combine(app.Environment.ContentRootPath, "web"));
            FileServerOptions options = new FileServerOptions
            {
                FileProvider = dfp,
                EnableDefaultFiles = true,
                EnableDirectoryBrowsing = false
            };
            options.DefaultFilesOptions.DefaultFileNames = ["index.html", "index.htm", "default.html", "default.htm"];
            app.UseFileServer(options);
            app.UseDefaultFiles(options.DefaultFilesOptions);

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
                    string manifest = Path.Join(dir, "manifest");
                    string path = File.Exists(manifest) == false ? string.Empty : File.ReadAllText(manifest)
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
            if (query)
            {
                subpath = SplitSubPath(subpath);
            }
            return fileProvider.GetFileInfo(subpath);
        }
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            (IFileProvider fileProvider, bool query) = GetFileProvider();
            if (query)
            {
                subpath = SplitSubPath(subpath);
            }
            return fileProvider.GetDirectoryContents(subpath);
        }
        public IChangeToken Watch(string filter)
        {
            (IFileProvider fileProvider, bool query) = GetFileProvider();
            return fileProvider.Watch(filter);
        }

        private string SplitSubPath(string subpath)
        {
            if (subpath.IndexOf('/', 1) > 0)
            {
                return subpath[subpath.IndexOf('/', 1)..];
            }
            return "/";
        }
        private (IFileProvider fileProvider, bool query) GetFileProvider()
        {
            HttpContext httpContext = httpContextAccessor.HttpContext;
            string host = httpContext?.Request.Host.Host ?? string.Empty;
            string path = httpContext?.Request.Path ?? string.Empty;

            FileProviderInfo provider;
            if (IPAddress.TryParse(host, out _) == false)
            {
                if (host2path.TryGetValue(host.Split('.')[0], out provider))
                {
                    return (provider.FileProvider, false);
                }
            }
            if (string.IsNullOrWhiteSpace(path) == false && host2path.TryGetValue(path.Split('/')[1], out provider))
            {
                return (provider.FileProvider, true);
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
