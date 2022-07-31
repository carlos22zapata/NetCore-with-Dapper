using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

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

            var connectionString = configuration.GetConnectionString(bd);
                                    
            return connectionString;
        }

        [HttpGet]
        [Route("GetSelect")]
        public object GetSelect(string ConnectionName, string Command)
        {

            string connex = ConBD(ConnectionName);

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
            
            return null;
        }

    }
}
