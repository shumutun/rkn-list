using ExtConfig;

namespace RknList.Config
{
    public class AppConfig
    {
        public string? ZapretInfoDumpUrl { get; set; }

        public int? ZapretInfoDumpObserveIntervalMs { get; set; }

        public static AppConfig? FromConfigFile()
        {
            return JsonConfigBuilder.Build<AppConfig>("Config.*.json");
        }
    }
}
