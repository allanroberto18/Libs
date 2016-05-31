using System.IO.Ports;

namespace Libs
{
    public class PortaCOM
    {
        public static void GetPorta()
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