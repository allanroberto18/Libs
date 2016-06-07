using System;
using System.IO.Ports;
using System.Text.RegularExpressions;

namespace Libs
{
    public class PortaCOM
    {
        public static void GetPorta()
        {
            string[] ports = SerialPort.GetPortNames();
            
            if (ports.Length == 0)
            {
                AppConfig.UpdateSetting("porta", "N");
                return;
            }

            foreach (string port in ports)
            {
                int value = Convert.ToInt16(Regex.Match(port, @"\d+").Value);

                if (value % 2 == 0)
                {
                    continue;
                }
                AppConfig.UpdateSetting("porta", port);
                return;
            }
        }

        public static void GetPort(string porta)
        {
            string[] ports = SerialPort.GetPortNames();
            int i = 0;

            if (ports.Length == 0)
            {
                AppConfig.UpdateSetting("porta", "N");
                return;
            }

            foreach (string port in ports)
            {
                if (i == 0)
                {
                    AppConfig.UpdateSetting("porta", port);
                    return;
                }
                i++;
            }
        }
    }
}