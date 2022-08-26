using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using static ARIS_Net.Tools;

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

        [HttpPost]
        [Route("CreateCsv")]
        public object CreateCsv(string path)
        {
            path = @"C:\Users\MyHP\Desktop\Prueba.csv";

            try
            {
                string Content = "";
                using (StreamReader srr = System.IO.File.OpenText(path))
                {
                    string s = "";
                    int line = 0;
                    while((s = srr.ReadLine()) !=null)
                    {
                        Content += ((line == 0 ? "{\"Head\":" : "\"Line" + line.ToString() + "\":") + "\"" + s + "\",");
                        line++;
                    }
                }

                Content = Content.Substring(0, Content.Length - 1) + "}";

                return Content; //"{" + Content.Replace(';',',') + "}"; //JsonConvert.SerializeObject(Content, Formatting.Indented));
            }
            catch (FileLoadException f)
            {
                Console.WriteLine("Hubo un problema con la dirección del archivo, error: " + f.Message);
            }

            return "";
        }



    }
}
