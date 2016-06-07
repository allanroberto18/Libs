using System;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Libs
{
    public class SMS
    {
        public SerialPort PortaSerial { get; set; }

        public string Porta
        {
            get
            {
                //PortaCOM.GetPorta();
                return AppConfig.GetValue("porta");
            }
            set
            {
                PortaCOM.GetPorta();
            }
        }

        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public int ReadTimeout { get; set; }
        public int WriteTimeout { get; set; }

        public SMS()
        {
            BaudRate = 9600;
            DataBits = 8;
            ReadTimeout = 300;
            WriteTimeout = 300;
        }

        public AutoResetEvent ReceiveNow { get; set; }

        public void PortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (e.EventType == SerialData.Chars)
                {
                    ReceiveNow.Set();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string ExecutarComando(string comando, int responseTimeout, string errorMessage)
        {
            try
            {
                PortaSerial.DiscardOutBuffer();
                PortaSerial.DiscardInBuffer();
                ReceiveNow.Reset();
                PortaSerial.Write(comando + "\r");

                string input = LerResposta(responseTimeout);
                if (input.Equals("10"))
                    throw new Exception("Não foi possível enviar sua mensagem, selecione outra porta");
                if (input.Contains("ERROR: 305"))
                    throw new Exception("Problemas com a quantidade de caractéres da mensagem");
                if (input.Contains("ERROR: 500"))
                    throw new Exception(
                        "A conexão entre o moldem e o chip não se mostra estável. \nVerifique se o moldem está funcionando devidamente");
                if (input.Contains("SMMEMFULL"))
                    throw new Exception(
                        "A memória do seu chip está cheio, remova mensagens existentes e verifique o seu saldo para porder enviar mensagens");
                if (input.Contains("CMTI"))
                    throw new Exception(
                        "Atenção, seus créditos podem ter acabado ou você pode ter excedido a quantidade de mensagens enviadas durante o dia");
                if ((input.Length == 0) || ((!input.Contains("\r\n> ")) && (!input.Contains("\r\nOK\r\n"))))
                    throw new ApplicationException("Você não possui créditos para executar essa operação.");
                return input;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string LerResposta(int timeout)
        {
            string buffer = string.Empty;
            int i = 0;
            try
            {
                do
                {
                    if (ReceiveNow.WaitOne(timeout, false))
                    {
                        string t = PortaSerial.ReadExisting();
                        buffer += t;
                    }

                    if (i == 10)
                    {
                        buffer = i.ToString();
                    }
                    i++;
                } while (!buffer.Contains("\r\nOK\r\n") && !buffer.Contains("\r\n>") &&
                         !buffer.EndsWith("\r\nERROR\r\n") && !buffer.Contains("SMMEMFULL") && !buffer.Contains("CMTI") &&
                         !buffer.Contains("ERROR: 500") && !buffer.Contains("+CMS ERROR: 305") && !buffer.Equals("10"));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return buffer;
        }

        public void OpenPort()
        {
            ReceiveNow = new AutoResetEvent(false);
            PortaSerial = new SerialPort();
            try
            {
                PortaSerial.PortName = Porta;
                PortaSerial.BaudRate = BaudRate;
                PortaSerial.DataBits = DataBits;
                PortaSerial.StopBits = StopBits.One;
                PortaSerial.Parity = Parity.None;
                PortaSerial.ReadTimeout = ReadTimeout;
                PortaSerial.WriteTimeout = ReadTimeout;
                PortaSerial.DataReceived += new SerialDataReceivedEventHandler(PortDataReceived);
                PortaSerial.Open();
                PortaSerial.DtrEnable = true;
                PortaSerial.RtsEnable = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ClosePort()
        {
            if (PortaSerial.IsOpen)
            {
                PortaSerial.Close();
                PortaSerial.DataReceived += new SerialDataReceivedEventHandler(PortDataReceived);
                PortaSerial = null;
            }
        }

        public string ReturnSim()
        {
            string result = ExecutarComando("AT+CIMI", 300, "Nenhum Telefone Conectado");

            Regex regex = new Regex(@"\d+");
            Match match = regex.Match(result);

            string sim = match.Value.ToString();

            return sim;
        }
    }
}
