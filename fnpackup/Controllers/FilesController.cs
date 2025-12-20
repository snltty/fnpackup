
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Text;

namespace fnpackup.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class FilesController : BaseController
    {
        private readonly string root = "./projects";
        public FilesController()
        {
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

            string result = CommandHelper.Execute($"fnpack", $" {sb.ToString()}", root, out string error);
            if (string.IsNullOrWhiteSpace(error) == false)
            {
                return error;
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
        [HttpPost]
        public async Task<string> Write([FromQuery]string path,[FromBody]string content)
        {
            path = Path.Join(root, path);
            if (Path.GetFullPath(path).StartsWith(Path.GetFullPath(root)) == false)
            {
                return $"Access to the path [{Path.GetFullPath(path)}] is denied";
            }
            await System.IO.File.WriteAllTextAsync(path, content.Replace("\r\n", "\n")).ConfigureAwait(false);
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
                return ["ÇëÑ¡ÔñÎÄ¼þ"];
            }

            foreach (var file in files)
            {
                var filePath = Path.Combine(path, file.FileName);
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
            Crlf2lf(name);

            string result = CommandHelper.Execute($"fnpack", $" build", Path.Join(root, name), out string error);
            if (string.IsNullOrWhiteSpace(error) == false)
            {
                return error;
            }

            return result;
        }

        private void Crlf2lf(string name)
        {
            string path = Path.Join(root, name, "cmd");
            var files = Directory.GetFiles(path).ToList();
            files.Add(Path.Join(root, name, "manifest"));
            foreach (var item in files)
            {
                string text = System.IO.File.ReadAllText(item);
                System.IO.File.WriteAllText(item, text.Replace("\r\n", "\n"));
            }

        }
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
}
