namespace AlgorithmicArt.Data;

public enum LogLevel
{
    Info,
    Warn,
    Error
}

public class LogEntry
{
    public DateTime Time { get; set; }
    public LogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>轻量日志服务（M1 内置实现，后续可替换为 Serilog）。</summary>
public class SimpleLogger
{
    public event Action<LogEntry>? EntryAdded;

    public void Info(string msg) => Add(LogLevel.Info, msg);
    public void Warn(string msg) => Add(LogLevel.Warn, msg);
    public void Error(string msg) => Add(LogLevel.Error, msg);

    private void Add(LogLevel level, string message)
    {
        var entry = new LogEntry { Time = DateTime.Now, Level = level, Message = message };
        EntryAdded?.Invoke(entry);
    }
}
