using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ARIS_Net.Controllers
{
    public class ConnectionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Microsoft.AspNetCore.Authorization.Authorize]
        
        public string ConBD(string bd)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

            var conex = configuration.GetConnectionString(bd);

            bool isMySQL = bd.Substring(0, 5).ToUpper() == "MYSQL";

            int startText = conex.IndexOf("Password=") + 9;
            int endText = (isMySQL ? conex.Length-1 : conex.IndexOf(";Trusted_Connection=")) - startText;

            var passText1 = conex.Substring(startText, endText).Split('.');
            var passText2 = Decrypt(passText1[1], "Tecno$GO%4321", passText1[0]);

            string newConex = ""; 
            
            if(isMySQL)
            {
                newConex = conex.Substring(0, startText) + passText2;
            }
            else
            {
                newConex = conex.Substring(0, startText) + passText2 + conex.Substring(conex.IndexOf(";Trusted_Connection="), conex.Length - conex.IndexOf(";Trusted_Connection="));
            }
            

            return newConex;
        }

        [HttpGet]
        [Route("GetSelect")]
        public object GetSelect(string ConnectionName, string Command)
        {
            bool isMySQL = ConnectionName.Substring(0, 5).ToUpper() == "MYSQL";

            string connex = ConBD(ConnectionName);

            if(isMySQL)
            {
                using (IDbConnection db = new MySqlConnection(connex))
                {

                    db.Open();

                    try
                    {
                        var result = db.Query(Command).AsList();
                        return JsonConvert.SerializeObject(result, Formatting.Indented);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Hubo un error en la linea de comandos indicada al motor de base de datos, excepción: " + ex.Message);
                    }
                }
            }
            else
            {
                using (IDbConnection db = new SqlConnection(connex))
                {

                    db.Open();

                    try
                    {
                        var result = db.Query(Command).AsList();
                        return JsonConvert.SerializeObject(result, Formatting.Indented);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Hubo un error en la linea de comandos indicada al motor de base de datos, excepción: " + ex.Message);
                    }
                }
            }

            
            
            return null;
        }

        public static String Decrypt(String encryptedText, String key, String iv)
        {
            var encryptedBytes = Convert.FromBase64String(encryptedText);
            var aes = GetAES(key);
            aes.IV = Convert.FromBase64String(iv);
            return Encoding.UTF8.GetString(Decrypt(encryptedBytes, aes));
        }

        private static byte[] Decrypt(byte[] encryptedData, Aes aes)
        {
            return aes.CreateDecryptor()
                .TransformFinalBlock(encryptedData, 0, encryptedData.Length);
        }

        public static Aes GetAES(String secretKey)
        {
            int keysize = 256;

            var keyBytes = new byte[keysize / 8];
            var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
            Array.Copy(secretKeyBytes, keyBytes, Math.Min(keyBytes.Length, secretKeyBytes.Length));

            var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = keysize;
            aes.BlockSize = 128;//AES es siempre 128 el blocksize
            aes.Key = keyBytes;
            aes.GenerateIV();

            return aes;
        }

    }
}
