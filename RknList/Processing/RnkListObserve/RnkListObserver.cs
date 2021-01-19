using NLog;
using RknList.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace RknList.Processing.RnkListObserve
{
    public class RnkListObserver
    {
        private readonly Regex _ip4Regex = new Regex(@"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\/\d+)?", RegexOptions.None);

        private readonly string _zapretInfoDumpUrl;
        private readonly int _zapretInfoDumpObserveIntervalMs;
        private readonly object _locker = new object();
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        private Timer? _observeTimer;
        private string[]? _firstList;
        private string[]? _secondList;
        private bool _firstIsEffective;

        public RnkListObserver(AppConfig config, ILogger logger)
        {
            _zapretInfoDumpUrl = config.ZapretInfoDumpUrl ?? throw new ArgumentNullException(nameof(config.ZapretInfoDumpUrl));
            _zapretInfoDumpObserveIntervalMs = config.ZapretInfoDumpObserveIntervalMs ?? throw new ArgumentNullException(nameof(config.ZapretInfoDumpUrl));
            _logger = logger;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(60);
        }

        public void StartObserve()
        {
            lock (_locker)
            {
                if (_observeTimer == null)
                {
                    _observeTimer = new Timer(ObserveRknList, null, 0, Timeout.Infinite);
                    _logger.Info("Observer started");
                }
            }
        }

        public IReadOnlyCollection<string> GetCurrentIPAddressList()
        {
            lock (_locker)
            {
                if (_firstIsEffective)
                    return _firstList ?? Array.Empty<string>();
                return _secondList ?? Array.Empty<string>();
            }
        }

        private async void ObserveRknList(object? args)
        {
            try
            {
                var response = await _httpClient.GetAsync(_zapretInfoDumpUrl, HttpCompletionOption.ResponseContentRead);
                _logger.Info($"Zapret-Info dump recieved. HttpStatus: {response.StatusCode}; Lenght: {response.Content?.Headers?.ContentLength}");
                if (!response.IsSuccessStatusCode || response.Content == null)
                    return;
                var dump = Encoding.GetEncoding(1251).GetString(await response.Content.ReadAsByteArrayAsync());
                var ips = new LinkedList<string>();
                foreach (var line in dump.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    var ipVals = new LinkedList<string>();
                    var ipVal = new StringBuilder();
                    foreach (var i in line)
                    {
                        if (i == ';')
                        {
                            ipVals.AddLast(ipVal.ToString());
                            break;
                        }
                        else if (i == '|')
                        {
                            ipVals.AddLast(ipVal.ToString());
                            ipVal.Clear();
                            continue;
                        }
                        ipVal.Append(i);
                    }
                    foreach (var val in ipVals)
                    {
                        var match = _ip4Regex.Match(val);
                        if (match.Success)
                            ips.AddLast(match.Value);
                    }
                }
                AssignToResult(CollapceIpAddresses(ips));
                _observeTimer?.Change(_zapretInfoDumpObserveIntervalMs, Timeout.Infinite);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occured while recieving Zapret-Info dump");
            }
        }

        private void AssignToResult(string[] addresses)
        {
            lock (_locker)
            {
                if (_firstIsEffective)
                {
                    _secondList = addresses;
                    _firstIsEffective = false;
                    _firstList = null;
                }
                else
                {
                    _firstList = addresses;
                    _firstIsEffective = true;
                    _secondList = null;
                }
            }
        }

        private string[] CollapceIpAddresses(IEnumerable<string> addresses)
        {
            return addresses.Distinct().OrderBy(i => i).ToArray();
        }
    }
}