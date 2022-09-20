using ARIS_Net.Models;
using Common;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using static Common.Connection;

namespace ARIS_Net.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    public class ImportFileController : ControllerBase
    {

        private readonly IConection _connection;
        
        public ImportFileController(IConection connection)
        {
            _connection = connection;
        }

        [HttpGet]
        [Route("GetModelFiles")]
        public object GetModelFiles(string ConnectionName, int? Id)
        {
            List<ImportModel> ModelFile = new List<ImportModel>();

            string connex = _connection.ConBD(ConnectionName);

            using (IDbConnection db = new MySqlConnection(connex))
            {
                if(Id == null || Id == 0)
                {
                    var Mod = db.Query<object>("Select Id, Name, JsonModel, Site From ImportModel").ToList();
                    return Mod;
                }
                else
                {
                    var Mod = db.Query<object>("Select Id, Name, JsonModel, Site From ImportModel where Id = " + Id.ToString()).ToList();
                    return Mod;
                }
            }

            return null;
        }

        [HttpGet]
        [Route("GetAllTables")]
        public object GetAllTables(string ConnectionName)
        {
            List<TableModels> ModelFile = new List<TableModels>();

            string connex = _connection.ConBD(ConnectionName);

            using (IDbConnection db = new MySqlConnection(connex))
            {
                ModelFile = db.Query<TableModels>(@"SELECT TABLE_NAME name FROM information_schema.tables where TABLE_SCHEMA  = '" + ConnectionName + "'" ).ToList();
            }

            return ModelFile;
        }

        /// <summary>
        /// Inserta un nuevo registro en la tabla de los modelos de los archivos
        /// </summary>
        /// <param name="ConnectionName"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("InsertModelFiles")]
        public string InsertModelFiles(string ConnectionName, [FromBody] ImportModel record)
        {
            string connex = "";

            try
            {
                connex = _connection.ConBD(ConnectionName);

                string modelString = "[";

                foreach (var item in record.JsonModel)
                {
                    modelString = modelString + "{fieldBD:" + item.fieldBD + ",fieldFile:" + item.fieldFile + "},";
                }

                int end = modelString.Length;

                modelString = modelString.Substring(0, modelString.Length - 1) + "]";

                using (IDbConnection db = new MySqlConnection(connex))
                {
                    string sqlExists = "select count(*) from importmodel where Name = '" + record.Name + "'";
                    int isExists = db.Query<int>(sqlExists).SingleOrDefault();
                    string sql = "";

                    if(isExists > 0)
                    {
                        sql = "Update importmodel set Site = '" + record.Site + "', JsonModel = '" + modelString + "' where Name = '" + record.Name + "'";

                        db.Query<ImportModel>(sql, record).SingleOrDefault();
                        return "Registro actualizado satisfactoriamente!";
                    }
                    else
                    {
                        sql = "INSERT INTO importmodel (Name, CreationDate, Enable, Site, JsonModel) VALUES('" +
                        record.Name + "', '" +
                        record.CreateDate + "', " +
                        record.Enable + ", '" +
                        record.Site + "', '" +
                        modelString + "'); ";

                        db.Query<ImportModel>(sql, record).SingleOrDefault();
                        return "Registro guardado satisfactoriamente!";
                    } 
                                     
                }
               
            }
            catch (Exception ex)
            {
                return null;
            }

        }


    }
}
