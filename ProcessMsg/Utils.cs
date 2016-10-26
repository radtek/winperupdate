﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace ProcessMsg
{
    public static class Utils
    {
        /// Encripta una cadena
        public static string Encriptar(this string _cadenaAencriptar)
        {
            string result = string.Empty;
            byte[] encryted = System.Text.Encoding.Unicode.GetBytes(_cadenaAencriptar);
            result = Convert.ToBase64String(encryted);
            return result;
        }

        /// Esta función desencripta la cadena que le envíamos en el parámentro de entrada.
        public static string DesEncriptar(this string _cadenaAdesencriptar)
        {
            string result = string.Empty;
            byte[] decryted = Convert.FromBase64String(_cadenaAdesencriptar);
            //result = System.Text.Encoding.Unicode.GetString(decryted, 0, decryted.ToArray().Length);
            result = System.Text.Encoding.Unicode.GetString(decryted);
            return result;
        }

        public static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        public static Boolean SendMail(String mensaje, String asunto, String mailctt)
        {
            string userMail = ConfigurationManager.AppSettings["userMail"];
            string pwdMail = ConfigurationManager.AppSettings["pwdMail"];
            string fromMail = ConfigurationManager.AppSettings["fromMail"];
            string HostMail = ConfigurationManager.AppSettings["HostMail"];

            var correo = new System.Net.Mail.MailMessage();
            correo.From = new System.Net.Mail.MailAddress(userMail, fromMail);
            correo.To.Add(new System.Net.Mail.MailAddress(mailctt));
            correo.Subject = asunto;
            correo.IsBodyHtml = true;
            correo.BodyEncoding = System.Text.Encoding.GetEncoding("ISO-8859-1");
            correo.Body = mensaje;

            var smtp = new System.Net.Mail.SmtpClient();
            smtp.Host = HostMail;
            //smtp.Port = 50001;
            smtp.Credentials = new System.Net.NetworkCredential(userMail, pwdMail);
            smtp.EnableSsl = false;

            try
            {
                smtp.Send(correo);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static String ReadPlantilla(String sPathPlantilla)
        {
            string textoHtml = "";
            var sr = new System.IO.StreamReader(sPathPlantilla);
            textoHtml = sr.ReadToEnd();
            sr.Close();

            return textoHtml;
        }

        public static string GetMd5Hash(string input)
        {
            byte[] data = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
        public static bool VerificarMd5Hash(string input, string hash)
        {
            string hashOfInput = GetMd5Hash(input);
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            return (0 == comparer.Compare(hashOfInput, hash));
        }

        public static string GenerarLicencia(int NumFolio, int estmtc, string mesini, string nrotrbc
                                            , string nrotrbh, string nrousr)
        {
            string fStr = string.Format("{0}{1}{2}{3}{4}{5}", NumFolio, estmtc, mesini,nrotrbc,nrotrbh,nrousr);
            string hash = GetMd5Hash(fStr);
            string licencia = "";
            for (int i = 0; i < hash.Length; i++)
            {
                licencia += hash[i];
                if ((i + 1) % 8 == 0 && (i + 1) < hash.Length)
                {
                    licencia += "-";
                }
            }
            return licencia.ToUpper();
        }
    }
}
