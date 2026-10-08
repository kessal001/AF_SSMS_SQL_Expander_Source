using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace AfSsmsSqlExpander
{
    internal static class SnippetStore
    {
        private static readonly object Sync = new object();
        private static readonly string BaseFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AF-Sviluppo",
            "SsmsSqlExpander");

        internal static readonly string ConfigPath = Path.Combine(BaseFolder, "snippets.json");
        private static readonly string LogPath = Path.Combine(BaseFolder, "errors.log");

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

        private static void EnsureLoaded()
        {
            try
            {
                lock (Sync)
                {
                    Directory.CreateDirectory(BaseFolder);

                    if (!File.Exists(ConfigPath))
                        WriteDefaultConfig();

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
                LogError("Impossibile leggere snippets.json. Mantengo l'ultima configurazione valida.", ex);
            }
        }

        private static void WriteDefaultConfig()
        {
            string json =
                "{\r\n" +
                "  \"insa\": \"ISNULL(annullato,'')<>'S'\",\r\n" +
                "  \"dropt\": \"DROP TABLE IF EXISTS $cursor$\",\r\n" +
                "  \"nol\": \"WITH (NOLOCK)\",\r\n" +
                "  \"tryc\": \"BEGIN TRY\\n    $cursor$\\nEND TRY\\nBEGIN CATCH\\n    THROW;\\nEND CATCH\"\r\n" +
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
                // Mai interferire con l'editor per un problema di logging.
            }
        }
    }
}
