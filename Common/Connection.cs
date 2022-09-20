using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ARIS_Net.Tools;

namespace Common
{
    
    public class Connection
    {
        static string BdEngine = "";
        static string ConnectionString = "";


        [Microsoft.AspNetCore.Authorization.Authorize]
        public string ConBD(string bd)
        {
            var conex = "";

            if (ConnectionString == "")
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

                conex = configuration.GetConnectionString(bd);

                bool isMySQL = false;

                if (bd.Length > 5)
                    isMySQL = bd.Substring(0, 5).ToUpper() == "MYSQL";

                int prueba = conex.IndexOf("Password=");

                int startText = conex.IndexOf("Password=") + 9;
                int endText = 0;
                string newConex = "";

                if (isMySQL)
                {
                    endText = (conex.Length - 1) - startText;
                    BdEngine = "MYSQL";
                    //HttpContext.Session.SetString(BdEngine, "MYSQL");

                    var passText0 = conex.Substring(startText, endText);
                    var passText1 = passText0.Split('.');
                    var passText2 = Decrypt(passText1[1], "Tecno$GO%4321", passText1[0]);

                    newConex = conex.Substring(0, startText) + passText2;
                }
                else
                {
                    endText = (conex.IndexOf(";Trusted_Connection=")) - startText;
                    BdEngine = "SQLSERVER";
                    //HttpContext.Session.SetString(BdEngine, "SQLSERVER");

                    var passText1 = conex.Substring(startText, endText).Split('.');
                    var passText2 = Decrypt(passText1[1], "Tecno$GO%4321", passText1[0]);

                    newConex = conex.Substring(0, startText) + passText2 + conex.Substring(conex.IndexOf(";Trusted_Connection="), conex.Length - conex.IndexOf(";Trusted_Connection="));
                }

                ConnectionString = newConex;
                //HttpContext.Session.SetString(ConnectionString, "Prueba");
                return newConex;
            }
            else
            {
                return ConnectionString;
            }
        }
    }
}
