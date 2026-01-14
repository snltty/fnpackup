using System.Diagnostics;
using System.Text;
namespace fnpackup
{
    public sealed class CommandHelper
    {
        public static TaskCompletionSource ExecuteAsync(string fileName, string root, Action<string> msgCallback)
        {
            Process proc = new Process();
            proc.StartInfo.WorkingDirectory = Path.GetFullPath(root);
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = Path.GetFullPath(Path.Join(root, fileName));
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.Verb = "runas";

            proc.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            proc.StartInfo.StandardErrorEncoding = Encoding.UTF8;

            proc.OutputDataReceived += (sender, args) =>
            {
                Console.WriteLine(args.Data);
            };
            proc.ErrorDataReceived += (sender, args) =>
            {
                Console.WriteLine(args.Data);
            };
           
            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            TaskCompletionSource tcs = new TaskCompletionSource();
            proc.Exited += (sender, args) =>
            {
                proc.Close();
                proc.Dispose();
                tcs.SetResult();
            };
            proc.EnableRaisingEvents = true;
            if (proc.HasExited)
            {
                tcs.TrySetResult();
            }

            return tcs;

        }

        public static string Execute(string fileName, string arg, string[] commands, string root, out string error)
        {
            using Process proc = new Process();
            proc.StartInfo.WorkingDirectory = Path.GetFullPath(root);
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.Arguments = arg;
            proc.StartInfo.Verb = "runas";

            proc.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            proc.StartInfo.StandardErrorEncoding = Encoding.UTF8;
            proc.Start();

            if (commands.Length > 0)
            {
                for (int i = 0; i < commands.Length; i++)
                {
                    proc.StandardInput.WriteLine(commands[i]);
                }
            }

            proc.StandardInput.AutoFlush = true;
            proc.StandardInput.WriteLine("exit");
            proc.StandardInput.Close();


            string output = proc.StandardOutput.ReadToEnd();
            error = proc.StandardError.ReadToEnd();

            proc.WaitForExit();
            proc.Close();
            proc.Dispose();

            return output;
        }
    }
}
