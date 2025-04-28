using System;
using System.IO;

namespace uTransferServer
{
    public class Logger
    {
        private static readonly string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        private static readonly string logFile = Path.Combine(logDirectory, "ServerLog.txt");
        
        static Logger()
        {
            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);

            if (!File.Exists(logFile))
                File.Create(logFile).Dispose();
        }

        public static void Log(string message)
        {
            string timestampedMessage = $"[{DateTime.Now:dd-MM-yyyy HH:mm:ss}] {message}";
            Console.WriteLine(timestampedMessage);

            try
            {
                File.AppendAllText(logFile, timestampedMessage + Environment.NewLine);
            }
            catch { }
        }
    }
}
