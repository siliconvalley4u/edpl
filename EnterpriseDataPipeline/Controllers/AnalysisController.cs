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
    public class AnalysisController : Controller
    //public class AnalysisController : DynamicController
    {
        //static ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            StorageViewModel viewModelStorage = new StorageViewModel();

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

        #region "Get DB"
        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        public ActionResult GetDB(string storage)        
        {
            JsonResult jsonResult = new JsonResult();
            string myConnectionString;

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
                    case "Impala":
                        myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionImpala"].ConnectionString;
                        myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                        myConnectionString = myConnectionString.Replace("DATABASE", tbStorage.ToList()[0].DBName);
                        myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                        myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                        myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);
                        jsonResult = GetImpalaDB(myConnectionString, tbStorage[0].Id);
                        break;
                    case "Spark":
                        myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionSpark"].ConnectionString;
                        //jsonResult = GetSparkDB(myConnectionString);
                        //var myTask = GetSparkDB(myConnectionString, storage, jsonResult);
                        myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                        myConnectionString = myConnectionString.Replace("DATABASE", tbStorage.ToList()[0].DBName);
                        myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                        myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                        myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);

                        jsonResult = GetSparkDBV2(myConnectionString, tbStorage[0].Id);
                        //var myTask = GetSparkDBV3(myConnectionString, storage, jsonResult);
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
        private JsonResult GetImpalaDB(string myConnectionString, int storageId)
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
        private JsonResult GetHBaseDB(int storageId)
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
                ApplicationDbContext db = new ApplicationDbContext();

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
                ApplicationDbContext db = new ApplicationDbContext();

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
            ApplicationDbContext db = new ApplicationDbContext();
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

                        Regex regex = new Regex(@"(ERROR\s\S*\sFailed\s\S*\s\S*)");
                        bool isMatched = regex.IsMatch(error);
                        if (isMatched)
                        {
                            var errorResult = regex.Match(error).Groups[1].Value;

                            jsonResult = new JsonResult()
                            {
                                Data = new { success = false, val = errorResult },
                                JsonRequestBehavior = JsonRequestBehavior.AllowGet
                            };
                        }
                        else
                        {

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
            ApplicationDbContext db = new ApplicationDbContext();
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
            ApplicationDbContext db = new ApplicationDbContext();
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
            string myConnectionString;

            try
            {
                ApplicationDbContext db = new ApplicationDbContext();
                int storageId = int.Parse(storage);
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
                        jsonResult = GetMySQLDBTable(myConnectionString, storage);
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
                    //case "HBase":
                    //    myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionHBase"].ConnectionString;
                    //    myConnectionString = myConnectionString.Replace("database=default", "database=" + database);
                    //    jsonResult = GetHBaseDBTableV2(myConnectionString);
                    //    break;
                    case "Hive":
                        myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionHive"].ConnectionString;
                        //myConnectionString = myConnectionString.Replace("database=default", "database=" + database);
                        //myConnectionString = myConnectionString.Replace("DATABASE", database);
                        myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                        myConnectionString = myConnectionString.Replace("DATABASE", database);
                        myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                        myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                        myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);
                        jsonResult = GetHiveDBTable(myConnectionString, storage);
                        break;
                    case "Impala":
                        myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionImpala"].ConnectionString;
                        myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                        myConnectionString = myConnectionString.Replace("DATABASE", tbStorage.ToList()[0].DBName);
                        myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                        myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                        myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);
                        jsonResult = GetImpalaDBTable(myConnectionString, storage);
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
                                //modelTB.Add(new object[] { "all" });    //added by Anthony Lai on 2015-08-10 to allow move all tables
                                //var sClusterParameters = db.Storage.Where(i => i.Name == storage);
                                //modelTB.Add(new object[] { sClusterParameters.ToList()[0].TableName });    //added by Anthony Lai on 2015-09-01 to select default table

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
        private JsonResult GetHBaseDBTableV2(string myConnectionString)
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

                    string query = "show \"tables\"";

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

            //var modelDB_new = new List<object[]>();
            //modelDB_new.Add(new object[] { "books" });    //added by Anthony Lai on 2015-09-01 to select default database

            //jsonResult = new JsonResult()
            //{
            //    Data = new { success = true, val = modelDB_new },
            //    JsonRequestBehavior = JsonRequestBehavior.AllowGet
            //};

            return jsonResult;
        }
        private JsonResult GetHiveDBTable(string myConnectionString, string storage)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            try
            {
                //ApplicationDbContext db = new ApplicationDbContext();
                //var sParameters = db.Storage.Where(i => i.Name == storage);
                //myConnectionString = myConnectionString.Replace("HOST_IP", sParameters.ToList()[0].IPAddress).Replace("PORT_NO", sParameters.ToList()[0].Port).Replace("USER_ID", sParameters.ToList()[0].UserName).Replace("PASSWORD", sParameters.ToList()[0].Password);

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
        private JsonResult GetImpalaDBTable(string myConnectionString, string storage)
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
            ApplicationDbContext db = new ApplicationDbContext();
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

                        Regex regex = new Regex(@"(ERROR\s\S*\sFailed\s\S*\s\S*)");
                        bool isMatched = regex.IsMatch(error);
                        if (isMatched)
                        {
                            var errorResult = regex.Match(error).Groups[1].Value;

                            jsonResult = new JsonResult()
                            {
                                Data = new { success = false, val = errorResult },
                                JsonRequestBehavior = JsonRequestBehavior.AllowGet
                            };
                        }

                        else
                        {
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
            string myConnectionString = "";

            //ApplicationDbContext db = new ApplicationDbContext();
            //var sClusterParameters = db.Storage.Where(i => i.Name == source);
            try
            {
                ApplicationDbContext db = new ApplicationDbContext();
                int storageId = int.Parse(source);
                var tbStorage = db.Storage.Where(s => s.Id == storageId).ToList();
                string strStorageType = tbStorage[0].Type;

                //switch (source)
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
                            //jsonResult = GetHBaseQueryResult(source, table, query);
                            jsonResult = GetHBaseQueryResult(tbStorage[0].Id, table, query);
                        }
                        else
                        {
                            myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionHBase"].ConnectionString;
                            //myConnectionString = myConnectionString.Replace("database=default", "database=" + database);
                            myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                            myConnectionString = myConnectionString.Replace("DATABASE", database);
                            myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                            myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                            myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);

                            jsonResult = GetHBaseQueryResultV2(myConnectionString, query);
                        }
                        break;
                    //case "HBase":
                    //    myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionHBase"].ConnectionString;
                    //    myConnectionString = myConnectionString.Replace("database=default", "database=" + database);
                    //    jsonResult = GetHBaseQueryResultV2(myConnectionString, query);
                    //    break;
                    case "Hive":
                        myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionHive"].ConnectionString;
                        //myConnectionString = myConnectionString.Replace("database=default", "database=" + database);
                        //myConnectionString = myConnectionString.Replace("DATABASE", database);
                        myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                        myConnectionString = myConnectionString.Replace("DATABASE", database);
                        myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                        myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                        myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);
                        jsonResult = GetHiveQueryResult(myConnectionString, query, source);
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
                    case "Impala":
                        myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionImpala"].ConnectionString;
                        myConnectionString = myConnectionString.Replace("HOST_IP", tbStorage.ToList()[0].IPAddress);
                        myConnectionString = myConnectionString.Replace("DATABASE", tbStorage.ToList()[0].DBName);
                        myConnectionString = myConnectionString.Replace("PORT_NO", tbStorage.ToList()[0].Port);
                        myConnectionString = myConnectionString.Replace("USER_ID", tbStorage.ToList()[0].UserName);
                        myConnectionString = myConnectionString.Replace("PASSWORD", tbStorage.ToList()[0].Password);
                        jsonResult = GetImpalaQueryResult(myConnectionString, query, source);
                        break;
                    case "Spark":
                        myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionSpark"].ConnectionString;
                        //myConnectionString = myConnectionString.Replace("database=default", "database=" + database);
                        //jsonResult = GeSparkQueryResult(myConnectionString, query);

                        //myConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionHive"].ConnectionString;
                        //myConnectionString = myConnectionString.Replace("database=default", "database=" + database);
                        //jsonResult = GeHiveQueryResult(myConnectionString, query);

                        jsonResult = GetSparkQueryResultV2(myConnectionString, tbStorage[0].Id, table, query);
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

            //var result = new List<dynamic>();

            //var obj = (IDictionary<string, object>)new ExpandoObject();
            //obj.Add("Req_Name", "Ibrahim");
            //obj.Add("Req_Date", "Today");
            //obj.Add("ABC", "CBA");
            //obj.Add("ABbC", "CBA");
            //result.Add(obj);
            //ViewBag.Result = result;

            //return View(result);
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
        //private JsonResult GetHBaseQueryResult(string storage, string table, string query)
        private JsonResult GetHBaseQueryResult(int storageId, string table, string query)
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
            query = query.Replace("\n", " ");

            string strCmd = "\\cp tmpHBaseDBTB.ini tmpHBaseQuery.ini; echo \"" + query + "\" >> tmpHBaseQuery.ini; cat tmpHBaseQuery.ini | hbase shell";

            string cmdType = "SCAN";
            if (query.ToUpper().StartsWith("GET"))
                cmdType = "GET";

            //Find HBase Parameters
            ApplicationDbContext db = new ApplicationDbContext();
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
        private JsonResult GetHiveQueryResult(string myConnectionString, string query, string storage)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            try
            {
                //ApplicationDbContext db = new ApplicationDbContext();
                //var sParameters = db.Storage.Where(i => i.Name == storage);
                //myConnectionString = myConnectionString.Replace("HOST_IP", sParameters.ToList()[0].IPAddress).Replace("PORT_NO", sParameters.ToList()[0].Port).Replace("USER_ID", sParameters.ToList()[0].UserName).Replace("PASSWORD", sParameters.ToList()[0].Password);

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
        private JsonResult GetImpalaQueryResult(string myConnectionString, string query, string storage)
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
        private JsonResult GetSparkQueryResultV2(string myConnectionString, int storageId, string table, string query)
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

            //string strCmd = "\\cp ant.ini tmp.ini; echo \"hiveContext.sql(\\\"" + table + "\\\").collect.foreach(println)\" >> tmp.ini ; echo \"hiveContext.sql(\\\"" + query + "\\\").collect.foreach(println)\" >> tmp.ini; cat tmp.ini | spark-shell | sed '1,/collect/d'";
            string strCmd = "\\cp ant.ini tmp.ini; echo \"sqlContext.sql(\\\"" + table + "\\\").collect.foreach(println)\" >> tmp.ini ; echo \"sqlContext.sql(\\\"" + query + "\\\").collect.foreach(println)\" >> tmp.ini; cat tmp.ini | spark-shell | sed '1,/collect/d'";
            //string strCmd = "\\cp ant.ini tmp.ini; echo \"hiveContext.sql(\\\"" + query + "\\\").collect.foreach(println)\" >> tmp.ini; cat tmp.ini | spark-shell | sed '1,/collect/d'";

            //Find Spark Parameters
            ApplicationDbContext db = new ApplicationDbContext();
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

                        //var modelTB = Read(result).ToList();

                        //int idxcol = result.IndexOf("scala> hiveContext");
                        int idxcol = result.IndexOf("scala> sqlContext");   //Update by Anthony Lai on 2016-03-09 for Spark-Shell
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
                        //if (val[j].Contains("hiveContext.sql"))
                        if (val[j].Contains("sqlContext.sql"))
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

        //
        // POST: /Job/RunJob
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Index(string sourceTable, string destinationTable)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
                //ContentType = "application/x-www-form-urlencoded;charset=utf-8;"
            };
            string result = "";

            //try
            //{
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
            string destination = Request.Form["Destination"];//get the destination selected

            string dbSourceDB = Request.Form["dbSourceDB"];//get the source table selected
            string dbDestinationDB = Request.Form["dbDestinationDB"];//get the destination table selected

            string dbSourceTable = Request.Form["dbSourceTable"];//get the source table selected
            string dbDestinationTable = Request.Form["dbDestinationTable"];//get the destination table selected



            string key = Guid.NewGuid().ToString();

            ApplicationDbContext db = new ApplicationDbContext();
            //Find the job name
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
                var sParameters = db.Storage
                    .Where(i => i.Name == destination);
                strSourceIP = sParameters.ToList()[0].IPAddress;
                strSourceUserName = sParameters.ToList()[0].UserName;
                strSourcePassword = sParameters.ToList()[0].Password;
                strSourceDatabase = sParameters.ToList()[0].DBName;
                strSourceTable = sParameters.ToList()[0].TableName;

                //Destination Parameters
                var dParameters = db.Storage
                    .Where(i => i.Name == source);
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

                DoCreateJobStatus(key, source, strDestinationIP, strDestinationTable, destination, strSourceIP, strSourceTable, strJobName, db);
            }
            else if (strJobName.ToUpper().EndsWith("SQL.JAR"))
            {
                //Source Parameters
                var sParameters = db.Storage
                    .Where(i => i.Name == source);
                strSourceIP = sParameters.ToList()[0].IPAddress;
                strSourceUserName = sParameters.ToList()[0].UserName;
                strSourcePassword = sParameters.ToList()[0].Password;
                strSourceDatabase = sParameters.ToList()[0].DBName;
                strSourceTable = sParameters.ToList()[0].TableName;

                //Destination Parameters
                var dParameters = db.Storage
                    .Where(i => i.Name == destination);
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

                DoCreateJobStatus(key, source, strSourceIP, strSourceTable, destination, strDestinationIP, strDestinationTable, strJobName, db);
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

            //string x = await Task.Run(() => sshCmdClient.RunCmdInRemoteServerAsync());

            //string x = sshCmdClient.RunCmdInRemoteServerAsync();

            //byte[] gb = Guid.NewGuid().ToByteArray();
            //int id = BitConverter.ToInt32(gb, 0);

            //DoCreateJobStatus(key, source, strDestinationIP, strDestinationTable, destination, strSourceIP, strSourceTable, strJobName, db);

            //db.JobStatus.Add
            //(
            //    new JobStatus()
            //    {
            //            Id = i,
            //            Source = source,
            //            SourceIP = strSourceIP,
            //            SourceTable = strSourceTable,
            //            Destination = destination,
            //            DestinationIP = strDestinationIP,
            //            DestinationTable = strDestinationTable,
            //            StartDateTime = DateTime.Now,
            //            //EndDateTime = DateTime.Now,
            //            Status = "Running"
            //        }
            //    );
            //    db.SaveChanges();



            var myTask = sshCmdClient.RunCmdInRemoteServerAsync();
            result = await myTask;
            string tmpResult = result;
            //if(result.Contains("SSH connection shutdown"))            
            if (result.Contains("Return code: 0"))
            {
                result = "Transfer Successfully";

                jsonResult = new JsonResult()
                {
                    Data = new { success = true, val = result },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    //ContentType = "application/x-www-form-urlencoded;charset=utf-8"
                };
            }
            else if (result.Contains("Return code: 1"))
            {
                result = "Transfer Failed";

                jsonResult = new JsonResult()
                {
                    Data = new { success = false, val = result },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    //ContentType = "application/x-www-form-urlencoded;charset=utf-8"
                };
            }


            //jsonResult = new JsonResult()
            //{
            //    Data = new { success = true, val = result },
            //    JsonRequestBehavior = JsonRequestBehavior.AllowGet
            //    //ContentType = "application/x-www-form-urlencoded;charset=utf-8"
            //};

            //update JobStatus 
            DoUpdateJobStatus(key, result, tmpResult);


            //result += cmd.Error;

            //db = new ApplicationDbContext();
            //return View(db.Storage.OrderBy(s => s.Name).ToList());

            //return View(model);
            //}
            //catch (Exception ex)
            //{
            //    string strError = ex.Message;
            //    //return View("Error");
            //    jsonResult = new JsonResult()
            //    {
            //        Data = new { success = false, val = strError },
            //        JsonRequestBehavior = JsonRequestBehavior.AllowGet
            //    };
            //}

            //return View();

            return jsonResult;
        }

        private void DoCreateJobStatus(string key, string source, string strSourceIP, string strSourceTable,
                                       string destination, string strDestinationIP, string strDestinationTable, 
                                       string jobName, ApplicationDbContext db)
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
            catch (Exception ex)
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
                    if (ts.Value.Days > 0)
                    {
                        str = ts.Value.ToString(@"d\.hh\:mm\:ss") + "days";
                    }
                    else if (ts.Value.Hours > 0)
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