using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows.Forms;
using rdjInterface;

namespace Plugin_ListenerCount
{
    public class PluginClass : IPlugin
    {
        private const string SettingsSection = "Settings";
        private const string DefaultUrl = "http://localhost/listener.txt";
        private const string DefaultIntervalMinutes = "5";
        private const string DefaultOutputPath = @"C:\RadioDJv3\listener.txt";

        private static readonly Regex NumberPattern = new Regex(@"^-?\d+$", RegexOptions.Compiled);
        private static readonly HttpClient HttpClient = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("RadioDJ Listener Count");
            return client;
        }

        private IHost host;
        private System.Timers.Timer pollTimer;
        private volatile bool isPolling;

        private string url;
        private int intervalMinutes;
        private string outputPath;

        public string PluginName => "ListenerCount";
        public string PluginTitle => "Listener Count Importer";
        public string PluginDescription => "Polls a URL for the current listener count and writes it to a text file.";
        public string PluginVersion => "1.0.0";
        public int PluginZone => 0;
        public bool HasActions => false;

        public void Initialize(IHost host)
        {
            this.host = host;
            LoadSettings();
            StartTimer();
            PollOnceFireAndForget();
        }

        public void Closing()
        {
            if (pollTimer != null)
            {
                pollTimer.Stop();
                pollTimer.Elapsed -= OnTimerElapsed;
                pollTimer.Dispose();
                pollTimer = null;
            }
        }

        public void ShowMain() => ShowConfig();

        public void ShowConfig()
        {
            using (var form = new ConfigForm(url, intervalMinutes, outputPath))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    url = form.Url;
                    intervalMinutes = form.IntervalMinutes;
                    outputPath = form.OutputPath;
                    SaveSettings();
                    StartTimer();
                    PollOnceFireAndForget();
                }
            }
        }

        public void ShowAbout()
        {
            MessageBox.Show(
                $"{PluginTitle}\nVersion {PluginVersion}\n\n{PluginDescription}",
                PluginTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        public UserControl LoadGUI() => null;

        public void KeyDown(object sender, KeyEventArgs e)
        {
        }

        public bool RunAction(string action, string[] parameters, Guid? guid) => false;

        public System.Collections.Generic.List<Events.EventAction> AvailableActions() =>
            new System.Collections.Generic.List<Events.EventAction>();

        public void AddTrack2Plugin(long trackId, long id2, int id3)
        {
        }

        public void AddTrack2Plugin(TrackPlayer track, long id2, int id3)
        {
        }

        public void StateChanged(Features feature, object data)
        {
        }

        private void LoadSettings()
        {
            url = host.GetSetting(SettingsSection, "Url", DefaultUrl);
            outputPath = host.GetSetting(SettingsSection, "OutputPath", DefaultOutputPath);

            var intervalText = host.GetSetting(SettingsSection, "IntervalMinutes", DefaultIntervalMinutes);
            if (!int.TryParse(intervalText, NumberStyles.Integer, CultureInfo.InvariantCulture, out intervalMinutes) || intervalMinutes < 1)
            {
                intervalMinutes = int.Parse(DefaultIntervalMinutes, CultureInfo.InvariantCulture);
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                url = DefaultUrl;
            }

            if (string.IsNullOrWhiteSpace(outputPath))
            {
                outputPath = DefaultOutputPath;
            }
        }

        private void SaveSettings()
        {
            host.SaveSetting(SettingsSection, "Url", url);
            host.SaveSetting(SettingsSection, "IntervalMinutes", intervalMinutes.ToString(CultureInfo.InvariantCulture));
            host.SaveSetting(SettingsSection, "OutputPath", outputPath);
        }

        private void StartTimer()
        {
            if (pollTimer != null)
            {
                pollTimer.Stop();
                pollTimer.Elapsed -= OnTimerElapsed;
                pollTimer.Dispose();
            }

            pollTimer = new System.Timers.Timer(intervalMinutes * 60000.0)
            {
                AutoReset = true,
            };
            pollTimer.Elapsed += OnTimerElapsed;
            pollTimer.Start();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e) => PollOnceFireAndForget();

        private void PollOnceFireAndForget()
        {
            if (isPolling)
            {
                return;
            }

            isPolling = true;
            _ = PollAsync();
        }

        private async System.Threading.Tasks.Task PollAsync()
        {
            try
            {
                string body;
                try
                {
                    body = await HttpClient.GetStringAsync(url).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    host?.DebugAddLine($"ListenerCount: failed to fetch '{url}': {ex.Message}");
                    return;
                }

                var trimmed = body?.Trim();
                if (string.IsNullOrEmpty(trimmed) || !NumberPattern.IsMatch(trimmed))
                {
                    host?.DebugAddLine($"ListenerCount: unexpected response from '{url}': '{trimmed}'");
                    return;
                }

                try
                {
                    WriteAtomic(outputPath, trimmed);
                }
                catch (Exception ex)
                {
                    host?.DebugAddLine($"ListenerCount: failed to write '{outputPath}': {ex.Message}");
                }
            }
            finally
            {
                isPolling = false;
            }
        }

        private static void WriteAtomic(string path, string content)
        {
            var tempPath = path + ".tmp";
            File.WriteAllText(tempPath, content);

            if (File.Exists(path))
            {
                File.Replace(tempPath, path, null);
            }
            else
            {
                File.Move(tempPath, path);
            }
        }
    }
}
