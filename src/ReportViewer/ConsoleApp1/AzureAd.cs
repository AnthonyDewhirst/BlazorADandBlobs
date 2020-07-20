using System.Globalization;

namespace ConsoleApp1
{
    public class AzureAd
    {

        public string Instance { get; set; } = "https://login.microsoftonline.com/{0}";

        public string Tenant { get; set; }

        public string ClientId { get; set; }

        public string Authority => string.Format(CultureInfo.InvariantCulture, Instance, Tenant);
    }
}