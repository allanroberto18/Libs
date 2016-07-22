using System;
using System.Data;
using GsmComm.GsmCommunication;
using GsmComm.PduConverter;
using GsmComm.PduConverter.SmartMessaging;
using Entities.Models;
using Entities.Services;

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

        public void SerialNumber()
        {
            //GsmCommMain conn = new GsmCommMain(Porta);
            SMS conn = new SMS();
            string resultado;
            try
            {
                conn.OpenPort();
                resultado = conn.ReturnSim();

                Sims entity = new Sims();
                entity.SetParams(resultado, 1);

                SimsService service = new SimsService();
                service.Add(entity);
                
                conn.ClosePort();
            }
            catch
            {
                conn.ClosePort();
                resultado = "SIM não localizado";
            }
            AppConfig.UpdateSetting("sim", resultado);
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
            // Checando os disparos
            string sim = AppConfig.GetValue("sim");
            if (sim == "SIM" || sim == "SIM não localizado")
            {
                throw new Exception("O chip não foi localizado, verifique se o mesmo está conectado e tente mais tarde");
            }
            SimsService simsService = new SimsService();
            Sims entitySims = simsService.getBySim(sim);
            AppConfig.UpdateSetting("disparos", entitySims.Quantidade.ToString());

            if (entitySims.Quantidade >= 190)
            {
                throw new Exception("O chip excedeu a quantidade de disparos diária");
            }

            if (text.Length > 160)
            {
                EnviarMensagemByArray(text, phone, entitySims);
                return true;
            }

            int quantidade = entitySims.Quantidade + 1;

            SmsSubmitPdu pdu = new SmsSubmitPdu(text, phone, "");
            GsmCommMain conn = new GsmCommMain(Porta);

            try
            {
                conn.Open();

                conn.SendMessage(pdu);

                conn.Close();

                entitySims.Quantidade = quantidade;

                simsService.Edit(entitySims);

                AppConfig.UpdateSetting("disparos", quantidade.ToString());

                return true;
            }
            catch (Exception ex)
            {
                if (conn.IsConnected())
                    conn.Close();
                throw ex;
            }
        }

        public void EnviarMensagemByArray(string text, string phone, Sims entity)
        {
            SmsSubmitPdu[] pdu = SmartMessageFactory.CreateConcatTextMessage(text, phone);

            GsmCommMain conn = new GsmCommMain(Porta);
            

            if (entity.Quantidade >= 190)
            {
                throw new Exception("O chip excedeu a quantidade de disparos diária");
            }

            int quantidade = GetTamanhoMsg(text) + entity.Quantidade;

            foreach (SmsSubmitPdu item in pdu)
            {
                try
                {
                    conn.Open();

                    conn.SendMessage(item);

                    SimsService simsService = new SimsService();
                    entity.Quantidade = quantidade;
                    simsService.Edit(entity);

                    AppConfig.UpdateSetting("disparos", quantidade.ToString());

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

        private int GetTamanhoMsg(string text)
        {
            if (text.Length <= 160)
                return 1;
            if (text.Length <= 320)
                return 2;
            if (text.Length <= 480)
                return 3;
            return 4;
        }
    }
}