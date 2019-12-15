using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace NetworkNode
{
    public class NetworkNodeConfig
    {
        public IPAddress ManagementSystemAddress { get; set; }

        public ushort ManagementSystemPort { get; set; }

        public string NodeName { get; set; }

        public IPAddress CloudAddress { get; set; }

        public IPAddress NodeAddress { get; set; }

        public ushort CloudPort { get; set; }

        public static NetworkNodeConfig ParseConfig(string FileName)
        {
            var content = File.ReadAllLines(FileName).ToList();
            var config = new NetworkNodeConfig();

            config.ManagementSystemAddress = IPAddress.Parse(GetProperty(content, "MANAGEMENTADDRESS"));
            config.ManagementSystemPort = ushort.Parse(GetProperty(content, "MANAGEMENTPORT"));
            config.NodeName = GetProperty(content, "NODENAME");
            config.CloudAddress = IPAddress.Parse(GetProperty(content, "CLOUDADDRESS"));
            config.NodeAddress = IPAddress.Parse(GetProperty(content, "NODEADDRESS"));
            config.CloudPort = ushort.Parse(GetProperty(content, "CLOUDPORT"));
            return config;
        }

        private static string GetProperty(List<string> content, string propertyName)
        {
            return content.Find(line => line.StartsWith(propertyName)).Replace($"{propertyName} ", "");
        }
    }
}