using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CyanLauncherManager
{
    static class Program
    {
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
        static public bool isIcon(string file)
        {
            if (file.Contains(".ico")) return true;
            else return false;
        }
        static public bool isLink(string file)
        {
            if (file.Contains(".lnk")) return true;
            else return false;
        }
        static public bool isText(string file)
        {
            if (file.Contains(".txt")) return true;
            else return false;
        }

        static public void KillProc(string name)
        {
            Process[] processCollection = Process.GetProcesses();
            foreach (Process p in processCollection) if (p.ProcessName == name) p.Kill();
            System.Threading.Thread.Sleep(100);
        }

        static public List<string> GetApplicationNames(string path, bool only_name = false)
        {
            List<string> output = new List<string>();
            foreach (string directory in Directory.EnumerateDirectories(path))
            {
                if (directory != Path.Combine(path, "__Icons__") && directory != Path.Combine(path, "__Matrix__"))
                {
                    output.Add(only_name ? Path.GetFileName(directory) : directory);
                }
            }
            return output;
        }

        static public string GenerateCausualString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
