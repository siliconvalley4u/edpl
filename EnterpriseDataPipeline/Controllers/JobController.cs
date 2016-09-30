using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EnterpriseDataPipeline.Models;
using DynamicMVC.UI.Controllers;
using System.Threading.Tasks;

using System.Collections;
using System.Text;
using System.Web.Routing;
using DynamicMVC.Business.Attributes;
using DynamicMVC.Business.Models;
using DynamicMVC.Data;
using DynamicMVC.UI.Extensions;
using DynamicMVC.UI.ViewModels;

using DynamicMVC.UI;

using System.Data.Common;

using SSHWrapper;

using MySql.Data;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Text.RegularExpressions;

using Npgsql;

using System.Data;
using System.Data.Odbc;

using Renci.SshNet;
using System.Net.Http;
using System.IO;

using EnterpriseDataPipeline.ViewModel;
using System.Collections.ObjectModel;

namespace EnterpriseDataPipeline.Controllers
{
    [Authorize(Roles = "Operator, Admin")]
    public class JobController : Controller
    {
        public ActionResult Index()
        {
            StorageViewModel viewModelStorage = new StorageViewModel();

            try
            {
                ApplicationDbContext db = new ApplicationDbContext();
                List<Storage> listStorage = db.Storage.OrderBy(s => s.Name).ToList();

                var selectList = new SelectList(listStorage, "Id", "Name");
                var vm = new StorageViewModel
                {
                    Storage = selectList,
                    SourceId = 1,           //Default Source
                    DestinationId = 6       //Default Destination
                };
                return View(vm);
            }
            catch (Exception ex)
            {
                string str = ex.Message;
                return View("Error");
            }
        }

        #region "Get Database"
        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        public ActionResult GetDB(string storage)
        //public JsonResult GetDB(string storage)
        {
            JsonResult jsonResult = new JsonResult();
            string myConnectionString = "";

            try
            {
                ApplicationDbContext db = new ApplicationDbContext();
                //var tbStorage = db.Storage.Where(s => s.Name == storage).ToList();
                //var tbStorage = db.Storage.First(t => t.Name.Equals(storage, System.StringComparison.InvariantCultureIgnoreCase));
                int storageId = int.Parse(storage);
                var tbStorage = db.Storage.Where(s => s.Id == storageId).ToList(); 
                string strStorageType = tbStorage[0].Type;

                //myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                //jsonResult = GetStorageType(myConnectionString, storage);

                //switch (storage)
                switch (strStorageType)
                {
                    case "MySQL":
                        myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionMySQL"].ConnectionString;
                        myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                        myConnectionString = myConnectionString.Replace("DATABASE", tbStorage.ToList()[0].DBName);
                        myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                        myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                        myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);
                        jsonResult = GetMySQLDB(myConnectionString, tbStorage[0].Id);
                        break;
                    case "Hive":
                        myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionHive"].ConnectionString;
                        myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                        myConnectionString = myConnectionString.Replace("DATABASE", tbStorage.ToList()[0].DBName);
                        myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                        myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                        myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);
                        jsonResult = GetHiveDB(myConnectionString, tbStorage[0].Id);
                        break;
                    case "HBase":
                        myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionHBase"].ConnectionString;
                        myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                        myConnectionString = myConnectionString.Replace("DATABASE", tbStorage.ToList()[0].DBName);
                        myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                        myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                        myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);
                        jsonResult = GetHBaseDB2(myConnectionString, storage);
                        break;
                    case "Postgres":
                        myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionPostgreSQL"].ConnectionString;
                        myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                        myConnectionString = myConnectionString.Replace("DATABASE", tbStorage.ToList()[0].DBName);
                        myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                        myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                        myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);
                        jsonResult = GetPostgresDB(myConnectionString, tbStorage[0].Id);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                string strError = ex.Message;
                jsonResult = new JsonResult()
                {
                    Data = new { success = false, val = strError },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            return jsonResult;
        }
        private JsonResult GetMySQLDB(string myConnectionString, int storageId)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            try
            {
                ApplicationDbContext db = new ApplicationDbContext();
                //var tbStorage = db.Storage.Where(s => s.Name == strStorage);

                //myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                //myConnectionString = myConnectionString.Replace("DATABASE", tbStorage.ToList()[0].DBName);
                //myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                //myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                //myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);

                using (var conn = new MySql.Data.MySqlClient.MySqlConnection(myConnectionString))
                {
                    conn.Open();

                    string query = "show databases";
                    //IEnumerable<object[]> modelDB = null;

                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            var modelDB = Read(reader).ToList();
                            //if(modelDB.Count > 0)
                            //modelDB = Read(reader).ToList();

                            //Convert to list.
                            //List<object[]> list = modelDB.ToList();

                            //Add new item to list.
                            //list.Add(new object[] { "all" });

                            //Cast list to IEnumerable
                            //modelDB = (IEnumerable<object[]>)list;

                            //model.Add(new IEnumerable<object[]> { "all" });    //added by Anthony Lai on 2015-08-10 to allow move all tables
                            //if(modelDB.Count > 0)
                                //ViewBag.SourceDB = modelDB.ElementAt(0).ToString();
                                //ViewBag.SourceDB = modelDB[0][0].ToString();

                            //jsonResult = new JsonResult()
                            //{
                            //    Data = new { success = true, val = modelDB },
                            //    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                            //};

                            if (modelDB.Count > 0)
                            {
                                var sClusterParameters = db.Storage.Where(i => i.Id == storageId);
                                modelDB.Add(new object[] { sClusterParameters.ToList()[0].DBName });    //added by Anthony Lai on 2015-09-01 to select default database

                                jsonResult = new JsonResult()
                                {
                                    Data = new { success = true, val = modelDB },
                                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                                };
                            }
                        }
                    }

                    //conn.Close();

                    //query = "show tables";
                    //IEnumerable<object[]> modelTB = null;

                    //using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn))
                    //{
                    //    using (var reader = cmd.ExecuteReader())
                    //    {
                    //        //var model = Read(reader).ToList();
                    //        //model.Add(new object[] {"all"});    //added by Anthony Lai on 2015-08-10 to allow move all tables
                    //        //jsonResult = new JsonResult()
                    //        //{
                    //        //    Data = new { success = true, val = model },
                    //        //    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    //        //};

                    //        //var model = Read(reader).ToList();
                    //        modelTB = Read(reader).ToList();

                    //        //Convert to list.
                    //        List<object[]> list = modelTB.ToList();

                    //        //Add new item to list.
                    //        list.Add(new object[] { "all" });

                    //        //Cast list to IEnumerable
                    //        modelTB = (IEnumerable<object[]>)list;
                    //    }
                    //}

