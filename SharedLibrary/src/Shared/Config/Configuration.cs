namespace Shared.Config;

using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;

public static class Configuration
{
    // Diccionario en memoria donde se guardará la configuración final.
    private static StringDictionary? appConfiguration;

    // -------------------------------------------------------------
    // Devuelve la configuración cargada. Si nunca ha sido cargada,
    // activa el proceso de "lazy initialization" para construirla.
    // -------------------------------------------------------------
    private static StringDictionary GetAppConfiguration()
    {
        return appConfiguration == null
            ? appConfiguration = LoadAppConfiguration()
            : appConfiguration;
    }

    // -------------------------------------------------------------
    // Carga ambos archivos de configuración:
    //  - appsettings.default.cfg
    //  - appsettings.cfg  (puede sobrescribir valores)
    //
    // Combina ambas configuraciones en un solo diccionario.
    // -------------------------------------------------------------
    private static StringDictionary LoadAppConfiguration()
    {
        var cfg = new StringDictionary();
        var basePath = Directory.GetCurrentDirectory();
        var deploymentMode = Environment.GetEnvironmentVariable("DEPLOYMENT_MODE") ?? "development";

        var paths = new string[]
        {
            $"{basePath}/appsettings.default.cfg",
            $"{basePath}/appsettings.{deploymentMode}.cfg",
            $"{basePath}/appsettings.cfg"
        };

        foreach (var path in paths)
        {
            var file = Path.Combine(basePath, path);

            if (File.Exists(file))
            {
                var tmp = LoadConfigurationFile(file);

                foreach (string k in tmp.Keys)
                    cfg[k] = tmp[k];
            }
        }

        return cfg;
    }

    // -------------------------------------------------------------
    // Lee un archivo línea por línea, elimina comentarios,
    // extrae pares clave-valor y los devuelve en un diccionario.
    // -------------------------------------------------------------
    public static StringDictionary LoadConfigurationFile(string file)
    {
        string[] lines = File.ReadAllLines(file);
        var cfg = new StringDictionary();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].TrimStart();

            if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                continue;

            var kv = line.Split('=', 2, StringSplitOptions.TrimEntries);
            cfg[kv[0]] = kv[1];
        }

        return cfg;
    }

    // -------------------------------------------------------------
    // Devuelve el valor de una clave (o null si no existe).
    // -------------------------------------------------------------
    public static string? Get(string key)
    {
        return Get(key, null);
    }

    // -------------------------------------------------------------
    // Devuelve un valor con opción a un "fallback"
    // si la clave no existe en configuración.
    // -------------------------------------------------------------
    public static string? Get(string key, string? val)
    {
        return Environment.GetEnvironmentVariable(key)
            ?? GetAppConfiguration()[key]
            ?? val;
    }

    // -------------------------------------------------------------
    // Devuelve valores tipados, usando conversión automática
    // (int, bool, double, enums, etc.)
    // -------------------------------------------------------------
    public static T Get<T>(string key)
    {
        return Get<T>(key, default!);
    }

    // -------------------------------------------------------------
    // Versión avanzada: permite valor por defecto tipado.
    // -------------------------------------------------------------
    public static T Get<T>(string key, T val)
    {
        string? value = Environment.GetEnvironmentVariable(key)
            ?? GetAppConfiguration()[key];

        if (string.IsNullOrWhiteSpace(value))
            return val;

        try
        {
            Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            // Conversión especial para enums
            if (targetType.IsEnum)
                return (T)Enum.Parse(targetType, value, ignoreCase: true);

            var converter = TypeDescriptor.GetConverter(targetType);

            if (converter != null && converter.CanConvertFrom(typeof(string)))
                return (T)converter.ConvertFromString(null, CultureInfo.InvariantCulture, value)!;

            return (T)Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }
        catch
        {
            // Si algo falla, se devuelve el valor por defecto de seguridad.
            return val;
        }
    }
}
