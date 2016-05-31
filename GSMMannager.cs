using System;
using System.Data;
using System.IO.Ports;
using System.Linq;
using GsmComm.GsmCommunication;
using GsmComm.PduConverter;
using GsmComm.PduConverter.SmartMessaging;
using GsmComm.Server;

namespace Libs
{
    public class GSMMannager
    {
        public string Porta { get; set; }
        public string BaudRate { get; set; }
        public string Timeout { get; set; }

        public DataTable Messages;

        public GSMMannager() 
        {
            Porta = AppConfig.GetValue("porta");
            BaudRate = "9600";
        }

        public GSMMannager(string timeout) : this()
        {
            Timeout = timeout;
        }

        public string SerialNumber()
        {
            //GsmCommMain conn = new GsmCommMain(Porta);
            SMS conn = new SMS();
            try
            {
                conn.OpenPort();

                string resultado = conn.ReturnSim();

                conn.ClosePort();

                return resultado;
            }
            catch (Exception ex)
            {
                conn.ClosePort();
                return ex.Message;
            }
        }

        public string TestConnection()
        {
            string result = "Não está conectado";

            GsmCommMain conn = new GsmCommMain(Porta);
            try
            {
                conn.Open();

                result = "Conectado";
                
                conn.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public bool EnviarMensagem(string text, string phone)
        {
            if (text.Length > 160)
            {
                EnviarMensagemByArray(text, phone);

                return true;
            }

            SmsSubmitPdu pdu = new SmsSubmitPdu(text, phone, "");
            GsmCommMain conn = new GsmCommMain(Porta);

            try
            {
                conn.Open();

                conn.SendMessage(pdu);

                conn.Close();

                return true;
            }
            catch (Exception ex)
            {
                if (conn.IsConnected())
                    conn.Close();
                throw ex;
            }
        }

        public void EnviarMensagemByArray(string text, string phone)
        {
            SmsSubmitPdu[] pdu = SmartMessageFactory.CreateConcatTextMessage(text, phone);

            GsmCommMain conn = new GsmCommMain(Porta);

            foreach (SmsSubmitPdu item in pdu)
            {
                try
                {
                    conn.Open();

                    conn.SendMessage(item);

                    conn.Close();
                }
                catch (Exception ex)
                {
                    if (conn.IsConnected())
                        conn.Close();
                    throw ex;
                }
            }
        }

        public DataTable GetMessages()
        {
            GsmCommMain conn = new GsmCommMain(Porta);

            Messages = new DataTable();
            try
            {
                conn.Open();

                DecodedShortMessage[] messages = conn.ReadMessages(PhoneMessageStatus.All, PhoneStorageType.Sim);

                Messages.Columns.Add("Remetente", typeof(string));
                Messages.Columns.Add("Hora de Envio", typeof(string));
                Messages.Columns.Add("Mensagem", typeof(string));

                foreach (DecodedShortMessage message in messages)
                {
                    ShowMessage(message.Data);
                }

                conn.Close();

                return Messages;
            }
            catch (Exception ex)
            {
                if (conn.IsConnected())
                    conn.Close();
                throw ex;
            }
        }

        private void BindGrid(SmsPdu pdu)
        {
            DataRow dr = Messages.NewRow();
            SmsDeliverPdu data = (SmsDeliverPdu)pdu;

            dr[0] = data.OriginatingAddress.ToString();
            dr[1] = data.SCTimestamp.ToString();
            dr[2] = data.UserDataText;
            Messages.Rows.Add(dr);
        }

        private void ShowMessage(SmsPdu pdu)
        {
            BindGrid(pdu);
        }
    }
}