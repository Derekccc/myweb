using Logger.Logging;
using MY_WEBSITE_API.Controllers.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace MY_WEBSITE_API.Classes
{
    public class Password
    {
        public static string decryptedPass(string passOri)
        {
            string password = string.Empty;
            var keybytes = Encoding.UTF8.GetBytes("7061737323313233");
            var iv = Encoding.UTF8.GetBytes("7061737323313233");

            var encrypted = Convert.FromBase64String(passOri);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = keybytes;
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(encrypted))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            password = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return password;
        }

        public static string encryptedPass(string user, string passOri)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            try
            {
                string passText = decryptedPass(passOri);

                byte[] hash = SHA1.Create().ComputeHash(Encoding.Default.GetBytes(passText));
                StringBuilder stringBuilder = new StringBuilder();
                for (int index = 0; index < hash.Length; ++index)
                    stringBuilder.Append(hash[index].ToString());
                return stringBuilder.ToString();
            }
            catch (Exception ex)
            {
                string values = JsonConvert.SerializeObject(passOri, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                apiCommon.WebApiLog(LogType.ERROR_TYPE, user, apiCommon.MethodName(), "Values: " + values + "\r\n\r\n" + ex.ToString());
                return string.Empty;

            }


        }
      
    }
}