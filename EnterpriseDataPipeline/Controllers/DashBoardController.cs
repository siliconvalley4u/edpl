using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EnterpriseDataPipeline.Models;
//using EnterpriseDataPipeline.Repo;

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

using SSHWrapper;

using MySql.Data;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Text.RegularExpressions;

using System.Data;
using System.Data.Odbc;
using System.Data.Common;
using System.Dynamic;

using Renci.SshNet;
using System.Net.Http;
using System.IO;

using EnterpriseDataPipeline.ViewModel;
using Npgsql;


namespace EnterpriseDataPipeline.Controllers
{
    //[Authorize(Roles = "Admin")]
    [Authorize(Roles = "Operator, Admin")]
    //[HandleError]
    public class DashBoardController : Controller
    //public class AnalysisController : DynamicController
    {
        static ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            try
            {
                //var result = new List<dynamic>();

                //var obj = (IDictionary<string, object>)new ExpandoObject();
                //obj.Add("Req_Name", "Ibrahim");
                //obj.Add("Req_Date", "Today");
                //result.Add(obj);
                //ViewBag.Result = result;

                //ApplicationDbContext db = new ApplicationDbContext();
                //return View(db.Storage.OrderBy(s => s.Name).ToList());

                ApplicationDbContext db = new ApplicationDbContext();
                List<Storage> listStorage = db.Storage.OrderBy(s => s.Name).ToList();
                var selectList = new SelectList(listStorage, "Id", "Name");
                var vm = new StorageViewModel
                {
                    Storage = selectList,
                    SourceId = 1,           //Default Source
                    DestinationId = 7       //Default Destination
                };
                return View(vm);
            }
            catch (Exception ex)
            {
                string str = ex.Message;
                return View("Error");
            }

            //return View();

            //ApplicationDbContext db = new ApplicationDbContext();
            //return View(db.Storage.OrderBy(s => s.Name).ToList());
        }

        public ActionResult ViewPage(string page)
        {
            ViewData["page"] = page;
            return View();
        }

        #region "Get DB"
        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        public ActionResult GetDB(string storage)        
        {
            JsonResult jsonResult = new JsonResult();
            string myConnectionString = "";

            try
            {

                ApplicationDbContext db = new ApplicationDbContext();
                int storageId = int.Parse(storage);
                var tbStorage = db.Storage.Where(s => s.Id == storageId).ToList();
                string strStorageType = tbStorage[0].Type;

                //switch (storage)
                switch (strStorageType)
                {
                    case "MySQL":
                        myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionMySQL"].ConnectionString;
                        //var sClusterParameters = db.Storage.Where(i => i.Name == storage);
                        //ViewBag.SourceDB = sClusterParameters.ToList()[0].DBName;
                        myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                        myConnectionString = myConnectionString.Replace("DATABASE", tbStorage.ToList()[0].DBName);
                        myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                        myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                        myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);
                        jsonResult = GetMySQLDB(myConnectionString, tbStorage[0].Id);
                        break;
                    case "HBase":
                        //myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionHBase"].ConnectionString;
                        //jsonResult = GetHBaseDB(storage);
                        myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionHBase"].ConnectionString;
                        myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                        myConnectionString = myConnectionString.Replace("DATABASE", tbStorage.ToList()[0].DBName);
                        myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                        myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                        myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);
                        jsonResult = GetHBaseDBV2(myConnectionString, storage);
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
                    case "Postgres":
                        myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionPostgreSQL"].ConnectionString;
                        myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                        myConnectionString = myConnectionString.Replace("DATABASE", tbStorage.ToList()[0].DBName);
                        myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                        myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                        myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);
                        jsonResult = GetPostgresDB(myConnectionString, tbStorage[0].Id);
                        break;
                    case "Spark":
                        myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionSpark"].ConnectionString;
                        //jsonResult = GetSparkDB(myConnectionString);
                        //var myTask = GetSparkDB(myConnectionString, storage, jsonResult);

