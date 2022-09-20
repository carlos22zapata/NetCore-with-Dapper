using Common;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using static Common.Connection;

namespace ARIS_Net.Controllers
{
    public class ConnectionsController : Controller
    {
        private readonly IConection _connection;
        

        public ConnectionsController(IConection connection)
        {
            _connection = connection;
            
            //if (_db == null)
            //{
            //    _db = db;
            //}                          
        }

        public IActionResult Index()
        {
            return View();
        }       


        [HttpGet]
        [Route("GetSelect")]
        public object GetSelect(string ConnectionName, string Command)
        {
            //bool isMySQL = ConnectionName.Substring(0, 5).ToUpper() == "MYSQL";
            
            string connex = _connection.ConBD(ConnectionName);

            if (BdEngine == "MYSQL")
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
            else if (BdEngine == "SQLSERVER")
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


        /// <summary>
        /// Para importar un archivo CSV a una tabla 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ImportCsv")]
        public object ImportCsv(string ConnectionName, string path, string tableName)
        {
            //path = @"C:\Users\MyHP\Desktop\Prueba.csv";

            string connex = _connection.ConBD(ConnectionName);

            try
            {


                string Content = "";
                using (StreamReader srr = System.IO.File.OpenText(path))
                {
                    string s = "";
                    int line = 0;
                    while ((s = srr.ReadLine()) != null)
                    {
                        Content += ((line == 0 ? "{\"Head\":" : "\"Line" + line.ToString() + "\":") + "\"" + s + "\",");

                        if (line == 0)
                        {
                            string[] items = Content.Split(";");
                            (bool, string, Array) val = validateTable(tableName, items);

                            if (val.Item1 == false)
                                return val.Item2;

                            foreach (var item in val.Item3)
                            {
                                var prueba = item;
                            }
                        }

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

        [HttpGet]
        [Route("GetTableStructure")]
        public object GetTableStructure(string ConnectionName, string TableName)
        {
            string connex = _connection.ConBD(ConnectionName);

            //C:\Users\MyHP\Desktop\Prueba.csv

            if (BdEngine == "MYSQL")
            {
                using (IDbConnection db = new MySqlConnection(connex))
                {
                    string Command = "select COLUMN_NAME from information_schema.columns where table_name ='" + TableName + "'";
                    IEnumerable<dynamic> result = db.Query(Command).AsList();

                    return result;
                }
            }
            else if (BdEngine == "SQLSERVER")
            {
                using (IDbConnection db = new SqlConnection(connex))
                {
                    string Command = "SELECT COL.name AS TableColumn FROM dbo.syscolumns COL JOIN dbo.sysobjects OBJ ON OBJ.id = COL.id WHERE OBJ.name ='" + TableName + "'";
                    
                    var result = db.Query(Command).AsList();

                    return result;
                }
            }

            return null;
        }

        [HttpGet]
        [Route("GetFileStructure")]
        public object GetFileStructure(string path)
        {

            //path = @"C:\Users\MyHP\Desktop\Prueba.csv";

            using (StreamReader srr = System.IO.File.OpenText(path))
            {

                string[] items = srr.ReadLine().Split(";");

                List<string> resultList = new List<string>(); 

                foreach(var item in items)
                {
                    resultList.Add("{ FileColumn: " + item + " }");
                }

                //return JsonConvert.SerializeObject(resultList, Formatting.Indented);
                return resultList;
            }

            return null;
        }

        private (bool, string, Array) validateTable(string table, string[] items)
        {
            string connex = _connection.ConBD("MiSQLEngeni");

            if (BdEngine == "MYSQL")
            {
                using (IDbConnection db = new MySqlConnection(connex))
                {
                    string Command = "SELECT COL.name AS TableColumn FROM dbo.syscolumns COL JOIN dbo.sysobjects OBJ ON OBJ.id = COL.id WHERE OBJ.name ='" + "" + "'";

                    /* Aplicar esta consulta aquí ....
                        SELECT COLUMN_NAME AS columna FROM 
                        information_schema.columns WHERE 
                        table_schema = 'nombre_de_tu_base_de_datos' 
                        AND 
                        table_name = 'nombre_de_la_tabla';
                     */

                    var result = db.Query(Command).AsList();
                }
            }
            else if (BdEngine == "SQLSERVER")
            {
                using (IDbConnection db = new SqlConnection(connex))
                {
                    string Command = "SELECT COL.name AS TableColumn FROM dbo.syscolumns COL JOIN dbo.sysobjects OBJ ON OBJ.id = COL.id WHERE OBJ.name ='" + table + "'";
                    var result = db.Query(Command).AsList();

                    var l = result.AsList();
                    var ll = l[0];

                    if (result.Count != items.Length)
                        return (false, "No se tiene la misma cantidad de campos en el archivo y la tabla", null);
                    else
                        return (true, "Campos validos!", result.ToArray());
                }
            }

            return (false, "Error inesperado...", null);
        }


    }
}