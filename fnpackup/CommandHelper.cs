using System.Diagnostics;
using System.Text;
namespace fnpackup
{
    public sealed class CommandHelper
    {
        public static string Execute(string fileName, string arg, string root, out string error)
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