                        jsonResult = GetSparkDBV2(myConnectionString, tbStorage[0].Id);

                        //var myTask = GetSparkDBV3(myConnectionString, storage, jsonResult);

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
                using (var conn = new MySql.Data.MySqlClient.MySqlConnection(myConnectionString))
                {
                    conn.Open();

                    string query = "show databases";

                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn))
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
        private JsonResult GetHiveDB(string myConnectionString, int storageId)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            try
            {
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
        private JsonResult GetHiveDBForImpala(string myConnectionString, int storageId)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            try
            {
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

            //ApplicationDbContext db = new ApplicationDbContext();
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
        private JsonResult GetHBaseDBV2(string myConnectionString, string storage)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            try
            {
                using (OdbcConnection conn = new OdbcConnection(myConnectionString))
                {
                    conn.Open();

                    //string query = "\"show databases\"";
                    //string query = "show schemas;";
                    //string query = "select * from \"User Tables\"";
                    //string query = "\"SHOW DATABASES\";";

                    //string query = "select * from \"openquery(HBase,'SHOW DATABASES')\"";

                    string query = "select * from \"SIMBA_METATABLE\"";


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
                using (var conn = new NpgsqlConnection(myConnectionString))
                {
                    conn.Open();

                    string query = "SELECT datname FROM pg_database WHERE datistemplate = false;";

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
        private JsonResult GetSparkDB(string myConnectionString)       
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            //try
            //{
            //    using (OdbcConnection conn = new OdbcConnection(myConnectionString))
            //    {
            //        conn.Open();

            //        string query = "show databases";

            //        using (var cmd = new OdbcCommand(query, conn))
            //        {
            //            using (var reader = cmd.ExecuteReader())
            //            {
            //                var modelDB = Read(reader).ToList();
            //                jsonResult = new JsonResult()
            //                {
            //                    Data = new { success = true, val = modelDB },
            //                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
            //                };
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    string strError = ex.Message;
            //    jsonResult = new JsonResult()
            //    {
            //        Data = new { success = false, val = strError },
            //        JsonRequestBehavior = JsonRequestBehavior.AllowGet
            //    };
            //}

            IEnumerable<object[]> modelTB = null;

            //var model = Read(reader).ToList();
            //model.Add(new object[] {"all"});    //added by Anthony Lai on 2015-08-10 to allow move all tables
            //jsonResult = new JsonResult()
            //{
            //    Data = new { success = true, val = model },
            //    JsonRequestBehavior = JsonRequestBehavior.AllowGet
            //};

            //Convert to list.
            List<object[]> list = new List<object[]>();//modelTB.ToList();

            //Add new item to list.
            list.Add(new object[] { "default" });

            //Cast list to IEnumerable
            modelTB = (IEnumerable<object[]>)list;

            jsonResult = new JsonResult()
            {
                Data = new { success = true, val = modelTB },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            return jsonResult;
        }
        private JsonResult GetSparkDBV2(string myConnectionString, int storageId)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            //ApplicationDbContext db = new ApplicationDbContext();
            string strClusterIP = "";
            string strClusterUserName = "";
            string strClusterPassword = "";
            string result = "";
            string error = "";
            //string query = "show tables";

            //string strCmd = "\\cp tmpHiveDB_TB.ini tmpHiveDB.ini; echo \"hiveContext.sql(\\\"" + query + "\\\").collect.foreach(println)\" >> tmpHiveDB.ini; cat tmpHiveDB.ini | spark-shell | sed '1,/collect/d'";
            //string strCmd = "cat /root/data/tmpTB.ini | spark-shell | sed '1,/collect/d'";
            string strCmd = "cat /root/tmpDB.ini | spark-shell | sed '1,/collect/d'";

            //Find Spark Parameters
            var sClusterParameters = db.Storage.Where(i => i.Id == storageId);
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

                        result = result.Replace(",false", "");

                        var modelTB = Read(result).ToList();

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
        private async Task<ActionResult> GetSparkDBV3(string myConnectionString, string storage, JsonResult jsonResult)
        {
            //ApplicationDbContext db = new ApplicationDbContext();
            string strClusterIP = "";
            string strClusterUserName = "";
            string strClusterPassword = "";
            string result = "";
            string error = "";
            string query = "show tables";

            string strCmd = "\\cp tmpHiveDB_TB.ini tmpHiveDB.ini; echo \"hiveContext.sql(\\\"" + query + "\\\").collect.foreach(println)\" >> tmpHiveDB.ini; cat tmpHiveDB.ini | spark-shell | sed '1,/collect/d'";

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
                        //cmd.Execute();
                        var myTask = Task.Factory.StartNew(() => cmd.Execute());
                        result = await myTask;

                        error += cmd.Error;
                        result += cmd.Result;

                        //result = result.Replace(",false", "");

                        var modelTB = Read(result).ToList();

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

        //private JsonResult GetSparkDB(string myConnectionString, string storage)
        private async Task<ActionResult> GetSparkDB(string myConnectionString, string storage, JsonResult jsonResult)
        {
            //JsonResult jsonResult = new JsonResult()
            //{
            //    Data = new { success = false, val = "" },
            //    JsonRequestBehavior = JsonRequestBehavior.AllowGet
            //};

            //try
            //{
            //    using (OdbcConnection conn = new OdbcConnection(myConnectionString))
            //    {
            //        conn.Open();

            //        string query = "show databases";

            //        using (var cmd = new OdbcCommand(query, conn))
            //        {
            //            using (var reader = cmd.ExecuteReader())
            //            {
            //                var modelDB = Read(reader).ToList();
            //                jsonResult = new JsonResult()
            //                {
            //                    Data = new { success = true, val = modelDB },
            //                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
            //                };
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    string strError = ex.Message;
            //    jsonResult = new JsonResult()
            //    {
            //        Data = new { success = false, val = strError },
            //        JsonRequestBehavior = JsonRequestBehavior.AllowGet
            //    };
            //}


            //ApplicationDbContext db = new ApplicationDbContext();
            //Find the job name
            //var ss = db.JobTB.Where(i => i.Source == source && i.Destinatiion == destination);
            //if (ss.ToList().Count > 0)
            //{
            //    strJobName = ss.ToList()[0].JobName;
            //}
            //else //No job to run
            //{
            //    ;   //do nothing
            //}
            string strClusterIP = "";
            string strClusterUserName = "";
            string strClusterPassword = "";
            string result = "";
            string query = "show database";

            //Find Spark Parameters
            var ss = db.Storage.Where(i => i.Name == storage);

            var sClusterParameters = db.JobServer;
            strClusterIP = sClusterParameters.ToList()[0].IPAddress;
            strClusterUserName = sClusterParameters.ToList()[0].UserName;
            strClusterPassword = sClusterParameters.ToList()[0].Password;


            //Initialize sshCmdClient
            SSHCommandClient sshCmdClient = new SSHCommandClient();

            sshCmdClient.ClusterHost = strClusterIP;
            sshCmdClient.ClusterUserName = strClusterUserName;
            sshCmdClient.ClusterPassword = strClusterPassword;

            var myTask = sshCmdClient.RunCmdInRemoteServerAsyncSpark(query);
            result = await myTask;
            string tmpResult = result;

            return jsonResult;
        }
        #endregion "Get DB"

        #region "Get DBTable"
        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        public ActionResult GetDBTable(string storage, string database)        
        {
            JsonResult jsonResult = new JsonResult();
            string myConnectionString = "";

            try
            {
                ApplicationDbContext db = new ApplicationDbContext();
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
                        jsonResult = GetMySQLDBTable(myConnectionString, storage);
                        break;
                    case "HBase":
                        //myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionHive"].ConnectionString;
                        //myConnectionString = myConnectionString.Replace("database=default", "database=" + database);
                        jsonResult = GetHBaseDBTable(storageId, database);
                        break;
                    case "Hive":
                        myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionHive"].ConnectionString;
                        //myConnectionString = myConnectionString.Replace("database=default", "database=" + database);
                        myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                        myConnectionString = myConnectionString.Replace("DATABASE", database);
                        myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                        myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                        myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);
                        jsonResult = GetHiveDBTable(myConnectionString);
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
                    case "Spark":
                        myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionSpark"].ConnectionString;
                        myConnectionString = myConnectionString.Replace("database=default", "database=" + database);
                        //jsonResult = GetSparkDBTable(myConnectionString);

                        //jsonResult = GetSparkDBTableV2(myConnectionString, storage);

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
        private JsonResult GetMySQLDBTable(string myConnectionString, string storage)
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

                    string query = "show tables";

                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            var modelTB = Read(reader).ToList();
                            if (modelTB.Count > 0)
                            {
                                //modelTB.Add(new object[] { "all" });    //added by Anthony Lai on 2015-08-10 to allow move all tables
                                //var sClusterParameters = db.Storage.Where(i => i.Name == storage);
                                //modelTB.Add(new object[] { sClusterParameters.ToList()[0].TableName });    //added by Anthony Lai on 2015-09-01 to select default table

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
        private JsonResult GetHBaseDBTableOld(int storageId, string database)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            //ApplicationDbContext db = new ApplicationDbContext();
            string strClusterIP = "";
            string strClusterUserName = "";
            string strClusterPassword = "";
            string result = "";
            string error = "";
            string strCmd = "\\cp tmpHBaseDBTB.ini tmpHBaseDB_TB.ini; echo \"list_namespace_tables '" + database + "'\" >> tmpHBaseDB_TB.ini; cat tmpHBaseDB_TB.ini | hbase shell";

            //string strCmd = "cat /root/tmpHBaseDB.ini | hbase shell";

            //Find Spark Parameters
            var sClusterParameters = db.Storage.Where(i => i.Id == storageId);
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

                        int idxcol = result.IndexOf("TABLE");
                        result = result.Substring(idxcol);

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
        private JsonResult GetHiveDBTable(string myConnectionString)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            try
            {
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
        private JsonResult GetHiveForImpalaDBTable(string myConnectionString)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            try
            {
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
                using (var conn = new NpgsqlConnection(myConnectionString))
                {
                    conn.Open();

                    string query = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';";

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
        private JsonResult GetSparkDBTable(string myConnectionString)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            //try
            //{
            //    using (OdbcConnection conn = new OdbcConnection(myConnectionString))
            //    {
            //        conn.Open();

            //        string query = "show tables";

            //        using (var cmd = new OdbcCommand(query, conn))
            //        {
            //            using (var reader = cmd.ExecuteReader())
            //            {
            //                var modelTB = Read(reader).ToList();
            //                if (modelTB.Count > 0)
            //                {
            //                    //modelTB.Add(new object[] { "all" });    //added by Anthony Lai on 2015-08-10 to allow move all tables
            //                    jsonResult = new JsonResult()
            //                    {
            //                        Data = new { success = true, val = modelTB },
            //                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
            //                    };
            //                }
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    string strError = ex.Message;
            //    jsonResult = new JsonResult()
            //    {
            //        Data = new { success = false, val = strError },
            //        JsonRequestBehavior = JsonRequestBehavior.AllowGet
            //    };
            //}


            IEnumerable<object[]> modelTB = null;

            //Convert to list.
            List<object[]> list = new List<object[]>();//modelTB.ToList();

            //Add new item to list.
            list.Add(new object[] { "books1" });
            list.Add(new object[] { "yahoo_orc_table" });
            list.Add(new object[] { "yahoo_table" });

            //list.Add(new object[] { "orders" });
            //list.Add(new object[] { "orders_by_pub" });
            //list.Add(new object[] { "orders_by_pub_stag" });

            //Cast list to IEnumerable
            modelTB = (IEnumerable<object[]>)list;

            jsonResult = new JsonResult()
            {
                Data = new { success = true, val = modelTB },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            return jsonResult;
        }
        private JsonResult GetSparkDBTableV2(string myConnectionString, string storage)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            //ApplicationDbContext db = new ApplicationDbContext();
            string strClusterIP = "";
            string strClusterUserName = "";
            string strClusterPassword = "";
            string result = "";
            string error = "";
            string query = "show tables";

            string strCmd = "\\cp tmpHiveDB_TB.ini tmpHiveTB.ini; echo \"hiveContext.sql(\\\"" + query + "\\\").collect.foreach(println)\" >> tmpHiveTB.ini; cat tmpHiveTB.ini | spark-shell | sed '1,/collect/d'";

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

                        result = result.Replace(",false", "");

                        var modelTB = Read(result).ToList();

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
        #endregion "Get DBTable"

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

        #region "Get Query Result"
        public ActionResult RunSourceQuery(string source, string database, string table, string query, string hbaseSQL)        
        {
            JsonResult jsonResult = new JsonResult();
            string myConnectionString;

            //ApplicationDbContext db = new ApplicationDbContext();
            //var sClusterParameters = db.Storage.Where(i => i.Name == source);

            try
            {
                ApplicationDbContext db = new ApplicationDbContext();
                int storageId = int.Parse(source);
                var tbStorage = db.Storage.Where(s => s.Id == storageId).ToList();
                string strStorageType = tbStorage[0].Type;

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
                        jsonResult = GetMySQLQueryResult(myConnectionString, query);
                        break;
                    case "HBase":
                        //myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionHive"].ConnectionString;
                        //myConnectionString = myConnectionString.Replace("database=default", "database=" + database);
                        if (!(hbaseSQL.ToUpper().Equals("TRUE")))
                        {
                            jsonResult = GetHBaseQueryResult(tbStorage[0].Id, table, query);
                        }
                        else
                        {
                            myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionHBase"].ConnectionString;
                            myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                            myConnectionString = myConnectionString.Replace("DATABASE", database);
                            myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                            myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                            myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);
                            jsonResult = GetHBaseQueryResultV2(myConnectionString, query);
                        }
                        break;
                    case "Hive":
                        myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionHive"].ConnectionString;
                        //myConnectionString = myConnectionString.Replace("database=default", "database=" + database);
                        myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                        myConnectionString = myConnectionString.Replace("DATABASE", database);
                        myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                        myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                        myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);
                        jsonResult = GetHiveQueryResult(myConnectionString, query);
                        break;
                    case "Postgres":
                        myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionPostgreSQL"].ConnectionString;
                        myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                        myConnectionString = myConnectionString.Replace("DATABASE", database);
                        myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                        myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                        myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);
                        jsonResult = GetPostgresQueryResult(myConnectionString, query, source);
                        break;
                    case "Spark":
                        myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionSpark"].ConnectionString;
                        //myConnectionString = myConnectionString.Replace("database=default", "database=" + database);
                        //jsonResult = GeSparkQueryResult(myConnectionString, query);

                        //myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionHive"].ConnectionString;
                        //myConnectionString = myConnectionString.Replace("database=default", "database=" + database);
                        //jsonResult = GeHiveQueryResult(myConnectionString, query);

                        jsonResult = GetSparkQueryResultV2(myConnectionString, source, table, query);
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
        private JsonResult GetMySQLQueryResult(string myConnectionString, string query)
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

                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            var modelTB = Read(reader).ToList();
                            if (modelTB.Count > 0)
                            {
                                //var result = new List<dynamic>();

                                //var obj = (IDictionary<string, object>)new ExpandoObject();
                                //obj.Add("Req_Name", "Ibrahim");
                                //obj.Add("Req_Date", "Today");
                                //obj.Add("ABC", "CBA");
                                //obj.Add("ABbC", "CBA");
                                //result.Add(obj);
                                //ViewBag.Result = result;
                                //return View();

                                List<string> columns = new List<string>();
                                DataTable dt = reader.GetSchemaTable();
                                foreach (DataRow row in dt.Rows)
                                {
                                    columns.Add(row.Field<String>("ColumnName"));
                                }

                                var result = columns.Select(x => x as object).ToArray();

                                modelTB.Add(result);

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
        private JsonResult GetHBaseQueryResult(int storageId, string table, string query)
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
            query = query.Replace("\n", " ");

            string strCmd = "\\cp tmpHBaseDBTB.ini tmpHBaseQuery.ini; echo \"" + query + "\" >> tmpHBaseQuery.ini; cat tmpHBaseQuery.ini | hbase shell";

            string cmdType = "SCAN";
            if (query.ToUpper().StartsWith("GET"))
                cmdType = "GET";

            //Find HBase Parameters
            var sClusterParameters = db.Storage.Where(i => i.Id == storageId);
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

                        //int idxcol = result.IndexOf("COLUMN+CELL");
                        //var resultColName = result.Substring(0, idxcol + "COLUMN+CELL".Length);
                        int idxcol = result.IndexOf("CELL");
                        var resultColName = result.Substring(0, idxcol + "CELL".Length);

                        List<string> columns = new List<string>();
                        var value = resultColName.Split('\n');

                        //get the set of column name
                        for (int k = 5; k < value.Length; k++)
                        {
                            var val = Regex.Split(value[k], @"\s+");
                            for (int j = 0; j < val.Length; j++)
                            {
                                string str = val[j];
                                columns.Add(str);
                            }
                        }
                        var result2 = columns.Select(x => x as object).ToArray();

                        //result = result.Substring(idxcol + "COLUMN+CELL".Length);
                        result = result.Substring(idxcol + "CELL".Length);

                        var modelTB = ReadHBaseTableData(result, 1, cmdType).ToList();

                        modelTB.Add(result2);

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
        private JsonResult GetHBaseQueryResultV2(string myConnectionString, string query)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            try
            {
                using (OdbcConnection conn = new OdbcConnection(myConnectionString))
                {
                    conn.Open();

                    //Added by Anthony Lai on 2015-08-31 to handle the DROP or DELETE command
                    if (query.ToUpper().StartsWith("DROP") || query.ToUpper().StartsWith("DELETE"))
                    {
                        using (var cmd = new OdbcCommand(query, conn))
                        {
                            int reader = cmd.ExecuteNonQuery();

                            List<string> columns = new List<string>();
                            columns.Add(query + " completed successfully");
                            var result2 = columns.Select(x => x as object).ToArray();

                            var values = new List<object>().ToList();
                            values.Add(result2);

                            jsonResult = new JsonResult()
                            {
                                Data = new { success = true, val = values },
                                JsonRequestBehavior = JsonRequestBehavior.AllowGet
                            };
                        }

                        return jsonResult;
                    }

                    using (var cmd = new OdbcCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            var modelTB = Read(reader).ToList();
                            if (modelTB.Count > 0)
                            {
                                List<string> columns = new List<string>();
                                DataTable dt = reader.GetSchemaTable();
                                foreach (DataRow row in dt.Rows)
                                {
                                    columns.Add(row.Field<String>("ColumnName"));
                                }

                                var result = columns.Select(x => x as object).ToArray();

                                modelTB.Add(result);

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
        private JsonResult GetHiveQueryResult(string myConnectionString, string query)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            try
            {
                using (OdbcConnection conn = new OdbcConnection(myConnectionString))
                {
                    conn.Open();

                    //Added by Anthony Lai on 2015-08-31 to handle the DROP or DELETE command
                    if (query.ToUpper().StartsWith("DROP") || query.ToUpper().StartsWith("DELETE"))
                    {
                        using (var cmd = new OdbcCommand(query, conn))
                        {
                            int reader = cmd.ExecuteNonQuery();

                            List<string> columns = new List<string>();
                            columns.Add(query + " completed successfully");
                            var result2 = columns.Select(x => x as object).ToArray();

                            var values = new List<object>().ToList();
                            values.Add(result2);

                            jsonResult = new JsonResult()
                            {
                                Data = new { success = true, val = values },
                                JsonRequestBehavior = JsonRequestBehavior.AllowGet
                            };
                        }

                        return jsonResult;
                    }

                    using (var cmd = new OdbcCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            var modelTB = Read(reader).ToList();
                            if (modelTB.Count > 0)
                            {
                                List<string> columns = new List<string>();
                                DataTable dt = reader.GetSchemaTable();
                                foreach (DataRow row in dt.Rows)
                                {
                                    columns.Add(row.Field<String>("ColumnName"));
                                }

                                var result = columns.Select(x => x as object).ToArray();

                                modelTB.Add(result);

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
        private JsonResult GetPostgresQueryResult(string myConnectionString, string query, string storage)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            try
            {
                using (var conn = new NpgsqlConnection(myConnectionString))
                {
                    conn.Open();

                    //Added by Anthony Lai on 2015-08-31 to handle the DROP or DELETE command
                    if (query.ToUpper().StartsWith("DROP") || query.ToUpper().StartsWith("DELETE"))
                    {
                        using (var cmd = new NpgsqlCommand(query, conn))
                        {
                            int reader = cmd.ExecuteNonQuery();

                            List<string> columns = new List<string>();
                            columns.Add(query + " completed successfully");
                            var result2 = columns.Select(x => x as object).ToArray();

                            var values = new List<object>().ToList();
                            values.Add(result2);

                            jsonResult = new JsonResult()
                            {
                                Data = new { success = true, val = values },
                                JsonRequestBehavior = JsonRequestBehavior.AllowGet
                            };
                        }

                        return jsonResult;
                    }

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            var modelTB = Read(reader).ToList();
                            if (modelTB.Count > 0)
                            {
                                List<string> columns = new List<string>();
                                DataTable dt = reader.GetSchemaTable();
                                foreach (DataRow row in dt.Rows)
                                {
                                    columns.Add(row.Field<String>("ColumnName"));
                                }

                                var result = columns.Select(x => x as object).ToArray();

                                modelTB.Add(result);

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
        private JsonResult GetSparkQueryResult(string myConnectionString, string query)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            try
            {
                using (OdbcConnection conn = new OdbcConnection(myConnectionString))
                {
                    conn.Open();

                    using (var cmd = new OdbcCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            var modelTB = Read(reader).ToList();
                            if (modelTB.Count > 0)
                            {
                                List<string> columns = new List<string>();
                                DataTable dt = reader.GetSchemaTable();
                                foreach (DataRow row in dt.Rows)
                                {
                                    columns.Add(row.Field<String>("ColumnName"));
                                }

                                var result = columns.Select(x => x as object).ToArray();

                                modelTB.Add(result);

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
        private JsonResult GetSparkQueryResultV2(string myConnectionString, string storage, string table, string query)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            //try
            //{
            //    using (OdbcConnection conn = new OdbcConnection(myConnectionString))
            //    {
            //        conn.Open();

            //        using (var cmd = new OdbcCommand(query, conn))
            //        {
            //            using (var reader = cmd.ExecuteReader())
            //            {
            //                var modelTB = Read(reader).ToList();
            //                if (modelTB.Count > 0)
            //                {
            //                    List<string> columns = new List<string>();
            //                    DataTable dt = reader.GetSchemaTable();
            //                    foreach (DataRow row in dt.Rows)
            //                    {
            //                        columns.Add(row.Field<String>("ColumnName"));
            //                    }

            //                    var result = columns.Select(x => x as object).ToArray();

            //                    modelTB.Add(result);

            //                    jsonResult = new JsonResult()
            //                    {
            //                        Data = new { success = true, val = modelTB },
            //                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
            //                    };
            //                }
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    string strError = ex.Message;
            //    jsonResult = new JsonResult()
            //    {
            //        Data = new { success = false, val = strError },
            //        JsonRequestBehavior = JsonRequestBehavior.AllowGet
            //    };
            //}

            //ApplicationDbContext db = new ApplicationDbContext();
            string strClusterIP = "";
            string strClusterUserName = "";
            string strClusterPassword = "";
            string result = "";
            string error = "";
            //string strCmd = "java -jar /root/proj/ -e ";
            //string strCmd = "cat books.ini | spark-shell";
            //string strCmd = "cat books.ini | spark-shell | sed '1,/collect/d'";
            query = query.Replace("\n", " ");
            table = "describe " + table;

            string strCmd = "\\cp ant.ini tmp.ini; echo \"hiveContext.sql(\\\"" + table + "\\\").collect.foreach(println)\" >> tmp.ini ; echo \"hiveContext.sql(\\\"" + query + "\\\").collect.foreach(println)\" >> tmp.ini; cat tmp.ini | spark-shell | sed '1,/collect/d'";
            //string strCmd = "\\cp ant.ini tmp.ini; echo \"hiveContext.sql(\\\"" + query + "\\\").collect.foreach(println)\" >> tmp.ini; cat tmp.ini | spark-shell | sed '1,/collect/d'";

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

                        //var modelTB = Read(result).ToList();

                        int idxcol = result.IndexOf("scala> hiveContext");
                        var resultColName = result.Substring(0, idxcol);

                        List<string> columns = new List<string>();
                        var value = resultColName.Split('\n');

                        //get the set of column name
                        for (int k = 0; k < value.Length - 2; k++)
                        {
                            value[k] = value[k].Remove(value[k].Length - 1);
                            value[k] = value[k].Substring(1);
                            var val = value[k].Split(',');
                            //for (int j = 0; j < val.Length - 2; j++)
                            //{
                                string str = val[0].ToString();
                                columns.Add(str);
                            //}
                        }
                        var result2 = columns.Select(x => x as object).ToArray();



                        var modelTB = ReadSparkTableData(result, value.Length).ToList();

                        modelTB.Add(result2);

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
                    //Data = new { success = false, val = err[0] + strError },
                    Data = new { success = false, val = err[0] },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            //if (!error.Equals(""))
            //{
            //    jsonResult = new JsonResult()
            //    {
            //        Data = new { success = false, val = error },
            //        JsonRequestBehavior = JsonRequestBehavior.AllowGet
            //    };
            //}
            //else
            //{
            //    jsonResult = new JsonResult()
            //    {
            //        Data = new { success = false, val = result },
            //        JsonRequestBehavior = JsonRequestBehavior.AllowGet
            //    };
            //}



            return jsonResult;
        }
        private static IEnumerable<object[]> Read(string result)
        {
            var value = result.Split('\n');
            for (int i = 0; i < value.Length-3; i++)
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
            //var value = result.Split('\n');
            var value = result.Split(',');
            //for (int i = 1; i < value.Length - 3; i++)
            for (int i = 0; i < value.Length; i++)
            {
                if (!value[i].Equals(""))
                {
                    var values = new List<object>();
                    //value[i] = value[i].Remove(value[i].Length - 1);
                    //value[i] = value[i].Substring(1);
                    //var val = value[i].Split(',');
                    //for (int j = 0; j < val.Length; j++)
                    //{
                    //    if (val[j].Contains("hiveContext.sql"))
                    //        val[j] = "DB_END";

                    var val = value[i].Split(':');
                    //object obj = (object)value[i];
                    object obj = (object)val[1];
                    values.Add(obj);
                    //}
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

        #endregion "Get Query Result"
    }
}