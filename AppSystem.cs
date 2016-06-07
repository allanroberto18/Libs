using System;
using System.Windows.Forms;

namespace Libs
{
    public class AppSystem
    {
        public static string TratarTelefone(string telefone, int tipo=1)
        {
            if (tipo == 2)
            {
                return telefone.Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", "");
            }
            return "+55" + telefone.Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", "");
        }

        public static string GerarCodigoRandomico()
        {
            int tamanho = 6;
            string value = string.Empty;

            for (int i = 0; i < tamanho; i++)
            {
                Random random = new Random();
                int codigo = Convert.ToInt32(random.Next(48, 122).ToString());

                if ((codigo >= 48 && codigo <= 57) || codigo >= 97 && codigo <= 122)
                {
                    string _char = ((char) codigo).ToString();
                    if (!value.Contains(_char))
                    {
                        value += _char;
                    }
                    else
                    {
                        i--;
                    }
                }
                else
                {
                    i--;
                }
            }
            return value.ToUpper();
        }

        public static void LimparText(Control con)
        {
            foreach (Control c in con.Controls)
            {
                if (c is TextBox)
                    ((TextBox)c).Clear();
                else
                    LimparText(c);
                if (c is MaskedTextBox)
                    ((MaskedTextBox)c).Clear();
                else
                    LimparText(c);
                if (c is RadioButton)
                    ((RadioButton) c).Checked = false;
                else 
                    LimparText(c);
                if (c is CheckBox)
                    ((CheckBox)c).Checked = false;
                else
                    LimparText(c);
                if (c is DateTimePicker)
                    ((DateTimePicker)c).ResetText();
                else
                    LimparText(c);
                if (c is GroupBox)
                    ((GroupBox) c).Enabled = false;
                else
                    LimparText(c);
            }
        }

        public static string FormatTelefone(string telefone)
        {
            string result = "";
            if (String.IsNullOrEmpty(telefone))
            {
                return result;
            }
            telefone = telefone.Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", "");
            if (telefone.Length == 10)
            {
                result = String.Format("{0:(##) 9####-####}", Convert.ToInt64(telefone));
            }
            if (telefone.Length == 11)
            {
                result = String.Format("{0:(##) #####-####}", Convert.ToInt64(telefone));
            }
            return result;
        }
    }
}