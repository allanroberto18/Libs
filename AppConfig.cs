using System.Configuration;

namespace Libs
{
    public class AppConfig
    {
        public static void UpdateSetting(string key, string value)
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings[key].Value = value;
            configuration.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("appSettings");
        }

        public static string GetValue(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}
