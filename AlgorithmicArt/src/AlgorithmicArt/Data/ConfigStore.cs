using System.Text.Json;
using AlgorithmicArt.Communication;

namespace AlgorithmicArt.Data;

/// <summary>应用配置。</summary>
public class AppConfig
{
    public SerialConfig Serial { get; set; } = new();
    public int Speed { get; set; } = 85;
    public bool AutoReconnect { get; set; }
    public string LastPage { get; set; } = "串口";
    public string BackgroundImagePath { get; set; } = "";
    public FilterSettings Filter { get; set; } = new();
    public PidSettings Pid { get; set; } = new();
    public int DataSize { get; set; } = 100;
    /// <summary>线性代数矩阵行数（2~6）。</summary>
    public int MatrixRows { get; set; } = 3;
    /// <summary>线性代数矩阵列数（2~6）。</summary>
    public int MatrixCols { get; set; } = 3;
}

/// <summary>配置中心：JSON 持久化串口参数、偏好。</summary>
public class ConfigStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _path;

    public ConfigStore(string? path = null)
    {
        _path = path ?? Path.Combine(AppContext.BaseDirectory, "config.json");
    }

    public AppConfig Load()
    {
        try
        {
            if (File.Exists(_path))
            {
                var json = File.ReadAllText(_path);
                return JsonSerializer.Deserialize<AppConfig>(json, Options) ?? new AppConfig();
            }
        }
        catch { /* 配置损坏则回退默认 */ }
        return new AppConfig();
    }

    public void Save(AppConfig cfg)
    {
        try
        {
            var json = JsonSerializer.Serialize(cfg, Options);
            File.WriteAllText(_path, json);
        }
        catch { /* ignore */ }
    }
}
