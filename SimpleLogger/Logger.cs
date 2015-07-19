using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace SimpleLogger
{
    public class Logger
    {
        private readonly bool _dublConsole;
        private bool _opened;
        private StreamWriter sw;

        public Logger(string path, bool console = false)
        {
            _dublConsole = console;
            _opened = OpenFile(path);
        }

        private bool OpenFile(string path)
        {
            try
            {
                if (_opened)
                    Close();
                sw = new StreamWriter(path, true) {AutoFlush = true};
                _opened = true;
            }
            catch (Exception)
            {
                Random rand = new Random(DateTime.Now.Millisecond);
                path = Path.GetFileNameWithoutExtension(path) + "_" + rand.Next(ushort.MaxValue) +
                       Path.GetExtension(path);
                return OpenFile(path);
            }
            return true;
        }

        public void Close()
        {
            lock (sw)
            {
                sw.Close();
                sw.Dispose();
                _opened = false;
            }
        }

        public void Write(string value, TypeMessage type)
        {
            if (sw == null || !_opened)
                return;
            ThreadPool.QueueUserWorkItem(state =>
            {
                lock (sw)
                {
                    string res = String.Format("[{0:HH:mm:ss.fff}] {1}: {2}", DateTime.Now, type, value);
                    if (_dublConsole)
                        Console.WriteLine(res);
                    try
                    {
                        sw.WriteLine(res);
                    }
                    catch
                    {
                        Close();
                    }
                }
            });
        }
    }

    [ObfuscationAttribute]
    public enum TypeMessage
    {
        Info, Warning, Error, Critical
    }
}
