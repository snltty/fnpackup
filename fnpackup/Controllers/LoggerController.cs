using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;

namespace fnpackup.Controllers
{
    public class LoggerController : BaseController
    {
        private readonly List<LoggerFileInfo> files = [
            new LoggerFileInfo { Name = "fnpackup.debug", Type = LoggerType.Debug },
            new LoggerFileInfo { Name = "fnpackup.info", Type = LoggerType.Info },
            new LoggerFileInfo { Name = "fnpackup.warning", Type = LoggerType.Warning },
            new LoggerFileInfo { Name = "fnpackup.error", Type = LoggerType.Error },
            new LoggerFileInfo { Name = "fnpackup.fatal", Type = LoggerType.Fatal },
        ];
        private readonly List<LoggerInfo> list = [];
        private string vol = string.Empty;

        public LoggerController()
        {
            InitLogger();
            CreateLogger();
            AppDomain.CurrentDomain.ProcessExit += (sender, e) => DeleteLogger();
            Console.CancelKeyPress += (sender, e) => DeleteLogger();
        }

        [HttpGet]
        public LoggerPageInfo List(string text, int p = 1, int ps = 10, LoggerType type = LoggerType.None)
        {
            IEnumerable<LoggerInfo> _list = list;

            if (type != LoggerType.None)
            {
                _list = _list.Where(x => x.Type == type);
            }
            if (string.IsNullOrWhiteSpace(text) == false)
            {
                _list = _list.Where(x => x.Msg.Contains(text));
            }

            return new LoggerPageInfo
            {
                P = p,
                Ps = ps,
                Count = _list.Count(),
                List = _list.Skip((p - 1) * ps).Take(ps).ToList(),
            };
        }

        private void InitLogger()
        {
            try
            {
                vol = Directory.ResolveLinkTarget("/app/apps/fnpackup/target", true).FullName.Split('/')[1];
            }
            catch (Exception)
            {
            }
            if (string.IsNullOrWhiteSpace(vol))
            {
                Console.WriteLine("没找到logger根目录.");
            }
        }
        private void DeleteLogger()
        {
            if (string.IsNullOrWhiteSpace(vol))
            {
                return;
            }
            foreach (var file in files)
            {
                try
                {
                    System.IO.File.Delete(Path.Combine(vol, file.Name));
                }
                catch (Exception)
                {
                }
            }
        }
        private void CreateLogger()
        {
            if (string.IsNullOrWhiteSpace(vol))
            {
                return;
            }
            foreach (var file in files)
            {
                string path = Path.Combine(vol, file.Name);
                try
                {
                    System.IO.File.Delete(path);
                }
                catch (Exception)
                {
                }
                try
                {
                    Process.Start("mkfifo", path).WaitForExit();
                    Process.Start("chmod", $"666 {path}").WaitForExit();
                    _ = ReadLoggerAsync(path, file.Type).ConfigureAwait(false);
                }
                catch (Exception)
                {
                }
            }
        }
        private async Task ReadLoggerAsync(string path, LoggerType type)
        {
            while (true)
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fs, Encoding.UTF8);
                string message = await reader.ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(message) == false)
                {
                    list.Add(new LoggerInfo { Type = type, Msg = message });
                    if (list.Count > 10000)
                    {
                        list.RemoveAt(0);
                    }
                }
            }
        }
    }

    public sealed class LoggerPageInfo
    {
        public int P { get; set; }
        public int Ps { get; set; }
        public int Count { get; set; }
        public List<LoggerInfo> List { get; set; } = [];
    }
    public sealed class LoggerInfo
    {
        public LoggerType Type { get; set; }
        public string Msg { get; set; }
    }
    public sealed class LoggerFileInfo
    {
        public string Name { get; set; }
        public LoggerType Type { get; set; }
    }

    public enum LoggerType
    {
        None = 0,
        Debug,
        Info,
        Warning,
        Error,
        Fatal,
    }
}
