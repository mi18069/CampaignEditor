using Database;
using System.Configuration;

namespace CampaignEditor
{
    // Class for establishing and saving connectionString
    public class AppSetting
    {
        Configuration config;

        public AppSetting()
        {
            config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        public string GetConnectionString(string key)
        {
            return config.ConnectionStrings.ConnectionStrings[key].ConnectionString;
        }

        public void SaveConnectionString(string key, string value)
        {
            config.ConnectionStrings.ConnectionStrings[key].ConnectionString = value;
            config.ConnectionStrings.ConnectionStrings[key].ProviderName = "Npqsql";
            config.Save(ConfigurationSaveMode.Modified);
            AppSettings.ConnectionString = value;
        }
    }
}
