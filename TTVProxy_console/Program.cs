using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CryptoLibrary;
using SimpleLogger;
using P2pProxy;


namespace P2pProxy_console
{
    class Program
    {
        private static P2pProxyApp app;
        private static readonly string exeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        static void Main (string[] args)
        {
            //
            app = new P2pProxyApp(true, args.Contains("--debug"));
            app.Start();
            while (true)
            {
                System.Threading.Thread.Sleep(1000);
            }
            app.Notifyed -= AppOnNotifyed;
            app.Close();
        }

        private static Dictionary<string, string> GetParameters(string[] args)
        {
            int i = 0;
            Dictionary<string, string> res = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            int c = 0;
            while (i < args.Length)
            {
                if (args[i].Contains("-"))
                {
                    if (!res.ContainsKey(args[i]))
                    {
                        res.Add(args[i++], i < args.Length && !args[i].Contains("-") ? args[i++] : "");
                        c++;
                    }
                }
                else
                {
                    res.Add(c.ToString(), args[i++]);
                    c++;
                }
            }
            return res;
        }

        private static void Start()
        {
            app.Start();
        }

        private static void AppOnNotifyed(string text, TypeMessage icon)
        {
            if (Console.CursorLeft != 0)
                Console.Write("\r\n");
            string type = icon.ToString();
            Console.WriteLine("[{2}]{0} : {1}", DateTime.Now, text, type);
            
        }
    }
}
