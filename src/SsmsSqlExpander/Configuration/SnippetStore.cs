using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace SsmsSqlExpander
{
    internal static class SnippetStore
    {
        private static readonly object Sync = new object();

        private static readonly string BaseFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SsmsSqlExpander");

        private static readonly string LegacyBaseFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AF-Sviluppo",
            "SsmsSqlExpander");

        internal static readonly string ConfigPath = Path.Combine(BaseFolder, "snippets.json");
        private static readonly string LogPath = Path.Combine(BaseFolder, "errors.log");
        private static readonly string LegacyConfigPath = Path.Combine(LegacyBaseFolder, "snippets.json");

        private static Dictionary<string, string> _snippets =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private static DateTime _lastWriteUtc = DateTime.MinValue;
        private static bool _initialized;

        public static bool TryGet(string abbreviation, out string expansion)
        {
            EnsureLoaded();
            lock (Sync)
                return _snippets.TryGetValue(abbreviation, out expansion);
        }

        internal static void EnsureConfigExists()
        {
            lock (Sync)
            {
                Directory.CreateDirectory(BaseFolder);

                if (File.Exists(ConfigPath))
                    return;

                if (File.Exists(LegacyConfigPath))
                {
                    File.Copy(LegacyConfigPath, ConfigPath, overwrite: false);
                    return;
                }

                WriteDefaultConfig();
            }
        }

        private static void EnsureLoaded()
        {
            try
            {
                lock (Sync)
                {
                    EnsureConfigExists();

                    DateTime writeUtc = File.GetLastWriteTimeUtc(ConfigPath);
                    if (_initialized && writeUtc == _lastWriteUtc)
                        return;

                    string json = File.ReadAllText(ConfigPath, Encoding.UTF8);
                    var serializer = new JavaScriptSerializer();
                    var loaded = serializer.Deserialize<Dictionary<string, string>>(json);

                    var normalized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    if (loaded != null)
                    {
                        foreach (var pair in loaded)
                        {
                            string key = (pair.Key ?? string.Empty).Trim();
                            if (key.Length == 0 || pair.Value == null)
                                continue;

                            normalized[key] = pair.Value;
                        }
                    }

                    _snippets = normalized;
                    _lastWriteUtc = writeUtc;
                    _initialized = true;
                }
            }
            catch (Exception ex)
            {
                LogError("Unable to read snippets.json. Keeping the last valid configuration.", ex);
            }
        }

        private static void WriteDefaultConfig()
        {
            string json =
                "{\r\n" +
                "  \"isna\": \"ISNULL(annullato,'')<>'S'\",\r\n" +
                "  \"sel\": \"SELECT * FROM \",\r\n" +
                "  \"selt\": \"SELECT TOP 100 * FROM \",\r\n" +
                "  \"dropt\": \"DROP TABLE IF EXISTS $cursor$\",\r\n" +
                "  \"today\": \"CAST(GETDATE() AS date)\",\r\n" +
                "  \"monday\": \"IF DATEDIFF(DAY, '19000101', CURRENT_TIMESTAMP) % 7 + 1 = 1 -- Monday=1\\nBEGIN\\n    $cursor$\\nEND\",\r\n" +
                "  \"join\": \"INNER JOIN $1 $2 ON $2.$3 = $4.$3$0\",\r\n" +
                "  \"tryc\": \"BEGIN TRY\\n    $1\\nEND TRY\\nBEGIN CATCH\\n    THROW;\\nEND CATCH\\n$0\"\r\n" +
                "}\r\n";

            File.WriteAllText(ConfigPath, json, new UTF8Encoding(false));
        }

        internal static void LogError(string message, Exception ex)
        {
            try
            {
                Directory.CreateDirectory(BaseFolder);
                File.AppendAllText(
                    LogPath,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + message + Environment.NewLine +
                    ex + Environment.NewLine + Environment.NewLine,
                    Encoding.UTF8);
            }
            catch
            {
                // Logging must never interfere with the SQL editor.
            }
        }
    }
}