                    //jsonResult = new JsonResult()
                    //{
                    //    Data = new { success = true, databases = modelDB, tables = modelTB },
                    //    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    //};
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                string strError = ex.Message;
                jsonResult = new JsonResult()
                {
                    Data = new { success = false, val = strError },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            return jsonResult;
        }
        private JsonResult GetHiveDB(string myConnectionString, int storageId)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            try
            {
                ApplicationDbContext db = new ApplicationDbContext();
                //var tbStorage = db.Storage.Where(s => s.Name == strStorage);

                //myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                //myConnectionString = myConnectionString.Replace("DATABASE", tbStorage.ToList()[0].DBName);
                //myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                //myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                //myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);

                using (OdbcConnection conn = new OdbcConnection(myConnectionString))
                {
                    conn.Open();

                    string query = "show databases";

                    using (var cmd = new OdbcCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            var modelDB = Read(reader).ToList();

                            if (modelDB.Count > 0)
                            {
                                var sClusterParameters = db.Storage.Where(i => i.Id == storageId);
                                modelDB.Add(new object[] { sClusterParameters.ToList()[0].DBName });    //added by Anthony Lai on 2015-09-01 to select default database

                                jsonResult = new JsonResult()
                                {
                                    Data = new { success = true, val = modelDB },
                                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                                };
                            }

                            //jsonResult = new JsonResult()
                            //{
                            //    Data = new { success = true, val = modelDB },
                            //    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                            //};
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string strError = ex.Message;
                jsonResult = new JsonResult()
                {
                    Data = new { success = false, val = strError },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            return jsonResult;
        }
        private JsonResult GetHBaseDB(string storage)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            ApplicationDbContext db = new ApplicationDbContext();
            string strClusterIP = "";
            string strClusterUserName = "";
            string strClusterPassword = "";
            string result = "";
            string error = "";
            //string strCmd = "\\cp tmpHBaseDB.ini tmpHBaseDB_TB.ini; echo \"list_namespace_tables '" + query + "'\" >> tmpHBaseDB_TB.ini; cat tmpHBaseDB_TB.ini | hbase shell";

            string strCmd = "cat /root/tmpHBaseDB.ini | hbase shell";

            //Find Spark Parameters
            var sClusterParameters = db.Storage.Where(i => i.Name == storage);
            strClusterIP = sClusterParameters.ToList()[0].IPAddress;
            strClusterUserName = sClusterParameters.ToList()[0].UserName;
            strClusterPassword = sClusterParameters.ToList()[0].Password;

            var values = new List<object>();
            try
            {
                ConnectionInfo connectionInfo = new PasswordConnectionInfo(strClusterIP, strClusterUserName, strClusterPassword);
                using (SshClient client = new SshClient(connectionInfo))
                {
                    if (!client.IsConnected)
                        client.Connect();

                    if (client.IsConnected)
                    {
                        SshCommand cmd = client.CreateCommand(strCmd);
                        cmd.Execute();
                        error += cmd.Error;
                        result += cmd.Result;

                        int idxcol = result.IndexOf("NAMESPACE");
                        result = result.Substring(idxcol);

                        var modelTB = ReadHBaseMetaData(result).ToList();

                        modelTB.Add(new object[] { sClusterParameters.ToList()[0].DBName });    //added by Anthony Lai on 2015-09-01 to select default database

                        jsonResult = new JsonResult()
                        {
                            Data = new { success = true, val = modelTB },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                string strError = ex.Message;
                var err = error.Split('\n');
                jsonResult = new JsonResult()
                {
                    Data = new { success = false, val = err[0] },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            return jsonResult;
        }
        private JsonResult GetHBaseDB2(string myConnectionString, string storage)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            try
            {
                ApplicationDbContext db = new ApplicationDbContext();

                using (OdbcConnection conn = new OdbcConnection(myConnectionString))
                {
                    conn.Open();

                    //string query = "show databases";
                    //string query = "select * from \"SIMBA_METATABLE\"";

                    string query = "SELECT SCHEMA_NAME FROM \"SCHEMATA\" ORDER BY SCHEMA_NAME;";

                    using (var cmd = new OdbcCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            var modelDB = Read(reader).ToList();

                            if (modelDB.Count > 0)
                            {
                                var sClusterParameters = db.Storage.Where(i => i.Name == storage);
                                modelDB.Add(new object[] { sClusterParameters.ToList()[0].DBName });    //added by Anthony Lai on 2015-09-01 to select default database

                                jsonResult = new JsonResult()
                                {
                                    Data = new { success = true, val = modelDB },
                                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                                };
                            }

                            //jsonResult = new JsonResult()
                            //{
                            //    Data = new { success = true, val = modelDB },
                            //    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                            //};
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string strError = ex.Message;
                jsonResult = new JsonResult()
                {
                    Data = new { success = false, val = strError },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            var modelDB_new = new List<object[]>();
            modelDB_new.Add(new object[] { "default" });    //added by Anthony Lai on 2015-09-01 to select default database
            modelDB_new.Add(new object[] { "" });           //added by Anthony Lai on 2015-09-01 to select default database

            jsonResult = new JsonResult()
            {
                Data = new { success = true, val = modelDB_new },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            return jsonResult;
        }
        private JsonResult GetPostgresDB(string myConnectionString, int storageId)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            try
            {
                ApplicationDbContext db = new ApplicationDbContext();
                //var tbStorage = db.Storage.Where(s => s.Name == strStorage);

                //myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                //myConnectionString = myConnectionString.Replace("DATABASE", tbStorage.ToList()[0].DBName);
                //myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                //myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                //myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);

                //using (var conn = new NpgsqlConnection("Host=myserver;Username=mylogin;Password=mypass;Database=mydatabase"))
                //using (OdbcConnection conn = new OdbcConnection(myConnectionString))
                using (var conn = new NpgsqlConnection(myConnectionString))
                {
                    conn.Open();

                    //string query = "show databases";

                    string query = "SELECT datname FROM pg_database WHERE datistemplate = false;";

                    //using (var cmd = new OdbcCommand(query, conn))
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            var modelDB = Read(reader).ToList();
                            if (modelDB.Count > 0)
                            {
                                var sClusterParameters = db.Storage.Where(i => i.Id == storageId);
                                modelDB.Add(new object[] { sClusterParameters.ToList()[0].DBName });    //added by Anthony Lai on 2065-04-26 to select default database

                                jsonResult = new JsonResult()
                                {
                                    Data = new { success = true, val = modelDB },
                                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                                };
                            }

                            //jsonResult = new JsonResult()
                            //{
                            //    Data = new { success = true, val = modelDB },
                            //    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                            //};
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string strError = ex.Message;
                jsonResult = new JsonResult()
                {
                    Data = new { success = false, val = strError },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            return jsonResult;
        }
        #endregion "Get Database"

        private JsonResult GetStorageType(string myConnectionString, string strStorage)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            try
            {
                using (var conn = new MySql.Data.MySqlClient.MySqlConnection(myConnectionString))
                {
                    conn.Open();

                    string query = "SELECT * FROM storages WHERE Name = BINARY '" + strStorage + "'";

                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            var modelDB = Read(reader).ToList();

                            jsonResult = new JsonResult()
                            {
                                Data = new { success = true, val = modelDB },
                                JsonRequestBehavior = JsonRequestBehavior.AllowGet
                            };
                        }
                    }
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                string strError = ex.Message;
                jsonResult = new JsonResult()
                {
                    Data = new { success = false, val = strError },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            return jsonResult;
        }

        #region "Get Database Table"
        //public async Task<ActionResult> GetDBTable(string dbTable)
        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        public ActionResult GetDBTable(string storage, string database)
        //public JsonResult GetDBTable(string storage, string database)
        {
            JsonResult jsonResult = new JsonResult();
            string myConnectionString = "";

            //string source = Request.Form["Source"];//get the source selected

            try
            {
                ApplicationDbContext db = new ApplicationDbContext();
                //var tbStorage = db.Storage.Where(s => s.Name == storage).ToList();
                //string strStorage = tbStorage[0].Type;
                //var tbStorage = db.Storage.First(t => t.Name.Equals(storage, System.StringComparison.InvariantCultureIgnoreCase));
                //var tbStorage = db.Storage.Where(t => t.Name.Equals(storage, System.StringComparison.CurrentCulture)).ToList();

                int storageId = int.Parse(storage);
                var tbStorage = db.Storage.Where(s => s.Id == storageId).ToList();

                string strStorageType = tbStorage[0].Type;

                //switch (storage)
                switch (strStorageType)
                {
                    case "MySQL":
                        myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionMySQL"].ConnectionString;
                        //myConnectionString = myConnectionString.Replace("BooksDB", database);
                        myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                        myConnectionString = myConnectionString.Replace("DATABASE", database);
                        myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                        myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                        myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);
                        jsonResult = GetMySQLDBTable(myConnectionString, storage, database);
                        break;
                    case "HBase":
                        myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionHBase"].ConnectionString;
                        //myConnectionString = myConnectionString.Replace("database=default", "database=" + database);
                        myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                        myConnectionString = myConnectionString.Replace("DATABASE", database);
                        myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                        myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                        myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);
                        jsonResult = GetHBaseDBTable(tbStorage[0].Id, database);
                        break;
                    case "Hive":
                        myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionHive"].ConnectionString;
                        //myConnectionString = myConnectionString.Replace("database=default", "database=" + database);
                        myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                        myConnectionString = myConnectionString.Replace("DATABASE", database);
                        myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                        myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                        myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);
                        jsonResult = GetHiveDBTable(myConnectionString, storage, database);
                        break;
                    case "Postgres":
                        myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionPostgreSQL"].ConnectionString;
                        myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                        myConnectionString = myConnectionString.Replace("DATABASE", database);
                        myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                        myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                        myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);
                        jsonResult = GetPostgresDBTable(myConnectionString, storage, database);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                string strError = ex.Message;
                jsonResult = new JsonResult()
                {
                    Data = new { success = false, val = strError },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            return jsonResult;
        } 
        private JsonResult GetMySQLDBTable(string myConnectionString, string strStorage, string strDatabase)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            try
            {
                //ApplicationDbContext db = new ApplicationDbContext();
                //var tbStorage = db.Storage.Where(s => s.Name == strStorage);

                //myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                //myConnectionString = myConnectionString.Replace("DATABASE", strDatabase);
                //myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                //myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                //myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);


                using (var conn = new MySql.Data.MySqlClient.MySqlConnection(myConnectionString))
                {
                    conn.Open();

                    //string query = "show databases";
                    //IEnumerable<object[]> modelDB = null;

                    //using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn))
                    //{
                    //    using (var reader = cmd.ExecuteReader())
                    //    {
                    //        //var model = Read(reader).ToList();
                    //        modelDB = Read(reader).ToList();

                    //        //Convert to list.
                    //        //List<object[]> list = modelDB.ToList();

                    //        //Add new item to list.
                    //        //list.Add(new object[] { "all" });

                    //        //Cast list to IEnumerable
                    //        //modelDB = (IEnumerable<object[]>)list;

                    //        //model.Add(new IEnumerable<object[]> { "all" });    //added by Anthony Lai on 2015-08-10 to allow move all tables
                    //        //jsonResult = new JsonResult()
                    //        //{
                    //        //    Data = new { success = true, val = model },
                    //        //    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    //        //};
                    //    }
                    //}

                    string query = "show tables";
                    //IEnumerable<object[]> modelTB = null;

                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            var modelTB = Read(reader).ToList();
                            if (modelTB.Count > 0)
                            {
                                modelTB.Add(new object[] { "all" });    //added by Anthony Lai on 2015-08-10 to allow move all tables
                                jsonResult = new JsonResult()
                                {
                                    Data = new { success = true, val = modelTB },
                                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                                };
                            }

                            //var model = Read(reader).ToList();
                            //modelTB = Read(reader).ToList();

                            //Convert to list.
                            //List<object[]> list = modelTB.ToList();

                            //Add new item to list.
                            //list.Add(new object[] { "all" });

                            //Cast list to IEnumerable
                            //modelTB = (IEnumerable<object[]>)list;
                        }
                    }

                    //jsonResult = new JsonResult()
                    //{
                    //    Data = new { success = true, databases = modelDB, tables = modelTB },
                    //    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    //};

                    //conn.Close();
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                string strError = ex.Message;
                jsonResult = new JsonResult()
                {
                    Data = new { success = false, val = strError },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            return jsonResult;
        }
        private JsonResult GetHBaseDBTable(int storageId, string database)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            //ApplicationDbContext db = new ApplicationDbContext();
            string strClusterIP = "";
            string strPort = "";
            string strClusterUserName = "";
            string strClusterPassword = "";
            string result = "";
            string error = "";
            //string strCmd = "\\cp tmpHBaseDBTB.ini tmpHBaseDB_TB.ini; echo \"list_namespace_tables '" + database + "'\" >> tmpHBaseDB_TB.ini; cat tmpHBaseDB_TB.ini | hbase shell";
            //string strCmd = "curl -H \"Accept: application/json\" http://198.89.115.49:20550/";
            string strCmd = "curl -H \"Accept: application/json\" http://<IP_ADDRESS>/";


            //string strCmd = "cat /root/tmpHBaseDB.ini | hbase shell";

            //Find Spark Parameters
            ApplicationDbContext db = new ApplicationDbContext();
            var sClusterParameters = db.Storage.Where(i => i.Id == storageId);
            strClusterIP = sClusterParameters.ToList()[0].IPAddress;
            strPort = sClusterParameters.ToList()[0].Port;
            strClusterUserName = sClusterParameters.ToList()[0].UserName;
            strClusterPassword = sClusterParameters.ToList()[0].Password;

            //strCmd = strCmd.Replace("<IP_ADDRESS>", "198.89.115.49" + ":" + "20550");
            strCmd = strCmd.Replace("<IP_ADDRESS>", strClusterIP + ":" + strPort);

            var values = new List<object>();
            try
            {
                ConnectionInfo connectionInfo = new PasswordConnectionInfo(strClusterIP, strClusterUserName, strClusterPassword);
                using (SshClient client = new SshClient(connectionInfo))
                {
                    if (!client.IsConnected)
                        client.Connect();

                    if (client.IsConnected)
                    {
                        SshCommand cmd = client.CreateCommand(strCmd);
                        cmd.Execute();
                        error += cmd.Error;
                        result += cmd.Result;

                        //int idxcol = result.IndexOf("TABLE");
                        result = result.Replace("{", "").Replace("}", "").Replace("[", "").Replace("]", "").Replace("\"", "");
                        //int idxcol = result.IndexOf("table:");
                        //result = result.Substring(idxcol);
                        result = result.Replace("table:", "");

                        var modelTB = ReadHBaseMetaData(result);

                        jsonResult = new JsonResult()
                        {
                            Data = new { success = true, val = modelTB },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                string strError = ex.Message;
                var err = error.Split('\n');
                jsonResult = new JsonResult()
                {
                    Data = new { success = false, val = err[0] },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            return jsonResult;
        }
        private JsonResult GetHiveDBTable(string myConnectionString, string strStorage, string strDatabase)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            try
            {
                //ApplicationDbContext db = new ApplicationDbContext();
                //var tbStorage = db.Storage.Where(s => s.Name == strStorage);

                //myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                ////myConnectionString = myConnectionString.Replace("DATABASE", tbStorage.ToList()[0].DBName);
                //myConnectionString = myConnectionString.Replace("DATABASE", strDatabase);
                //myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                //myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                //myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);

                using (OdbcConnection conn = new OdbcConnection(myConnectionString))
                {
                    conn.Open();

                    string query = "show tables";

                    using (var cmd = new OdbcCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            var modelTB = Read(reader).ToList();
                            if (modelTB.Count > 0)
                            {
                                //modelTB.Add(new object[] { "all" });    //added by Anthony Lai on 2015-08-10 to allow move all tables
                                jsonResult = new JsonResult()
                                {
                                    Data = new { success = true, val = modelTB },
                                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string strError = ex.Message;
                jsonResult = new JsonResult()
                {
                    Data = new { success = false, val = strError },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            return jsonResult;
        }
        private JsonResult GetPostgresDBTable(string myConnectionString, string strStorage, string strDatabase)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            try
            {
                //ApplicationDbContext db = new ApplicationDbContext();
                //var tbStorage = db.Storage.Where(s => s.Name == strStorage);

                //myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                //myConnectionString = myConnectionString.Replace("DATABASE", strDatabase);
                //myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                //myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                //myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);

                //using (var conn = new NpgsqlConnection("Host=myserver;Username=mylogin;Password=mypass;Database=mydatabase"))
                //using (OdbcConnection conn = new OdbcConnection(myConnectionString))
                using (var conn = new NpgsqlConnection(myConnectionString))
                {
                    conn.Open();

                    //string query = "show databases";

                    string query = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';";

                    //using (var cmd = new OdbcCommand(query, conn))
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            var modelDB = Read(reader).ToList();
                            jsonResult = new JsonResult()
                            {
                                Data = new { success = true, val = modelDB },
                                JsonRequestBehavior = JsonRequestBehavior.AllowGet
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string strError = ex.Message;
                jsonResult = new JsonResult()
                {
                    Data = new { success = false, val = strError },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            return jsonResult;
        }
        private static IEnumerable<object[]> Read(DbDataReader reader)
        {
            while (reader.Read())
            {
                var values = new List<object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    values.Add(reader.GetValue(i));
                }
                yield return values.ToArray();
            }
        }
        #endregion "Get Database Table"

        //
        // POST: /Job/RunJob
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> RunJob(string sourceTable, string destinationTable)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            string result = "";


            string strClusterSourceIP = "";
            string strClusterUserName = "";
            string strClusterPassword = "";
            string strClusterJobDirectory = "";

            string strSourceIP = "";
            string strSourceUserName = "";
            string strSourcePassword = "";
            string strSourceDatabase = "";
            string strSourceTable = "";

            string strDestinationIP = "";
            string strDestinationUserName = "";
            string strDestinationPassword = "";
            string strDestinationDatabase = "";
            string strDestinationTable = "";

            string strJobName = "";

            string source = Request.Form["Source"];//get the source selected
            int sourceStorageId = int.Parse(source);
            string destination = Request.Form["Destination"];//get the destination selected
            int destinationStorageId = int.Parse(destination);

            string dbSourceDB = Request.Form["dbSourceDB"];//get the source table selected
            string dbDestinationDB = Request.Form["dbDestinationDB"];//get the destination table selected

            string dbSourceTable = Request.Form["dbSourceTable"];//get the source table selected
            string dbDestinationTable = Request.Form["dbDestinationTable"];//get the destination table selected

            string key = Guid.NewGuid().ToString();
            ApplicationDbContext db = new ApplicationDbContext();

            var sourceParameters = db.Storage.Where(i => i.Id == sourceStorageId).ToList();
            var destinationParameters = db.Storage.Where(i => i.Id == destinationStorageId).ToList();

            //Added by Anthony Lai to handle data transfer to HBase
            //if (destination.ToUpper().Contains("HBASE"))
            if (!sourceParameters[0].Type.ToUpper().Contains("POSTGRES") && destinationParameters[0].Type.ToUpper().Contains("HBASE"))
            {
                //Source Parameters
                //var sParameters = db.Storage.Where(i => i.Name == destination);
                var sParameters = db.Storage.Where(i => i.Id == destinationStorageId);
                strSourceIP = sParameters.ToList()[0].IPAddress;
                strSourceUserName = sParameters.ToList()[0].UserName;
                strSourcePassword = sParameters.ToList()[0].Password;
                strSourceDatabase = sParameters.ToList()[0].DBName;
                strSourceTable = sParameters.ToList()[0].TableName;

                //Destination Parameters
                //var dParameters = db.Storage.Where(i => i.Name == source);
                var dParameters = db.Storage.Where(i => i.Id == sourceStorageId);
                strDestinationIP = dParameters.ToList()[0].IPAddress;
                strDestinationUserName = dParameters.ToList()[0].UserName;
                strDestinationPassword = dParameters.ToList()[0].Password;
                strDestinationDatabase = dParameters.ToList()[0].DBName;
                if (dbSourceDB != null)
                {
                    strDestinationDatabase = dbSourceDB;
                }
                strDestinationTable = dParameters.ToList()[0].TableName;
                if (dbSourceTable != null)
                {
                    strDestinationTable = dbSourceTable;
                }
                    
                string strTmpResult = "mapreduce.ImportJobBase: Transferred 0 bytes";
                //DoCreateJobStatus(key, source, strDestinationIP, strDestinationTable, destination, strSourceIP, strSourceTable, strJobName, db);
                DoCreateJobStatus(key, dParameters.ToList()[0].Name, strDestinationIP, strDestinationTable, sParameters.ToList()[0].Name, strSourceIP, strSourceTable, strJobName, db);
                result = doHBaseDataTransfer(sourceStorageId, dbSourceDB, dbSourceTable, destinationStorageId, dbDestinationDB, dbDestinationTable, ref strTmpResult);
                    
                Regex regex = new Regex(@"(INFO mapreduce.Job:[\s*\S*]*completed successfully)");
                bool isMatched = regex.IsMatch(result);
                if (isMatched)
                {
                    result = "Transfer Successfully";

                    jsonResult = new JsonResult()
                    {
                        Data = new { success = true, val = result },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else 
                {
                    result = "Transfer Failed";

                    jsonResult = new JsonResult()
                    {
                        Data = new { success = false, val = result },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }

                DoUpdateJobStatus(key, result, strTmpResult);
                return jsonResult;
            }

            //Added by Anthony Lai to handle data transfer from Postgres to HBase
            //else if (source.ToUpper().Contains("POSTGRES"))
            else if (sourceParameters[0].Type.ToUpper().Contains("POSTGRES"))
            {
                //Source Parameters
                //var sParameters = db.Storage.Where(i => i.Name == destination);
                var sParameters = db.Storage.Where(i => i.Id == sourceStorageId);
                strSourceIP = sParameters.ToList()[0].IPAddress;
                strSourceUserName = sParameters.ToList()[0].UserName;
                strSourcePassword = sParameters.ToList()[0].Password;
                strSourceDatabase = sParameters.ToList()[0].DBName;
                strSourceTable = sParameters.ToList()[0].TableName;
                if (dbSourceTable != null)
                {
                    strSourceTable = dbSourceTable;
                }

                //Destination Parameters
                //var dParameters = db.Storage.Where(i => i.Name == source);
                var dParameters = db.Storage.Where(i => i.Id == destinationStorageId);
                strDestinationIP = dParameters.ToList()[0].IPAddress;
                strDestinationUserName = dParameters.ToList()[0].UserName;
                strDestinationPassword = dParameters.ToList()[0].Password;
                strDestinationDatabase = dParameters.ToList()[0].DBName;
                if (dbSourceDB != null)
                {
                    strDestinationDatabase = dbSourceDB;
                }
                strDestinationTable = dParameters.ToList()[0].TableName;


                string strTmpResult = "mapreduce.ImportJobBase: Transferred 0 bytes";
                DoCreateJobStatus(key, sParameters.ToList()[0].Name, strSourceIP, strSourceTable, dParameters.ToList()[0].Name, strDestinationIP, strDestinationTable, strJobName, db);
                result = doPostgresToHadoopDataTransfer(sourceStorageId, dbSourceDB, dbSourceTable, destinationStorageId, dbDestinationDB, dbDestinationTable, ref strTmpResult);

                Regex regex = new Regex(@"(INFO mapreduce.Job:[\s*\S*]*completed successfully)");
                bool isMatched = regex.IsMatch(result);
                if (isMatched)
                {
                    result = "Transfer Successfully";

                    jsonResult = new JsonResult()
                    {
                        Data = new { success = true, val = result },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    result = "Transfer Failed";

                    jsonResult = new JsonResult()
                    {
                        Data = new { success = false, val = result },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }

                DoUpdateJobStatus(key, result, strTmpResult);
                return jsonResult;
            }



            //Find the job name
            source = sourceParameters[0].Type;
            destination = destinationParameters[0].Type;
            var ss = db.JobTB.Where(i => i.Source == source && i.Destinatiion == destination);
            if (ss.ToList().Count > 0)
            {
                strJobName = ss.ToList()[0].JobName;
            }
            else //No job to run
            {
                ;   //do nothing
            }

            //Job Serve Parameters
            var sClusterParameters = db.JobServer;
            strClusterSourceIP = sClusterParameters.ToList()[0].IPAddress;
            strClusterUserName = sClusterParameters.ToList()[0].UserName;
            strClusterPassword = sClusterParameters.ToList()[0].Password;
            strClusterJobDirectory = sClusterParameters.ToList()[0].JobDirectory;

            if (strJobName.ToUpper().EndsWith("HDFS.JAR") || strJobName.ToUpper().EndsWith("HIVE.JAR"))
            {
                //Source Parameters
                //var sParameters = db.Storage.Where(i => i.Name == destination);
                var sParameters = db.Storage.Where(i => i.Id == destinationStorageId);
                strSourceIP = sParameters.ToList()[0].IPAddress;
                strSourceUserName = sParameters.ToList()[0].UserName;
                strSourcePassword = sParameters.ToList()[0].Password;
                strSourceDatabase = sParameters.ToList()[0].DBName;
                strSourceTable = sParameters.ToList()[0].TableName;

                //Destination Parameters
                //var dParameters = db.Storage.Where(i => i.Name == source);
                var dParameters = db.Storage.Where(i => i.Id == sourceStorageId);
                strDestinationIP = dParameters.ToList()[0].IPAddress;
                strDestinationUserName = dParameters.ToList()[0].UserName;
                strDestinationPassword = dParameters.ToList()[0].Password;
                strDestinationDatabase = dParameters.ToList()[0].DBName;
                if (dbSourceDB != null)
                {
                    strDestinationDatabase = dbSourceDB;
                }
                strDestinationTable = dParameters.ToList()[0].TableName;
                if (dbSourceTable != null)
                {
                    strDestinationTable = dbSourceTable;
                }

                //DoCreateJobStatus(key, source, strDestinationIP, strDestinationTable, destination, strSourceIP, strSourceTable, strJobName, db);
                DoCreateJobStatus(key, sourceParameters[0].Name, strDestinationIP, strDestinationTable, destinationParameters[0].Name, strSourceIP, strSourceTable, strJobName, db);
            }
            else if (strJobName.ToUpper().EndsWith("SQL.JAR"))
            {
                //Source Parameters
                //var sParameters = db.Storage.Where(i => i.Name == source);
                var sParameters = db.Storage.Where(i => i.Id == sourceStorageId);
                strSourceIP = sParameters.ToList()[0].IPAddress;
                strSourceUserName = sParameters.ToList()[0].UserName;
                strSourcePassword = sParameters.ToList()[0].Password;
                strSourceDatabase = sParameters.ToList()[0].DBName;
                strSourceTable = sParameters.ToList()[0].TableName;

                //Destination Parameters
                //var dParameters = db.Storage.Where(i => i.Name == destination);
                var dParameters = db.Storage.Where(i => i.Id == destinationStorageId);
                strDestinationIP = dParameters.ToList()[0].IPAddress;
                strDestinationUserName = dParameters.ToList()[0].UserName;
                strDestinationPassword = dParameters.ToList()[0].Password;
                strDestinationDatabase = dParameters.ToList()[0].DBName;
                if (dbDestinationDB != null)
                {
                    strDestinationDatabase = dbDestinationDB;
                }
                strDestinationTable = dParameters.ToList()[0].TableName;
                if (dbDestinationTable != null)
                {
                    strDestinationTable = dbDestinationTable;
                }

                //DoCreateJobStatus(key, source, strSourceIP, strSourceTable, destination, strDestinationIP, strDestinationTable, strJobName, db);
                DoCreateJobStatus(key, sourceParameters[0].Name, strSourceIP, strSourceTable, destinationParameters[0].Name, strDestinationIP, strDestinationTable, strJobName, db);
            }



            //Initialize sshCmdClient
            SSHCommandClient sshCmdClient = new SSHCommandClient(strSourceIP, strSourceUserName, strSourcePassword,
                                                                    strSourceDatabase, strSourceTable,
                                                                    strDestinationIP, strDestinationUserName, strDestinationPassword,
                                                                    strDestinationDatabase, strDestinationTable, strJobName);

            sshCmdClient.ClusterHost = strClusterSourceIP;
            sshCmdClient.ClusterUserName = strClusterUserName;
            sshCmdClient.ClusterPassword = strClusterPassword;
            sshCmdClient.ClusterJobDirectory = strClusterJobDirectory;


            var myTask = sshCmdClient.RunCmdInRemoteServerAsync();
            result = await myTask;
            string tmpResult = result;
                
            if (result.Contains("Connect fails with the following exception"))
            {
                string strRegex2 = "(\\S*Exception:\\s\\S*\\s\\S*)";
                Regex regex = new Regex(@strRegex2);
                if (regex.IsMatch(result))
                {
                    Match match = regex.Match(result);
                    result = match.Groups[1].ToString();
                }

                jsonResult = new JsonResult()
                {
                    Data = new { success = true, val = result },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else if (result.Contains("Return code: 0"))
            {
                result = "Transfer Successfully";

                jsonResult = new JsonResult()
                {
                    Data = new { success = true, val = result },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else if (result.Contains("Return code: 1"))
            {
                result = "Transfer Failed";

                jsonResult = new JsonResult()
                {
                    Data = new { success = false, val = result },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            //update JobStatus 
            DoUpdateJobStatus(key, result, tmpResult);

            return jsonResult;
        }

        private string doHBaseDataTransfer(int sourceStorageId, string dbSourceDB, string dbSourceTable,
                                               int destinationStorageID, string dbDestinationDB, string dbDestinationTable, ref string strTmpResult)
        {

            string result = "";

            try
            {
                string strSourceIP = "";
                string strSourcePort = "";
                string strSourceUserName = "";
                string strSourcePassword = "";
                //string strSourceDatabase = "";
                //string strSourceTable = "";

                //string strDestinationIP = "";
                //string strDestinationUserName = "";
                //string strDestinationPassword = "";
                //string strDestinationDatabase = "";
                //string strDestinationTable = "";

                string strClusterIP = "";
                string strClusterUserName = "";
                string strClusterPassword = "";

                string strCmd = "sudo -u hdfs sqoop import --connect jdbc:mysql://SOURCE_IP/SOURCE_DB --table SOURCE_TB --username USER_NAME --hbase-table HBASE_TABLE --column-family info --hbase-create-table --hbase-row-key HBASE_ROW_KEY -m 1";

                string key = Guid.NewGuid().ToString();

                ApplicationDbContext db = new ApplicationDbContext();
                //Find HBase Parameters
                var sClusterParameters = db.Storage.Where(i => i.Id == destinationStorageID);
                strClusterIP = sClusterParameters.ToList()[0].IPAddress;
                strClusterUserName = sClusterParameters.ToList()[0].UserName;
                strClusterPassword = sClusterParameters.ToList()[0].Password;

                //Find MySql Parameters
                sClusterParameters = db.Storage.Where(i => i.Id == sourceStorageId);
                strSourceIP = sClusterParameters.ToList()[0].IPAddress;
                strSourcePort = sClusterParameters.ToList()[0].Port;
                strSourceUserName = sClusterParameters.ToList()[0].UserName;
                strSourcePassword = sClusterParameters.ToList()[0].Password;

                strCmd = strCmd.Replace("SOURCE_IP", strSourceIP);
                strCmd = strCmd.Replace("SOURCE_DB", dbSourceDB);
                strCmd = strCmd.Replace("SOURCE_TB", dbSourceTable);
                strCmd = strCmd.Replace("USER_NAME", strSourceUserName);
                strCmd = strCmd.Replace("HBASE_TABLE", dbSourceTable.ToLower());

                string myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionMySQL"].ConnectionString;
                //myConnectionString = myConnectionString.Replace("142.0.252.94", strSourceIP);
                //myConnectionString = myConnectionString.Replace("BooksDB", dbSourceDB);
                myConnectionString = myConnectionString.Replace("HOST_IP", strSourceIP);
                myConnectionString = myConnectionString.Replace("PORT_NO", strSourcePort);
                myConnectionString = myConnectionString.Replace("USER_ID", strSourceUserName);
                myConnectionString = myConnectionString.Replace("PASSWORD", strSourcePassword);
                myConnectionString = myConnectionString.Replace("DATABASE", dbSourceDB);
                string strPrimaryKey = GetPrimaryKey(dbSourceTable, myConnectionString);

                if (strPrimaryKey == null)
                {
                    strCmd = strCmd.Replace("--hbase-row-key HBASE_ROW_KEY -m 1", "");
                }

                strCmd = strCmd.Replace("HBASE_ROW_KEY", strPrimaryKey);

                //result = ExpectSSH(strClusterIP, strClusterUserName, strClusterPassword, strCmd);

                ConnectionInfo connectionInfo = new PasswordConnectionInfo(strClusterIP, strClusterUserName, strClusterPassword);

                using (SshClient client = new SshClient(connectionInfo))
                {
                    if (!client.IsConnected)
                        client.Connect();

                    if (client.IsConnected)
                    {
                        //SshCommand cmd = client.CreateCommand(strCmd);
                        //cmd.Execute();
                        //error += cmd.Error;
                        //result += cmd.Result;

                        IDictionary<Renci.SshNet.Common.TerminalModes, uint> termkvp = new Dictionary<Renci.SshNet.Common.TerminalModes, uint>();
                        termkvp.Add(Renci.SshNet.Common.TerminalModes.ECHO, 53);

                        ShellStream shellStream = client.CreateShellStream("xterm", 80, 24, 800, 600, 1024, termkvp);

                        string strRegex = "(\\[USER@\\S*\\s*\\S*\\]#)";
                        strRegex = strRegex.Replace("USER", strClusterUserName);

                        //TimeSpan ts = new TimeSpan(0, 5, 0);    //Set 5 minutes for timeout
                        //result = shellStream.Expect(new Regex(@strRegex), ts); //expect user prompt 
                        result = shellStream.Expect(new Regex(@strRegex)); //expect user prompt 

                        //send command
                        shellStream.WriteLine(strCmd);
                        shellStream.Flush();
                        //result = shellStream.Expect(new Regex(@strRegex), ts); //expect user prompt
                        result = shellStream.Expect(new Regex(@strRegex)); //expect user prompt

                        //string strCmdGetByte = "hadoop fs -ls /hbase/data/default/TABLE_NAME/*/info/*";
                        string strCmdGetByte = "hadoop fs -du /hbase/data/default";
                        //strCmdGetByte = strCmdGetByte.Replace("TABLE_NAME", dbSourceTable.ToLower());
                        shellStream.WriteLine(strCmdGetByte);
                        string strTmpByte = shellStream.Expect(new Regex(@strRegex)); //expect user prompt
                        
                        //Regex regex = new Regex(@"hbase\s*hbase\s*(\S*)");
                        string strRegex2 = "(\\d*)\\s*\\d*\\s*/hbase/data/default/TABLE_NAME\\b";
                        strRegex2 = strRegex2.Replace("TABLE_NAME", dbSourceTable.ToLower());
                        //Regex regex = new Regex(@"hbase\s*hbase\s*(\S*)");
                        Regex regex = new Regex(@strRegex2);
                        if (regex.IsMatch(strTmpByte))
                        {
                            Match match = regex.Match(strTmpByte);
                            string strByte = match.Groups[1].ToString();
                            long myByte = long.Parse(strByte) * 2;
                            strTmpResult = strTmpResult.Replace("0", myByte.ToString());
                        }


                        //int idxcol = result.IndexOf("NAMESPACE");
                        //result = result.Substring(idxcol);

                        //var modelTB = Read(result).ToList();

                        //modelTB.Add(new object[] { sClusterParameters.ToList()[0].DBName });    //added by Anthony Lai on 2015-09-01 to select default database
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }

        private string doPostgresToHadoopDataTransfer(int sourceStorageId, string dbSourceDB, string dbSourceTable,
                                       int destinationStorageID, string dbDestinationDB, string dbDestinationTable, ref string strTmpResult)
        {
            string result = "";

            try
            {
                string strSourceIP = "";
                string strSourcePort = "";
                string strSourceUserName = "";
                string strSourcePassword = "";
                //string strSourceDatabase = "";
                //string strSourceTable = "";

                //string strDestinationIP = "";
                //string strDestinationUserName = "";
                //string strDestinationPassword = "";
                //string strDestinationDatabase = "";
                //string strDestinationTable = "";

                string strClusterIP = "";
                string strClusterUserName = "";
                string strClusterPassword = "";

                string key = Guid.NewGuid().ToString();
                string strCmdGetByte = "";

                ApplicationDbContext db = new ApplicationDbContext();
                //Find HBase Parameters
                var dClusterParameters = db.Storage.Where(i => i.Id == destinationStorageID);
                strClusterIP = dClusterParameters.ToList()[0].IPAddress;
                strClusterUserName = dClusterParameters.ToList()[0].UserName;
                strClusterPassword = dClusterParameters.ToList()[0].Password;

                //Find PostgresSql Parameters
                var sClusterParameters = db.Storage.Where(i => i.Id == sourceStorageId);
                strSourceIP = sClusterParameters.ToList()[0].IPAddress;
                strSourcePort = sClusterParameters.ToList()[0].Port;
                strSourceUserName = sClusterParameters.ToList()[0].UserName;
                strSourcePassword = sClusterParameters.ToList()[0].Password;

                string strCmd = "";
                string strRegex2 = "";

                if (dClusterParameters.ToList()[0].Type.ToUpper().Contains("HBASE"))
                {
                    strCmd = "sudo -u hdfs sqoop import --connect jdbc:postgresql://SOURCE_IP:PORT_NO/SOURCE_DB --table SOURCE_TB --username USER_NAME --password PASSWORD --hbase-table HBASE_TABLE --column-family info --hbase-create-table --hbase-row-key HBASE_ROW_KEY -m 1";
                    strCmd = strCmd.Replace("SOURCE_IP", strSourceIP);
                    strCmd = strCmd.Replace("PORT_NO", strSourcePort);
                    strCmd = strCmd.Replace("SOURCE_DB", dbSourceDB);
                    strCmd = strCmd.Replace("SOURCE_TB", dbSourceTable);
                    strCmd = strCmd.Replace("USER_NAME", strSourceUserName);
                    strCmd = strCmd.Replace("PASSWORD", strSourcePassword);
                    strCmd = strCmd.Replace("HBASE_TABLE", dbSourceTable.ToLower());

                    string myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionPostgreSQL"].ConnectionString;
                    myConnectionString = myConnectionString.Replace("HOST_IP", strSourceIP);
                    myConnectionString = myConnectionString.Replace("PORT_NO", strSourcePort);
                    myConnectionString = myConnectionString.Replace("USER_ID", strSourceUserName);
                    myConnectionString = myConnectionString.Replace("PASSWORD", strSourcePassword);
                    myConnectionString = myConnectionString.Replace("DATABASE", dbSourceDB);
                    string strPrimaryKey = GetPrimaryKey(dbSourceTable, myConnectionString);

                    if (strPrimaryKey == null)
                    {
                        strCmd = strCmd.Replace("--hbase-row-key HBASE_ROW_KEY -m 1", "");
                    }

                    strCmd = strCmd.Replace("HBASE_ROW_KEY", strPrimaryKey);

                    strCmdGetByte = "hadoop fs -du /hbase/data/default";
                    strRegex2 = "(\\d*)\\s*\\d*\\s*/hbase/data/default/TABLE_NAME\\b";
                }
                else if (dClusterParameters.ToList()[0].Type.ToUpper().Contains("HIVE"))
                {
                    strCmd = "sudo -u hdfs sqoop import --connect jdbc:postgresql://SOURCE_IP:PORT_NO/SOURCE_DB --table SOURCE_TB --username USER_NAME --password PASSWORD --hive-table HIVE_TABLE --create-hive-table --hive-import -m 1";
                    strCmd = strCmd.Replace("SOURCE_IP", strSourceIP);
                    strCmd = strCmd.Replace("PORT_NO", strSourcePort);
                    strCmd = strCmd.Replace("SOURCE_DB", dbSourceDB);
                    strCmd = strCmd.Replace("SOURCE_TB", dbSourceTable);
                    strCmd = strCmd.Replace("USER_NAME", strSourceUserName);
                    strCmd = strCmd.Replace("PASSWORD", strSourcePassword);
                    strCmd = strCmd.Replace("HIVE_TABLE", dbSourceTable.ToLower());

                    strCmdGetByte = "hadoop fs -du /user/hive/warehouse";
                    strRegex2 = "(\\d*)\\s*\\d*\\s*/user/hive/warehouse/TABLE_NAME\\b";
                }
                

                ConnectionInfo connectionInfo = new PasswordConnectionInfo(strClusterIP, strClusterUserName, strClusterPassword);

                using (SshClient client = new SshClient(connectionInfo))
                {
                    if (!client.IsConnected)
                        client.Connect();

                    if (client.IsConnected)
                    {
                        IDictionary<Renci.SshNet.Common.TerminalModes, uint> termkvp = new Dictionary<Renci.SshNet.Common.TerminalModes, uint>();
                        termkvp.Add(Renci.SshNet.Common.TerminalModes.ECHO, 53);

                        ShellStream shellStream = client.CreateShellStream("xterm", 80, 24, 800, 600, 1024, termkvp);

                        string strRegex = "(\\[USER@\\S*\\s*\\S*\\]#)";
                        strRegex = strRegex.Replace("USER", strClusterUserName);

                        result = shellStream.Expect(new Regex(@strRegex)); //expect user prompt 

                        //send command
                        shellStream.WriteLine(strCmd);
                        shellStream.Flush();
                        result = shellStream.Expect(new Regex(@strRegex)); //expect user prompt

                        //string strCmdGetByte = "hadoop fs -du /hbase/data/default";
                        shellStream.WriteLine(strCmdGetByte);
                        string strTmpByte = shellStream.Expect(new Regex(@strRegex)); //expect user prompt

                        //string strRegex2 = "(\\d*)\\s*\\d*\\s*/hbase/data/default/TABLE_NAME\\b";
                        strRegex2 = strRegex2.Replace("TABLE_NAME", dbSourceTable.ToLower());
                        Regex regex = new Regex(@strRegex2);
                        if (regex.IsMatch(strTmpByte))
                        {
                            Match match = regex.Match(strTmpByte);
                            string strByte = match.Groups[1].ToString();
                            long myByte = long.Parse(strByte) * 2;
                            strTmpResult = strTmpResult.Replace("0", myByte.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }

        private string ExpectSSH(string address, string login, string password, string command)
        {
            string rep = null;
            try
            {
                SshClient sshClient = new SshClient(address, 22, login, password);

                sshClient.Connect();
                IDictionary<Renci.SshNet.Common.TerminalModes, uint> termkvp = new Dictionary<Renci.SshNet.Common.TerminalModes, uint>();
                termkvp.Add(Renci.SshNet.Common.TerminalModes.ECHO, 53);

                ShellStream shellStream = sshClient.CreateShellStream("xterm", 80, 24, 800, 600, 1024, termkvp);

                //Get logged in
                //rep = shellStream.Expect(new Regex(@"[$>]")); //expect user prompt
                //rep = shellStream.Expect(new Regex(@"([$#>:])")); //expect user prompt
                rep = shellStream.Expect(new Regex(@"(\[root@\S*\s*\S*\]#)")); //expect user prompt
                //string rep = shellStream.Read(); //expect user prompt
                //this.writeOutput(results, rep);

                //send command
                shellStream.WriteLine(command);
                shellStream.Flush();
                //rep = shellStream.Expect(new Regex(@"([$#>:])")); //expect password or user prompt
                rep = shellStream.Expect(new Regex(@"(\[root@\S*\s*\S*\]#)")); //expect password or user prompt

                //this.writeOutput(results, rep);

                //check to send password
                if (rep.Contains(":"))
                {
                    //send password
                    shellStream.WriteLine(password);
                    rep = shellStream.Expect(new Regex(@"[$#>]")); //expect user or root prompt
                    //this.writeOutput(results, rep);
                }

                sshClient.Disconnect();

            }//try to open connection
            catch (Exception ex)
            {
                rep = ex.Message;
            }

            return rep;
        }

        private String GetPrimaryKey(String strTable, String myConnectionString)
        {
            try
            {
                string strColumnName = "";
                string strIndexName = "";
                String[] strRestricted = new String[4] { null, null, strTable, null }; 
                bool bIsPrimary = false;


                // Make sure that there is a connection.
                //using (var conn = new MySql.Data.MySqlClient.MySqlConnection(myConnectionString))
                using (var conn = new NpgsqlConnection(myConnectionString))
                {
                    conn.Open();

                    // DATABASE: Get the schemas needed.
                    var oSchemaIndexes = conn.GetSchema("Indexes", strRestricted);
                    var oSchemaIndexColumns = conn.GetSchema("IndexColumns", strRestricted);

                    // Get the index name for the primary key.
                    //foreach (DataRow oRow in oSchemaIndexes.Rows)
                    //{
                    //    // If we have a primary key, then we found what we want.
                    //    strIndexName = oRow["INDEX_NAME"].ToString();
                    //    bIsPrimary = (bool)oRow["PRIMARY"];
                    //    if (true == bIsPrimary)
                    //        break;
                    //}

                    // If no primary index, bail.
                    if (false == bIsPrimary)
                    {
                        int rowCount = conn.GetSchema("Columns", strRestricted).Rows.Count;
                        if (rowCount > 0)
                        {
                            strColumnName = conn.GetSchema("Columns", strRestricted).Rows[0][3].ToString();    //Get The first column for HBase ROW_ID
                        }

                        return strColumnName;
                    }

                    // Get the corresponding column name.
                    foreach (DataRow oRow in oSchemaIndexColumns.Rows)
                    {
                        // Get the column name.
                        if (strIndexName == (String)oRow["INDEX_NAME"])
                        {
                            strColumnName = (String)oRow["COLUMN_NAME"];
                            break;
                        }
                    }

                    return strColumnName;
                }
            }

            catch (Exception ex)
            {
                string str = ex.Message;
            }

            return null;
        }

        private static IEnumerable<object[]> Read(string result)
        {
            var value = result.Split('\n');
            for (int i = 0; i < value.Length - 3; i++)
            {
                if (!value[i].Equals(""))
                {
                    var values = new List<object>();
                    value[i] = value[i].Remove(value[i].Length - 1);
                    value[i] = value[i].Substring(1);
                    var val = value[i].Split(',');
                    for (int j = 0; j < val.Length; j++)
                    {
                        if (val[j].Contains("hiveContext.sql"))
                            val[j] = "DB_END";

                        object obj = (object)val[j];
                        values.Add(obj);
                    }
                    yield return values.ToArray();
                }
            }
        }

        private static IEnumerable<object[]> ReadHBaseMetaData(string result)
        {
            var value = result.Split(',');
            for (int i = 0; i < value.Length; i++)
            {
                if (!value[i].Equals(""))
                {
                    var values = new List<object>();
                    var val = value[i].Split(':');
                    object obj = (object)val[1];
                    values.Add(obj);

                    yield return values.ToArray();
                }
            }
        }
        private static IEnumerable<object[]> ReadSparkTableData(string result, int startRowIdx)
        {
            var value = result.Split('\n');
            for (int i = startRowIdx; i < value.Length - 3; i++)
            {
                if (!value[i].Equals(""))
                {
                    var values = new List<object>();
                    value[i] = value[i].Remove(value[i].Length - 1);
                    value[i] = value[i].Substring(1);
                    var val = value[i].Split(',');
                    for (int j = 0; j < val.Length; j++)
                    {
                        //if (val[j].Contains("hiveContext.sql"))
                        //    val[j] = "DB_END";

                        object obj = (object)val[j];
                        values.Add(obj);
                    }
                    yield return values.ToArray();
                }
            }
        }
        private static IEnumerable<object[]> ReadHBaseTableData(string result, int startRowIdx, string cmdType)
        {
            var value = result.Split('\n');
            for (int i = startRowIdx; i < value.Length - 3; i++)
            {
                if (!value[i].Equals(""))
                {
                    var values = new List<object>();

                    int idx = 0;
                    if (cmdType == "SCAN")
                        idx = value[i].IndexOf("column=");
                    else
                        idx = value[i].IndexOf("timestamp=");
                    var rowID = value[i].Substring(0, idx).Trim();
                    var colCell = value[i].Substring(idx).Replace("\n", "");

                    //Add ROW
                    object obj = (object)rowID;
                    values.Add(obj);

                    //Add COLUMN+CELL
                    obj = (object)colCell;
                    values.Add(obj);

                    yield return values.ToArray();
                }
            }
        }

        private void DoCreateJobStatus(string key, string source, string strSourceIP, string strSourceTable, 
                                       string destination, string strDestinationIP, string strDestinationTable, string jobName, ApplicationDbContext db)
        {
            try
            {
                //DateTime now = DateTime.Now;
                //DateTime now = DateTime.UtcNow;
                db.JobStatus.Add
                (
                    new JobStatus()
                    {
                        Key = key,
                        Source = source,
                        SourceIP = strSourceIP,
                        SourceTable = strSourceTable,
                        Destination = destination,
                        DestinationIP = strDestinationIP,
                        DestinationTable = strDestinationTable,
                        JobName = jobName,
                        //StartDateTime = DateTime.Now,
                        //StartDateTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond),
                        //StartDateTime = DateTime.UtcNow,

                        StartDateTime = DateTime.UtcNow.ToLocalTime(),

                        //EndDateTime = DateTime.Now,
                        Status = "Running"
                    }
                );

                db.SaveChanges();
            }
            catch(Exception ex)
            {
                string str = ex.Message;
            }
        }

        private void DoUpdateJobStatus(string key, string status, string tmpResult)
        {
            //JobStatus js = db.JobStatus.Find(id);
            //js.EndDateTime = DateTime.Now;
            //js.Status = "Finished Successfully";
            //db.JobStatus.Add(js);
            //db.SaveChanges();

            try
            {
                JobStatus jobStatus;
                //1. Get JobStatus from DB
                using (var ctx = new ApplicationDbContext())
                {
                    jobStatus = ctx.JobStatus.Where(s => s.Key == key).FirstOrDefault<JobStatus>();
                }

                //2. change JobStatus status in disconnected mode (out of ctx scope)
                if (jobStatus != null)
                {
                    string strByteTransfter = "";
                    //Regex r = new Regex(@"mapreduce.Import|ExportJobBase: Transferred\s(\S*\sbytes|\S*\sKB)");
                    string pattern = @"mapreduce.[ImportJobBase|ExportJobBase]*:\sTransferred\s(\S*\s[PB|TB|GB|MB|KB|bytes]*)";
                    //strByteTransfter = r.Match(@tmpResult).Groups[1].Value;
                    MatchCollection matches = Regex.Matches(tmpResult, pattern);
                    double myNum = 0.0;

                    foreach (Match match in matches)
                    {
                        GroupCollection groups = match.Groups;
                        string strVal = groups[1].Value;
                        if (strVal.Contains("bytes"))
                        {
                            myNum += double.Parse(strVal.Replace("bytes", "").Trim());
                        }
                        else if (strVal.Contains("KB"))
                        {
                            myNum += double.Parse(strVal.Replace("KB", "").Trim()) * 1024;
                        }
                        else if (strVal.Contains("MB"))
                        {
                            myNum += double.Parse(strVal.Replace("MB", "").Trim()) * 1024 * 1024;
                        }
                        else if (strVal.Contains("GB"))
                        {
                            myNum += double.Parse(strVal.Replace("GB", "").Trim()) * 1024 * 1024 * 1024;
                        }
                        else if (strVal.Contains("TB"))
                        {
                            myNum += double.Parse(strVal.Replace("TB", "").Trim()) * 1024 * 1024 * 1024 * 1024;
                        }
                        else if (strVal.Contains("PB"))
                        {
                            myNum += double.Parse(strVal.Replace("PB", "").Trim()) * 1024 * 1024 * 1024 * 1024 * 1024;
                        }
                        else
                        {
                            ;   //do nothing
                        }
                    }
                    //long mylong = (long) myNum / 2;
                    double num = myNum / 2; //divided by 2 because each item count to two times.
                    if (num >= ((long)1024 * 1024 * 1024 * 1024))
                        strByteTransfter = String.Format("{0:0.0000} TB", num / ((long)1024 * 1024 * 1024 * 1024));
                    else if (num >= (1024 * 1024 * 1024))
                        strByteTransfter = String.Format("{0:0.0000} GB", num / (1024 * 1024 * 1024));
                    else if (num >= (1024 * 1024))
                        strByteTransfter = String.Format("{0:0.0000} MB", num / (1024 * 1024));
                    else if (num >= 1024)
                        strByteTransfter = String.Format("{0:0.0000} KB", num / 1024);
                    else
                        strByteTransfter = String.Format("{0} bytes", num);

                    jobStatus.ByteTransfer = strByteTransfter;

                    jobStatus.Status = status;
                    //jobStatus.EndDateTime = DateTime.Now;
                    //jobStatus.EndDateTime = DateTime.UtcNow;
                    jobStatus.EndDateTime = DateTime.UtcNow.ToLocalTime();
                    TimeSpan? ts = jobStatus.EndDateTime - jobStatus.StartDateTime;
                    string str = "";
                    if(ts.Value.Days>0)
                    {
                        str = ts.Value.ToString(@"d\.hh\:mm\:ss") + "days";
                    }
                    else if(ts.Value.Hours > 0)
                    {
                        str = ts.Value.ToString(@"hh\:mm\:ss") + "hrs";
                    }
                    else if (ts.Value.Minutes > 0)
                    {
                        str = ts.Value.ToString(@"mm\:ss") + "mins";
                    }
                    else if (ts.Value.Seconds > 0)
                    {
                        str = ts.Value.ToString(@"mm\:ss") + "sec";
                    }
                    jobStatus.TimeTaken = str;
                }

                //save modified entity using new Context
                using (var dbCtx = new ApplicationDbContext())
                {
                    //3. Mark entity as modified
                    dbCtx.Entry(jobStatus).State = System.Data.Entity.EntityState.Modified;

                    //4. call SaveChanges
                    dbCtx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string str = ex.Message;
            }
        }
    }
}