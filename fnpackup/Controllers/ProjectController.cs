using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace fnpackup.Controllers
{
    [ApiController]
    public class ProjectController : BaseController
    {
        private readonly string root = "./projects";

        private TaskCompletionSource tcs;
        private ConcurrentQueue<string> queue = new();

        private readonly IHttpClientFactory httpClientFactory;
        public ProjectController(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
            if (Directory.Exists(root) == false)
            {
                Directory.CreateDirectory(root);
            }
        }

        [HttpGet]
        [Route("/system/version")]
        public string Version()
        {
            return $"v{string.Join(".", Assembly.GetExecutingAssembly().GetName().Version.ToString().Split('.').Take(3))}";
        }

        [HttpPost]
        [Route("/project/build")]
        public async Task<string> Build(string name, string shell)
        {
            if (tcs != null && tcs.Task.IsCompleted == false)
            {
                return queue.TryDequeue(out string msg) ? msg : "已有构建任务在执行中，请稍后再试";
            }
            tcs = CommandHelper.ExecuteAsync(shell, Path.Join(root, name, "building"), (msg) =>
            {
                Console.WriteLine(msg);
                //queue.Enqueue(msg);
            });
            return string.Empty;
        }
        [HttpPost]
        [Route("/project/create")]
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
                System.IO.Directory.CreateDirectory(Path.Join(root, info.Name, "building", "platform"));
            }
            catch (Exception)
            {
            }

            return result;
        }
        [HttpGet]
        [Route("/project/exists")]
        public async Task<FileExistsInfo> Exists(string name)
        {
            return new FileExistsInfo
            {
                Docker = System.IO.Directory.Exists(Path.Join(root, name, "app", "docker")),
                UI = System.IO.Directory.Exists(Path.Join(root, name, "app", "ui")),
            };
        }
        [HttpPost]
        [Route("/project/pack")]
        public async Task<List<PackResultInfo>> Pack(string name, string platform = "", string server = "app/server")
        {
            Dictionary<string, string> manifest = await GetManifest(name).ConfigureAwait(false);
            if (manifest.ContainsKey("platform") == false)
            {
                return new List<PackResultInfo> { new PackResultInfo { FileName = string.Empty, Msg = "请将manifest中的arch转为platform" } };
            }

            Backup(name);

            List<PackResultInfo> result = new List<PackResultInfo>();
            if (string.IsNullOrWhiteSpace(platform))
            {
                result.Add(PackRename(name, manifest));
            }
            else
            {
                foreach (var item in platform.Split(','))
                {
                    try
                    {
                        manifest["platform"] = item;
                        await WritePlatform(name, manifest).ConfigureAwait(false);

                        CopyPlatform(name, item, server);

                        result.Add(PackRename(name, manifest));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
            return result;

        }
        [HttpPost]
        [Route("/platform/empty")]
        public async Task<bool> PlatformEmpty(string name, string platform)
        {
            string platformDir = Path.Join(root, name, "building", "platform", platform);
            if (Directory.Exists(platformDir) == false)
            {
                return true;
            }

            ClearFile(platformDir);
            return DirAreEmpty(platformDir);
        }
        private PackResultInfo PackRename(string name, Dictionary<string, string> manifest)
        {
            string msg = CommandHelper.Execute($"fnpack", $" build", [], Path.Join(root, name), out string error);
            if (string.IsNullOrWhiteSpace(error) == false)
            {
                return new PackResultInfo { FileName = manifest["platform"], Msg = error };
            }
            if (msg.Contains("Packing successfully") == false)
            {
                return new PackResultInfo { FileName = manifest["platform"], Msg = msg };
            }
            string newName = $"{name}-{manifest["version"]}-{manifest["platform"]}";
            System.IO.File.Move(Path.Join(root, name, $"{name}.fpk"), Path.Join(root, name, $"{newName}.fpk"), true);
            return new PackResultInfo { FileName = $"{newName}.fpk", Msg = msg };
        }
        private void CopyPlatform(string name, string platform, string dist)
        {
            string platformDir = Path.Join(root, name, "building", "platform", platform);
            if (DirAreEmpty(platformDir) == false)
            {
                ClearFile(Path.Join(root, name, dist));
                CopyDir(platformDir, Path.Join(root, name, dist));
            }
        }
        private void Backup(string name)
        {
            if (System.IO.File.Exists(Path.Join(root, name, "building", "manifest")) == false)
            {
                if (Directory.Exists(Path.Join(root, name, "building")) == false)
                {
                    Directory.CreateDirectory(Path.Join(root, name, "building"));
                }
                System.IO.File.Copy(Path.Join(root, name, "manifest"), Path.Join(root, name, "building", "manifest"));
            }
        }
        private bool DirAreEmpty(string path)
        {
            return Directory.Exists(path) == false || (Directory.GetFiles(path).Length == 0 && Directory.GetDirectories(path).Length == 0);
        }
        private void ClearFile(string path)
        {
            if (Directory.Exists(path) == false)
            {
                return;
            }
            foreach (var item in Directory.GetFiles(path))
            {
                System.IO.File.Delete(item);
            }
            foreach (var item in Directory.GetDirectories(path))
            {
                ClearFile(item);
                Directory.Delete(item);
            }
        }
        private void CopyDir(string sourceDir, string destDir)
        {
            if (Directory.Exists(sourceDir) == false)
            {
                return;
            }

            Directory.CreateDirectory(destDir);
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFile = Path.Combine(destDir, Path.GetFileName(file));
                System.IO.File.Copy(file, destFile, true);
            }
            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                var destSubDir = Path.Combine(destDir, Path.GetFileName(directory));
                CopyDir(directory, destSubDir);
            }
        }
        private async Task<Dictionary<string, string>> GetManifest(string name)
        {
            string str = await System.IO.File.ReadAllTextAsync(Path.Join(root, name, "manifest")).ConfigureAwait(false);
            return str.Split("\n").Select(c =>
            {
                int index = c.IndexOf('=');
                string key = string.Empty, value = string.Empty;
                if (index > 0)
                {
                    key = c.Substring(0, index);
                    value = c.Substring(index + 1);
                }
                return new string[] { key, value };

            }).ToDictionary(k => k[0].Trim(), v => v.Length > 1 ? v[1].Trim() : string.Empty);
        }
        private async Task WritePlatform(string name, Dictionary<string, string> dic)
        {
            string content = string.Join("\n", dic.Where(c => string.IsNullOrWhiteSpace(c.Key) == false).Select(c => $"{c.Key}={c.Value}"));
            await System.IO.File.WriteAllTextAsync(Path.Join(root, name, "manifest"), content).ConfigureAwait(false);
        }


        [HttpGet]
        [Route("/file/list")]
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
            if (Path.GetFullPath(path).StartsWith(Path.GetFullPath(root)) == false)
            {
                return new FilePageInfo
                {
                    P = p,
                    Ps = ps,
                    Count = 0,
                };
            }

            if (Directory.Exists(path) == false)
            {
                return new FilePageInfo
                {
                    P = p,
                    Ps = ps,
                    Count = -1,
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
                    Size = c is System.IO.FileInfo fi ? fi.Length : 0,
                    Name = c.Name,
                    If = c is System.IO.FileInfo,
                    Lwt = c.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    Ct = c.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    Docker = System.IO.File.Exists(Path.Join(fullPath, c.FullName.Replace(fullPath, "").Split(Path.DirectorySeparatorChar)[1], "app/docker/docker-compose.yaml"))
                }).ToList()
            };
        }

        [HttpPost]
        [Route("/file/create")]
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
        [Route("/file/delete")]
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
        [Route("/file/rename")]
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
                System.IO.File.Move(path, path1, true);
            }
            else
            {
                System.IO.Directory.Move(path, path1);
            }
            return string.Empty;
        }
        [HttpPost]
        [Route("/file/copy")]
        public async Task<string> CopyFile(string path, string path1, bool f = true)
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
                System.IO.File.Copy(path, path1, true);
            }
            else
            {
                string error = string.Empty;
                if (OperatingSystem.IsWindows())
                {
                    CommandHelper.Execute("cmd.exe", string.Empty, [
                    $"xcopy \"{path}\" \"{path1}\" /E /I /H /Y",
                    ], root, out error);
                }
                else if (OperatingSystem.IsLinux())
                {
                    CommandHelper.Execute("/bin/bash", string.Empty, [
                    $"cp -rf \"{path}/.\" \"{path1}\"",
                    ], root, out error);
                }
                if (string.IsNullOrWhiteSpace(error) == false)
                {
                    return error;
                }
            }
            return string.Empty;
        }
        [HttpGet]
        [Route("/file/read")]
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
        [Route("/file/write")]
        public async Task<string> Write(FileWriteInfo info)
        {
            string path = Path.Join(root, info.Path);
            if (Path.GetFullPath(path).StartsWith(Path.GetFullPath(root)) == false)
            {
                return $"Access to the path [{Path.GetFullPath(path)}] is denied";
            }
            if (Directory.Exists(Path.GetDirectoryName(path)) == false)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            await System.IO.File.WriteAllTextAsync(path, info.Content.Replace("\r\n", "\n")).ConfigureAwait(false);
            return string.Empty;
        }
        [HttpGet]
        [Route("/file/img")]
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
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue, ValueLengthLimit = int.MaxValue)]
        [Route("/file/upload")]
        public async Task<List<string>> Upload([FromQuery] string path, [FromQuery] string fpk, List<IFormFile> files)
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
                if (Path.GetExtension(filePath) == ".fpk" && string.IsNullOrWhiteSpace(fpk) == false)
                {
                    await DecompressFpk(file);
                }
                else
                {
                    if (Directory.Exists(Path.GetDirectoryName(filePath)) == false)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    }

                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);
                }
            }

            return [];
        }
        private async Task DecompressFpk(IFormFile file)
        {

            string dir = Path.Join(root, Path.GetFileNameWithoutExtension(file.FileName));
            string path = Path.Join(dir, Path.GetFileName(file.FileName));
            if (Directory.Exists(dir))
            {
                return;
            }
            Directory.CreateDirectory(dir);

            try
            {
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                using var stream = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(stream);

                CommandHelper.Execute($"tar", $" -xvf {Path.GetFileName(file.FileName)}", [], dir, out string error);
                Directory.CreateDirectory(Path.Join(dir, "app"));
                CommandHelper.Execute($"tar", $" -xzvf app.tgz -C app", [], dir, out error);


                if (Directory.Exists(Path.Join(dir, "building")) == false)
                    System.IO.Directory.CreateDirectory(Path.Join(dir, "building"));
                if (Directory.Exists(Path.Join(dir, "building", "platform")) == false)
                    System.IO.Directory.CreateDirectory(Path.Join(dir, "building", "platform"));
            }
            catch (Exception)
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }
            finally
            {
                if (System.IO.File.Exists(Path.Join(dir, "app.tgz")))
                {
                    System.IO.File.Delete(Path.Join(dir, "app.tgz"));
                }
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
            try
            {
                Dictionary<string, string> manifest = await GetManifest(Path.GetFileNameWithoutExtension(file.FileName)).ConfigureAwait(false);
                if (manifest.TryGetValue("appname", out string appname) && appname != Path.GetFileNameWithoutExtension(file.FileName))
                {
                    string newDir = Path.Join(root, appname);
                    Directory.Move(dir, newDir);
                }
            }
            catch (Exception)
            {
            }
        }

        [HttpGet]
        [Route("/file/download")]
        public async Task<IActionResult> Download(string path)
        {
            path = Path.Join(root, path);
            if (System.IO.File.Exists(path))
            {
                var stream = new FileStream(path, FileMode.Open);
                return File(stream, "application/octet-stream", Path.GetFileName(path));
            }
            if (Directory.Exists(path) == false)
            {
                return Content("文件或文件夹不存在，如果是fpk，那可能项目文件夹名与实际appname不一致", "text/plain");
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

                string filename = Path.GetFileName(path);
                if (string.IsNullOrWhiteSpace(filename) || filename == ".")
                {
                    filename = $"fnpackup-{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}";
                }
                return File(memoryStream.ToArray(),
                    "application/zip",
                    $"{filename}.zip");
            }
        }

        [HttpGet]
        [Route("/app/list")]
        public async Task<AppCenterRespInfo> AppCenter(string name,string names)
        {
            try
            {
                string host = "http://localhost:5666/";
                string token = Request.Cookies["fnos-token"];

                if (string.IsNullOrWhiteSpace(token))
                {
                    return new AppCenterRespInfo
                    {
                        Code = 1,
                        Msg = "未能获取到飞牛token，无法查询应用列表"
                    };
                }

                string[] _names = (names ?? string.Empty).Split(':');
                if (_names.Length > 1)
                {
                    return await Search(host, token, _names);
                }
                return await Search(host, token, name);
            }
            catch (Exception ex)
            {
                return new AppCenterRespInfo
                {
                    Code = 1,
                    Msg = $"{ex}"
                };
            }
        }
        private async Task<AppCenterRespInfo> Search(string host, string token, string[] names)
        {
            AppCenterRespInfo finalResp = await Search(host, token, string.Empty);
            finalResp.Data.List = finalResp.Data.List.Where(c => names.Contains(c.AppName)).ToList();
            return finalResp;
        }
        private async Task<AppCenterRespInfo> Search(string host, string token, string name)
        {
            using HttpClient client = httpClientFactory.CreateClient();

            string url = $"{host}app-center/v1/app/list?language=zh";
            if (string.IsNullOrWhiteSpace(name) == false)
            {
                url = $"{host}app-center/v1/app/search?keyword={name}&language=zh";
            }
            client.DefaultRequestHeaders.Add("Authorization", $"trim {token}");

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
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            return JsonSerializer.Deserialize<AppCenterRespInfo>(str, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,  // 忽略大小写
            });
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
        }
    }

    public sealed class PackResultInfo
    {
        public string FileName { get; set; }
        public string Msg { get; set; }
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

        public long Size { get; set; }
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
