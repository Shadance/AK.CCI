using System.Collections.Specialized;
using System.Configuration;

namespace AK.CCI.Service.Settings
{
	public class Configuration : ConfigurationSection, IConfiguration
    {
        public Configuration()
        {
            this.CurrentConfiguration.GetSection("Configuration");
        }

        public string Login => (string)this["Login"];
        public string Password => (string)this["Password"];
    }
}