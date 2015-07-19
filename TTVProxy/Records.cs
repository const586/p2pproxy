using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;
using P2pProxy.Http.Content;

namespace P2pProxy
{
    [ObfuscationAttribute]
    public class Records
    {
        [ObfuscationAttribute]
        public string Name { get; set; }
        [ObfuscationAttribute]
        public DateTime Start;
        [ObfuscationAttribute]
        public DateTime End;
        public RecordStatus Status
        {
            get { return _status; }
            set
            {
                if (value == RecordStatus.Finished && _status != value)
                {
                    _status = value;
                    RaiseNotify();
                }
                _status = value;
            }
        }
        [ObfuscationAttribute]
        public int TorrentId;
        [ObfuscationAttribute]
        public string Id { get; set; }
        [ObfuscationAttribute]
        public string BTime { get { return Start.ToString("dd-MM-yy HH:mm:ss"); } }
        [ObfuscationAttribute]
        public string ETime { get { return End.ToString("dd-MM-yy HH:mm:ss"); } }

        public static readonly string MIME = "video/mpeg";
        

        public delegate void OnFinish(Records sender);

        public event OnFinish Finished;

        private readonly string _outpath;
        private readonly string _host;
        private bool _inProgress;
        private RecordStatus _status;

        public Uri Path
        {
            get { return new Uri(_outpath + "/" + Id + ".ts", UriKind.Absolute); }
        }

        public bool Exists
        {
            get { return File.Exists(Path.LocalPath); }
        }

        public Stream GetStream()
        {
            if (!Exists)
                return Stream.Null;
            return File.Open(Path.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public Records(string host, string path)
        {
            _host = host;
            _outpath = path;
        }

        private void RaiseNotify()
        {
            if (Finished != null) Finished(this);
        }


        public void Record()
        {
            if (_inProgress)
                return;
            _inProgress = true;
            if (Status == RecordStatus.Pause)
            {
                Status = RecordStatus.Start;
                return;
            }

            ThreadPool.QueueUserWorkItem(o =>
            {
                Stream stream;
                try
                {
                    stream = WebRequest.Create(String.Format("{0}{1}?id={2}", _host, ChannelContentProvider.PLAY_PATH, TorrentId)).GetResponse().GetResponseStream();
                }
                catch (Exception)
                {
                    Status = RecordStatus.Error;
                    return;
                }
                if (stream != null)
                {
                    int readbyte;
                    var buffer = new byte[4096];

                    if (!Directory.Exists(_outpath))
                        Directory.CreateDirectory(_outpath);

                    var FilePath = _outpath + "/" + Id + ".ts";

                    FileStream outstream = null;
                    try
                    {
                        if (File.Exists(FilePath))
                            File.Delete(FilePath);
                        outstream = File.OpenWrite(FilePath);

                    }
                    catch (Exception)
                    {
                        Status = RecordStatus.Error;
                        if (outstream != null)
                        {
                            outstream.Close();
                            outstream.Dispose();
                        }
                        stream.Close();
                        stream.Dispose();
                        return;
                    }
                    Start = DateTime.Now;
                    TimeSpan flushTS = new TimeSpan(0, 1, 0);
                    DateTime lastFlush = DateTime.Now;
                    while ((readbyte = stream.Read(buffer, 0, buffer.Length)) > 0 && _inProgress)
                    {
                        if (End < DateTime.Now)
                            break;
                        if (Status == RecordStatus.Pause)
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                        Status = RecordStatus.Start;
                        if (!outstream.CanWrite)
                        {
                            Status = RecordStatus.Error;
                            break;
                        }
                        if (readbyte == 4)
                            continue;
                        outstream.Write(buffer, 0, readbyte);
                        if (lastFlush + flushTS < DateTime.Now)
                        {
                            outstream.Flush(true);
                            lastFlush = DateTime.Now;
                        }
                    }
                    outstream.Close();
                    stream.Close();
                    outstream.Dispose();
                    Status = RecordStatus.Finished;
                    End = DateTime.Now;
                    GC.Collect();
                    _inProgress = false;
                }
            });
            Status = RecordStatus.Starting;
        }

        public void Pause()
        {
            Status = RecordStatus.Pause;
        }

        public void Stop()
        {
            _inProgress = false;
        }

        public XElement GetXml()
        {
            return new XElement("Record", new XAttribute("Name", Name),
                    new XAttribute("Start", DateTimeToString(Start)),
                    new XAttribute("End", DateTimeToString(End)),
                    new XAttribute("Status", Status.ToString()),
                    new XAttribute("TorrentId", TorrentId.ToString()),
                    new XAttribute("Id", Id));
        }

        private string DateTimeToString(DateTime date)
        {
            return date.ToString("ddMMyyyy_HHmmss");
        }

    }

    public enum RecordStatus
    {
        Init,
        Wait,
        Start,
        Pause,
        Finished,
        Starting,
        Error
    }

    public class RecordAsyncResult
    {
        public RecordStatus status;
        public Exception error;
    }
}
