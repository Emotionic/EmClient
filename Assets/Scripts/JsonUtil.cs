using System.IO;
using Newtonsoft.Json;

public static class JsonUtil
{
    /// <summary>
    /// オブジェクトをシリアライズしてJSONファイルに書き込みます(Json.NET使用)
    /// </summary>
    public static void Write<T>(string filepath, T data) where T : class
    {
        var _path = Path.GetDirectoryName(filepath);

        if (!Directory.Exists(_path))
        {
            Directory.CreateDirectory(_path);
        }

        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(filepath, json, System.Text.Encoding.UTF8);
    }

    /// <summary>
    /// JSONファイルをデシリアライズし、オブジェクトを返します(Json.NET使用)
    /// </summary>
    public static T Read<T>(string filepath) where T : class
    {
        if (!File.Exists(filepath))
            return null;

        string json = File.ReadAllText(filepath, System.Text.Encoding.UTF8);
        return JsonConvert.DeserializeObject<T>(json);
    }

}
