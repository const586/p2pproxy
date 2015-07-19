using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2pProxy.Http.Server
{
    public struct Range
    {
        public string Unit;
        public long? From;
        public long? To;

        public static Range[] Parse(string range)
        {
            string[] buf = range.Split("=".ToCharArray(), 2, StringSplitOptions.None);
            List<Range> res = new List<Range>();
            string unit = buf[0];
            if (string.IsNullOrEmpty(buf[1]))
            {
                res.Add(new Range() { Unit = unit });
                return res.ToArray();
            }
            buf = buf[1].Split(",".ToCharArray());
            foreach (var sr in buf)
            {
                Range r = new Range();
                r.Unit = unit;
                var rbuf = sr.Split("-".ToCharArray(), 2, StringSplitOptions.None);
                if (string.IsNullOrEmpty(rbuf[0]))
                    r.From = null;
                else
                    r.From = long.Parse(rbuf[0]);
                if (string.IsNullOrEmpty(rbuf[1]))
                    r.To = null;
                else r.To = long.Parse(rbuf[1]);
                res.Add(r);
            }

            return res.ToArray();
        }

        public bool IsLast
        {
            get
            {
                return From == null && To != null;
            }
        }

        public bool IsFirst { get { return From != null && To == null; } }
        public bool IsCenter { get { return From != null && To != null; } }
    }
}
