using Microsoft.AspNetCore.Mvc;
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
            if (result.Contains("Success! Created"))
            {
                if (info.WizardInstall) System.IO.File.Create(Path.Join(root, info.Name, "wizard", "install"));
                if (info.WizardUninstall) System.IO.File.Create(Path.Join(root, info.Name, "wizard", "uninstall"));
                if (info.WizardUpgrade) System.IO.File.Create(Path.Join(root, info.Name, "wizard", "upgrade"));
                if (info.WizardConfig) System.IO.File.Create(Path.Join(root, info.Name, "wizard", "config"));
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
            if (Path.GetFullPath(path).StartsWith(Path.GetFullPath(root)) == false)
            {
                return string.Empty;
            }

            return await System.IO.File.ReadAllTextAsync(path).ConfigureAwait(false);
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
    }


    public sealed class ProjectCreateInfo
    {
        public string Name { get; set; }
        public bool Docker { get; set; }
        public bool Ui { get; set; }
        public bool WizardInstall { get; set; }
        public bool WizardUninstall { get; set; }
        public bool WizardUpgrade { get; set; }
        public bool WizardConfig { get; set; }
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
