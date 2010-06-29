using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogViewer
{
    /// <summary>
    /// A log entry
    /// </summary>
    [Serializable]
    public class LogFilter
    {
        public DateTime TimeStamp { get; set; }
        public string Level { get; set; }
        public string Thread { get; set; }
        public string Message { get; set; }
        public string MachineName { get; set; }
        public string UserName { get; set; }
        public string HostName { get; set; }
        public string App { get; set; }
        public string Throwable { get; set; }
        public string Class { get; set; }
        public string Method { get; set; }
        public string File { get; set; }
        public string Line { get; set; }
        public string LogName { get; set; }

        public static List<LogEntry> FilteredEntries;  // last filtered list
        public bool IsFiltered;

        public LogFilter()
        {
            Clear();
        }

        public void Clear()
        {
            TimeStamp = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            File = string.Empty;
            Method = string.Empty;
            Class = string.Empty;
            Throwable = string.Empty;
            App = string.Empty;
            HostName = string.Empty;
            UserName = string.Empty;
            MachineName = string.Empty;
            Message = string.Empty;
            Thread = string.Empty;
            Level = string.Empty;
            Line = string.Empty;
            Message = string.Empty;
            Throwable = string.Empty;
            IsFiltered = false;
            LogName = string.Empty;
            FilteredEntries = null;
        }

        public void TrimAll()
        {
            File = File == null ? string.Empty : File.Trim();
            Method = Method == null ? string.Empty : Method.Trim();
            Class = Class == null ? string.Empty : Class.Trim();
            Throwable = Throwable.Trim();
            App = App == null ? string.Empty : App.Trim();
            HostName = HostName == null ? string.Empty : HostName.Trim();
            UserName = UserName == null ? string.Empty : UserName.Trim();
            MachineName = MachineName == null ? string.Empty : MachineName.Trim();
            Message = Message.Trim();
            Thread = Thread == null ? string.Empty : Thread.Trim();
            Level = Level==null ? string.Empty : Level.Trim();
            LogName = LogName == null ? string.Empty : LogName.Trim();
            Line = Line.Trim();
        }

    }
}
