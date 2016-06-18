using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

public class Logger
{
    public enum LogLevel
    {
        DEBUG = 0,
        VERBOSE,
        INFO,
        WARNING,
        ERROR,
        CONTINUE
    }
    private static readonly string[] logLevelTranslated = {
        "[DEBUG]  ",
        "[VERBOSE]",
        "[INFO]   ",
        "[WARNING]",
        "[ERROR]  ",
        "         "
    };
    private static Logger _instance;
    public static Logger Instance { get { if (_instance == null) _instance = new Logger(); return _instance; } }
    private StreamWriter fstream;
    private StreamWriter outStream;
    public Stream CurrentStream { get; private set; }

    public event EventHandler<string> OnLog;

    private LogLevel lastLogLevel;
    private LogLevel minLogLevel;
    public LogLevel LoggingLevel { get { return minLogLevel; } set { minLogLevel = value; } }
    public Logger()
    {
        String filePath = DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") + ".log";
        lastLogLevel = LogLevel.CONTINUE;
        minLogLevel = LogLevel.INFO;
        this.outStream = new StreamWriter(Console.OpenStandardOutput());
        this.CurrentStream = Console.OpenStandardOutput();
    }
    ~Logger()
    {
        this.close();
    }
    public void SetStream(Stream stream)
    {
        this.CurrentStream = stream;
        this.outStream = new StreamWriter(stream);
    }
    public void setLogFile(string path)
    {
        if (this.fstream != null)
            this.close();
        try
        {
            this.fstream = new StreamWriter(new FileStream(path == "" ? DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") + ".log" : path, FileMode.CreateNew));
        }
        catch (Exception ex)
        {
            outStream.WriteLine("Could not initiate the logger: " + ex.Message);
            this.fstream = null;
        }
    }
    public void log(LogLevel l, string msg, Action<string> logAction = null)
    {
        bool isInitial = true;
        foreach(var s in msg.Split('\n'))
        {
            doLog(isInitial ? l : LogLevel.CONTINUE, s, logAction);
            isInitial = false;
        }

    }
    private void doLog(LogLevel l, string msg, Action<string> logAction = null)
    {
        if (l != LogLevel.CONTINUE)
            lastLogLevel = l;
        if (lastLogLevel < minLogLevel)
            return;
        String line = logLevelTranslated[(int)l] + msg;
        outStream.WriteLine(line);
        if (this.fstream != null)
            fstream.WriteLine(line);
        if (logAction != null)
            logAction.Invoke(line);
        if (this.OnLog != null)
            this.OnLog(this, line);
    }
    public void close()
    {
        if (this.fstream == null)
            return;
        this.fstream.Close();
        this.fstream = null;
    }
    
}
