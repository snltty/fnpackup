
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace fnpackup.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class FilesController : BaseController
    {
        private readonly string root = "./projects";

        private readonly IHttpClientFactory httpClientFactory;
        public FilesController(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        public string Create([FromBody] ProjectCreateInfo info)
        {
            StringBuilder sb = new StringBuilder($"create {info.Name}");
            if (info.Docker)
            {
                sb.Append($" -t docker");
            }
            if (info.Ui == false)
            {
                sb.Append($" --without-ui true");
            }

            string result = CommandHelper.Execute($"fnpack", $" {sb.ToString()}", [], root, out string error);
            if (string.IsNullOrWhiteSpace(error) == false)
            {
                return error;
            }

            try
            {
                System.IO.Directory.CreateDirectory(Path.Join(root, info.Name, "building"));
                System.IO.Directory.CreateDirectory(Path.Join(root, info.Name, "building", "dist"));
                using var fs = System.IO.File.Create(Path.Join(root, info.Name, "building", "building"));
            }
            catch (Exception)
            {
            }

            return result;
        }

        [HttpGet]
        public FilePageInfo Get(string path = "", int p = 1, int ps = 20)
        {
            return SearchProjects(path, p, ps);
        }
        private FilePageInfo SearchProjects(string path = "", int p = 1, int ps = 20)
        {
            if (Directory.Exists(root) == false)
            {
                Directory.CreateDirectory(root);
            }

            path = Path.Join(root, path);
            if (Directory.Exists(path) == false)
            {
                return new FilePageInfo
                {
                    P = p,
                    Ps = ps,
                    Count = 0,
                };
            }

            string fullPath = Path.GetFullPath(root);
            var list = new DirectoryInfo(path).GetFileSystemInfos().OrderBy(item => item is System.IO.FileInfo).ThenBy(item => item.Name);
            return new FilePageInfo
            {
                P = p,
                Ps = ps,
                Count = list.Count(),
                List = list.Skip((p - 1) * ps).Take(ps).Select(c => new FileInfo
                {
                    Name = c.Name,
                    If = c is System.IO.FileInfo,
                    Lwt = c.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    Ct = c.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    Docker = System.IO.File.Exists(Path.Join(fullPath, c.FullName.Replace(fullPath, "").Split(Path.DirectorySeparatorChar)[1], "app/docker/docker-compose.yaml"))
                }).ToList()
            };
        }

        [HttpPost]
        public async Task<string> CreateFile(string path, bool f = true)
        {
            path = Path.Join(root, path);
            if (Path.GetFullPath(path).StartsWith(Path.GetFullPath(root)) == false)
            {
                return $"Access to the path [{Path.GetFullPath(path)}] is denied";
            }
            if (f)
            {
                using var fs = System.IO.File.Create(path);
            }
            else
            {
                System.IO.Directory.CreateDirectory(path);
            }
            return string.Empty;
        }
        [HttpPost]
        public async Task<string> DelFile(string path, bool f = true)
        {
            path = Path.Join(root, path);
            if (Path.GetFullPath(path).StartsWith(Path.GetFullPath(root)) == false)
            {
                return $"Access to the path [{Path.GetFullPath(path)}] is denied";
            }
            if (f)
            {
                System.IO.File.Delete(path);
            }
            else
            {
                System.IO.Directory.Delete(path, true);
            }
            return string.Empty;
        }
        [HttpPost]
        public async Task<string> RenameFile(string path, string path1, bool f = true)
        {
            path = Path.Join(root, path);
            path1 = Path.Join(root, path1);
            if (Path.GetFullPath(path).StartsWith(Path.GetFullPath(root)) == false)
            {
                return $"Access to the path [{Path.GetFullPath(path)}] is denied";
            }
            if (Path.GetFullPath(path1).StartsWith(Path.GetFullPath(root)) == false)
            {
                return $"Access to the path [{Path.GetFullPath(path1)}] is denied";
            }
            if (f)
            {
                System.IO.File.Move(path, path1);
            }
            else
            {
                System.IO.Directory.Move(path, path1);
            }
            return string.Empty;
        }
        [HttpGet]
        public async Task<string> Read(string path)
        {
            path = Path.Join(root, path);
            if (Path.GetFullPath(path).StartsWith(Path.GetFullPath(root)) == false || System.IO.File.Exists(path) == false)
            {
                return string.Empty;
            }

            return await System.IO.File.ReadAllTextAsync(path).ConfigureAwait(false);
        }
        [HttpGet]
        public async Task<IActionResult> Img(string path)
        {
            path = Path.Join(root, path);
            if (Path.GetFullPath(path).StartsWith(Path.GetFullPath(root)) == false || System.IO.File.Exists(path) == false)
            {
                return null;
            }

            return File(await System.IO.File.ReadAllBytesAsync(path).ConfigureAwait(false), "image/png");
        }

        [HttpPost]
        public async Task<string> Write(FileWriteInfo info)
        {
            string path = Path.Join(root, info.Path);
            if (Path.GetFullPath(path).StartsWith(Path.GetFullPath(root)) == false)
            {
                return $"Access to the path [{Path.GetFullPath(path)}] is denied";
            }
            await System.IO.File.WriteAllTextAsync(path, info.Content.Replace("\r\n", "\n")).ConfigureAwait(false);
            return string.Empty;
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<List<string>> Upload([FromQuery] string path, List<IFormFile> files)
        {
            path = Path.Join(root, path);
            if (Path.GetFullPath(path).StartsWith(Path.GetFullPath(root)) == false)
            {
                return [$"Access to the path [{Path.GetFullPath(path)}] is denied"];
            }

            if (files == null || files.Count == 0)
            {
                return ["请选择文件"];
            }

            foreach (var file in files)
            {
                var filePath = System.IO.File.Exists(path) ? path : Path.Combine(path, file.FileName);
                if (Directory.Exists(Path.GetDirectoryName(filePath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }

                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
            }

            return [];
        }

        [HttpGet]
        public async Task<IActionResult> Download(string path)
        {
            path = Path.Join(root, path);
            if (System.IO.File.Exists(path))
            {
                var stream = new FileStream(path, FileMode.Open);
                return File(stream, "application/octet-stream", Path.GetFileName(path));
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

                    foreach (var file in files)
                    {
                        var relativePath = Path.GetRelativePath(path, file);
                        var entry = archive.CreateEntry(relativePath, CompressionLevel.Fastest);
                        using (var entryStream = entry.Open())
                        {
                            using (var fileStream = System.IO.File.OpenRead(file))
                            {
                                await fileStream.CopyToAsync(entryStream);
                            }
                        }
                    }
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                return File(memoryStream.ToArray(),
                    "application/zip",
                    $"{Path.GetFileName(path)}.zip");
            }
        }

        [HttpPost]
        public async Task<string> Build(string name)
        {
            string result = CommandHelper.Execute($"fnpack", $" build", [], Path.Join(root, name), out string error);
            if (string.IsNullOrWhiteSpace(error) == false)
            {
                return error;
            }

            return result;
        }


        [HttpGet]
        public async Task<FileExistsInfo> Exists(string name)
        {
            return new FileExistsInfo
            {
                Docker = System.IO.Directory.Exists(Path.Join(root, name, "app", "docker")),
                UI = System.IO.Directory.Exists(Path.Join(root, name, "app", "ui")),
            };
        }

        [HttpGet]
        public async Task<AppCenterRespInfo> AppCenter(string name)
        {
            try
            {
                string host =  $"{(Request.Host.Host=="localhost"? "172.23.212.86":Request.Host.Host)}:5666";
                string token = $"trim {(string.IsNullOrWhiteSpace(Request.Cookies["fnos-token"]) ? "T+S+Oy4tTmk0VuIgCaMtAn4jtfREO4FkYDQ17kuTsX0=" : Request.Cookies["fnos-token"])}";

                using var client = httpClientFactory.CreateClient();

                string url = $"http://{host}/app-center/v1/app/list?language=zh";
                if (string.IsNullOrWhiteSpace(name) == false)
                {
                    url = $"http://{host}/app-center/v1/app/search?keyword={name}&language=zh";
                }
                client.DefaultRequestHeaders.Add("Authorization", token);
                HttpResponseMessage resp = await client.GetAsync(url);

                if (resp.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return new AppCenterRespInfo
                    {
                        Code = 1,
                        Msg = $"http code {resp.StatusCode}"
                    };
                }
                string str = await resp.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<AppCenterRespInfo>(str, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,  // 忽略大小写
                });
            }
            catch (Exception ex)
            {
                return new AppCenterRespInfo
                {
                    Code = 1,
                    Msg = ex.Message
                };
            }
        }
    }

    public sealed class FileWriteInfo
    {
        public string Path { get; set; }
        public string Content { get; set; }
    }
    public sealed class ProjectCreateInfo
    {
        public string Name { get; set; }
        public bool Docker { get; set; }
        public bool Ui { get; set; }
    }

    public sealed class FilePageInfo
    {
        public int P { get; set; }
        public int Ps { get; set; }
        public int Count { get; set; }
        public List<FileInfo> List { get; set; } = [];
    }
    public sealed class FileInfo
    {
        public string Name { get; set; }
        public bool If { get; set; }
        public string Ct { get; set; }
        public string Lwt { get; set; }
        public bool Docker { get; set; }
    }

    public sealed class FileExistsInfo
    {
        public bool Docker { get; set; }
        public bool UI { get; set; }
    }

    public sealed class AppCenterRespInfo
    {
        public int Code { get; set; }
        public AppCenterDataInfo Data { get; set; } = new AppCenterDataInfo();
        public string Msg { get; set; }


    }
    public sealed class AppCenterDataInfo
    {
        public List<AppCenterItemInfo> List { get; set; } = [];
    }
    public sealed class AppCenterItemInfo
    {
        public string AppName { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
    }
}
