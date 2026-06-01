using JailBreak.Config;
using System.Reflection;

namespace JailBreak.Services;

public class LangProvider : ILangProvider
{
    private readonly LangConfig _config;
    private readonly Dictionary<string, PropertyInfo> _properties;

    public LangProvider(LangConfig config)
    {
        _config = config;
        _properties = typeof(LangConfig).GetProperties()
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);
    }

    public string GetMessage(string key, params object[] args)
    {
        if (_properties.TryGetValue(key, out var prop))
        {
            var value = prop.GetValue(_config)?.ToString() ?? key;
            if (args != null && args.Length > 0)
            {
                try
                {
                    return string.Format(value, args);
                }
                catch (FormatException)
                {
                    return value;
                }
            }
            return value;
        }

        return key;
    }
}
