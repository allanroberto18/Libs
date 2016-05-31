using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Libs
{
    public static class Validacao
    {
        public static void ValidarCampos(TextBox textBox, ErrorProvider erro, String mensagem)
        {
            if (!String.IsNullOrWhiteSpace(textBox.Text))
            {
                erro.SetError(textBox, "");
            }
            else
            {
                erro.SetError(textBox, mensagem);
            }
        }

        public static void ValidarMaskCampos(MaskedTextBox textBox, ErrorProvider erro, String mensagem)
        {
            if (textBox.MaskFull)
            {
                erro.SetError(textBox, "");
            }
            else
            {
                erro.SetError(textBox, mensagem);
            }
        }

        public static void CompararCampos(TextBox textBox1, TextBox textBox2, ErrorProvider erro, String mensagem)
        {
            if (!String.IsNullOrWhiteSpace(textBox1.Text) && !String.IsNullOrWhiteSpace(textBox2.Text))
            {
                if (textBox1.Text == textBox2.Text)
                {
                    erro.SetError(textBox1, "");
                    erro.SetError(textBox2, "");
                }
            }
            else
            {
                erro.SetError(textBox1, mensagem);
                erro.SetError(textBox2, mensagem);
            }
        }

        public static void ValidarInteiro(TextBox textBox, ErrorProvider erro, String mensagem)
        {
            int teste;
            if (int.TryParse(textBox.Text, out teste))
            {
                erro.SetError(textBox, "");
            }
            else
            {
                erro.SetError(textBox, mensagem);
            }
        }

        public static void ValidarEmail(TextBox textBox, ErrorProvider erro)
        {
            bool isEmail = Regex.IsMatch(textBox.Text, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
            if (isEmail == true)
            {
                erro.SetError(textBox, "");
            }
            else
            {
                erro.SetError(textBox, "E-mail inválido");
            }
        }

        public static string RemoverAcentos(string texto)
        {
            string comAcentos = "ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûùÇç";
            string semAcentos = "AAAAAAaaaaaEEEEeeeeIIIIiiiiOOOOOoooooUUUuuuuCc";

            for (int i = 0; i < comAcentos.Length; i++)
            {
                texto = texto.Replace(comAcentos[i].ToString(), semAcentos[i].ToString());
            }
            return texto;
        }

        public static string RemoverCaracteresEspeciais(string texto)
        {
            string pattern = @"(?i)[^0-9a-záéíóúàèìòùâêîôûãõç\s]";
            string replacement = "";

            Regex rgx = new Regex(pattern);

            return rgx.Replace(texto, replacement);
        }

        public static string TratarMensagem(string texto)
        {
            return RemoverAcentos(RemoverCaracteresEspeciais(texto));
            //return texto;
        }
    }
}