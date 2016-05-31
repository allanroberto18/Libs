using System;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Libs
{
    public class SMSOld
    {
        public SerialPort PortaSerial { get; set; }
        public string Porta {
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

        public SMSOld()
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

        public string ExecutarComando(SerialPort port, string comando, int responseTimeout, string errorMessage)
        {
            try
            {
                port.DiscardOutBuffer();
                port.DiscardInBuffer();
                ReceiveNow.Reset();
                port.Write(comando + "\r");

                string input = LerResposta(port, responseTimeout);
                if (input.Equals("10"))
                    throw new Exception("Não foi possível enviar sua mensagem, selecione outra porta");
                if (input.Contains("ERROR: 305"))
                    throw new Exception("Problemas com a quantidade de caractéres da mensagem");
                if (input.Contains("ERROR: 500"))
                    throw new Exception("A conexão entre o moldem e o chip não se mostra estável. \nVerifique se o moldem está funcionando devidamente");
                if (input.Contains("SMMEMFULL"))
                    throw new Exception("A memória do seu chip está cheio, remova mensagens existentes e verifique o seu saldo para porder enviar mensagens");
                if (input.Contains("CMTI"))
                    throw new Exception("Atenção, seus créditos podem ter acabado ou você pode ter excedido a quantidade de mensagens enviadas durante o dia");
                if ((input.Length == 0) || ((!input.Contains("\r\n> ")) && (!input.Contains("\r\nOK\r\n"))))
                    throw new ApplicationException("Você não possui créditos para executar essa operação.");
                return input;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string LerResposta(SerialPort port, int timeout)
        {
            string buffer = string.Empty;
            int i = 0;
            try
            {
                do
                {
                    if (ReceiveNow.WaitOne(timeout, false))
                    {
                        string t = port.ReadExisting();
                        buffer += t;
                    }

                    if (i == 10)
                    {
                        buffer = i.ToString();
                    }
                    i++;
                }
                while (!buffer.Contains("\r\nOK\r\n") && !buffer.Contains("\r\n>") && !buffer.EndsWith("\r\nERROR\r\n") && !buffer.Contains("SMMEMFULL") && !buffer.Contains("CMTI") && !buffer.Contains("ERROR: 500") && !buffer.Contains("+CMS ERROR: 305") && !buffer.Equals("10"));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return buffer;
        }

        public SerialPort OpenPort()
        {
            ReceiveNow = new AutoResetEvent(false);
            PortaSerial = new SerialPort();
            if (PortaSerial.IsOpen)
            {
                PortaSerial.Close();
            }
                
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
            return PortaSerial;
        }

        public void ClosePort()
        {
            try
            {
                if (PortaSerial.IsOpen)
                {
                    PortaSerial.Close();
                    PortaSerial.DataReceived += new SerialDataReceivedEventHandler(PortDataReceived);
                    PortaSerial = null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ShortMessageCollection ParseMessages(string input)
        {
            ShortMessageCollection messages = new ShortMessageCollection();
            try
            {
                Regex r = new Regex(@"\+CMGL: (\d+),""(.+)"",""(.+)"",(.*),""(.+)""\r\n(.+)\r\n");
                Match m = r.Match(input);
                while (m.Success)
                {
                    ShortMessage msg = new ShortMessage();

                    msg.Index = m.Groups[1].Value;
                    msg.Status = m.Groups[2].Value;
                    msg.Sender = m.Groups[3].Value;
                    msg.Alphabet = m.Groups[4].Value;
                    msg.Sent = m.Groups[5].Value;
                    msg.Message = m.Groups[6].Value;
                    messages.Add(msg);

                    m = m.NextMatch();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return messages;
        }

        public string TesteRetorno()
        {
            string receiveData = ExecutarComando(PortaSerial, "AT#RSEN", 300, "Nenhum Telefone Conectado");
            //string receiveData = ExecutarComando(PortaSerial, "ATCMD1 CMD2=10+CMD1;+CMD2=, ,10;+CMD1?;+CMD1=?<CR>", 300, "Erro");
            //string receiveData = ExecutarComando(PortaSerial, "AT#RSS", 300, "Nenhum Telefone Conectado");
            //string receiveData = ExecutarComando(PortaSerial, "AT+GSN", 300, "Nenhum Telefone Conectado");
            //string receiveData = ExecutarComando(PortaSerial, "AT#CCID=?", 300, "Nenhum Telefone Conectado");
            //string receiveData = ExecutarComando(PortaSerial, "AT#CCID", 300, "Nenhum Telefone Conectado");
            //string receiveData = ExecutarComando(PortaSerial, "AT#CIMI", 300, "Nenhum Telefone Conectado");

            //string receiveData = ExecutarComando(PortaSerial, "AT#PCT", 300, "Nenhum Telefone Conectado");
            //string receiveData = ExecutarComando(PortaSerial, "AT+CPIN", 300, "Nenhum Telefone Conectado");

            return receiveData;;
        }

        public bool SendMensagem(SerialPort port, string telefone, string mensagem)
        {
            bool isSend = false;
            try
            {
                string receiveData = "";
                string comando = "";

                receiveData = ExecutarComando(port, "AT", 300, "Nenhum telefone conectado");
                receiveData = ExecutarComando(port, "AT+CMGF=1", 300, "Falha ao definir o formato de mensagem");
                receiveData = ExecutarComando(port, "AT+CSCS=\"GSM\"", 300, "Falha ao definir o encoding da mensagem");

                comando = "AT+CMGS=\"" + telefone + "\"";
                receiveData = ExecutarComando(port, comando, 300, "Falha para aceitar o número de telefone");

                comando = mensagem + char.ConvertFromUtf32(26) + "\r";
                
                receiveData = ExecutarComando(port, comando, 3000, "Falha para enviar a mensagem");

                if (receiveData.Contains("\r\nOK\r\n"))
                {
                    port.Close();
                    isSend = true;
                }
            }
            catch (Exception ex)
            {
                port.Close();
                throw ex;
            }
            return isSend;
        }

        public ShortMessageCollection LerMensagens(SerialPort port, string comando)
        {
            ShortMessageCollection messages = null;
            try
            {
                ExecutarComando(port, "AT", 300, "Nenhum telefone conectado");
                ExecutarComando(port, "AT+CMGF=1", 300, "Falha ao definir o formato de mensagem");
                ExecutarComando(port, "AT+CSCS=\"PCCP437\"", 300, "Falha ao definir o conjunto de caracteres");
                ExecutarComando(port, "AT+CPMS=\"SM\"", 300, "Falha ao selecionar o armazenamento de mensagens");
                string input = ExecutarComando(port, comando, 5000, "Falha ao ler as mensagens");

                /*
                string recievedData = ExecCommand(port, "at", 300, "No phone connected at .");
                // Debug.Print(recievedData.ToString());
                recievedData = ExecCommand(port, "at+stgi=0", 300, "Failed to SIM Toolkit Get Information.");
                // Debug.Print(recievedData.ToString());
                recievedData = ExecCommand(port, "at+stgr=0,1,177", 300, "Failed to SIM Toolkit Give Response.");
                // Debug.Print(recievedData.ToString());
                recievedData = ExecCommand(port, "at+stgi=6", 300, "Failed to SIM Toolkit Get Information."); 
                // Debug.Print(recievedData.ToString());//Error here: No success message was received.
                recievedData = ExecCommand(port, "at+stgr=6,1,2", 300, "Failed to set message format.");
                */

                messages = ParseMessages(input);

                return messages;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool DeleteMensagem(SerialPort port, string comando)
        {
            bool isDeleted = false;
            try
            {
                string receiveData = "";
                receiveData = ExecutarComando(port, "AT", 300, "Nenhum telefone conectado");
                receiveData = ExecutarComando(port, "AT+CMGF=1", 300, "Falha ao definir o formato de mensagem");
                receiveData = ExecutarComando(port, comando, 300, "Falha em excluir mensagens");

                if (receiveData.EndsWith("\r\nOK\r\n"))
                {
                    isDeleted = true;
                }
                return isDeleted;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
