using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Text;

namespace UEDumperCS.Utils
{
    public class Logger : IDisposable
    {
        /// <summary>
        /// Lock object used for thread safety
        /// </summary>
        readonly object _lockObject = new();

        /// <summary>
        /// Indicates if this class is disposed
        /// </summary>
        bool _disposed;

        /// <summary>
        /// Log filestream instance
        /// </summary>
        StreamWriter _logFileStream;

        /// <summary>
        /// Static logger instance object used by Get
        /// </summary>
        static Logger _instance;

        /// <summary>
        /// Creates or gets a logger instance
        /// </summary>
        public static Logger Get => _instance ??= new Logger();

        /// <summary>
        /// Initializes a new logger instance
        /// </summary>
        public Logger() { }

        /// <summary>
        /// Initializes a new logger instance and enables file logging
        /// </summary>
        public Logger(string fileName) => EnableFileLogging(fileName);

        /// <summary>
        /// Public dispose implementation
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Creates a file to log to
        /// </summary>
        /// <param name="fileName">The log filename.</param>
        public void EnableFileLogging(string fileName = "log.txt")
        {
            if (_logFileStream is not null)
                throw new InvalidOperationException();

            lock (_lockObject)
            {
                try
                {
                    var dir = Path.GetDirectoryName(fileName);
                    if (dir != string.Empty)
                        Directory.CreateDirectory(dir);

                    _logFileStream = new StreamWriter(
                        File.Open(fileName, FileMode.Append, FileAccess.Write, FileShare.Read), Encoding.UTF8)
                    {
                        AutoFlush = true
                    };
                }
                catch (Exception ex)
                {
                    this.Error($"Failed to open or create log file: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Logs a debug message
        /// </summary>
        /// <param name="msg">The message to log.</param>
        [Conditional("DEBUG")]
        public void Debug(string msg) => Log(msg, ConsoleColor.Cyan);

        /// <summary>
        /// Logs an info message
        /// </summary>
        /// <param name="msg">The message to log.</param>
        public void Info(string msg) => Log(msg, ConsoleColor.White);

        /// <summary>
        /// Logs a success message
        /// </summary>
        /// <param name="msg">The message to log.</param>
        public void Success(string msg) => Log(msg, ConsoleColor.Green);

        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="msg">The message to log.</param>
        public void Warning(string msg) => Log(msg, ConsoleColor.Magenta);

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="msg">The message to log.</param>
        public void Error(string msg) => Log(msg, ConsoleColor.Red);

        /// <summary>
        /// Main logging method
        /// </summary>
        /// <param name="msg">The message to log.</param>
        /// <param name="clr">The console foreground color for the message.</param>
        /// <param name="caller">The caller method name.</param>
        private void Log(string msg, ConsoleColor clr, [CallerMemberName] string caller = "")
        {
            lock (_lockObject)
            {
                msg = $"[{DateTime.Now}]{caller,8}| {msg}";

                Console.ForegroundColor = clr;
                Console.WriteLine(msg);
                Console.ResetColor();

                _logFileStream?.WriteLine(msg);
            }
        }

        /// <summary>
        /// Internal dispose implementation
        /// </summary>
        /// <param name="disposing">Should managed objects be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_instance == this)
                    _instance = null;

                _logFileStream?.Dispose();
                _logFileStream = null;
            }

            _disposed = true;
        }
    }
}