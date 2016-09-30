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

namespace EnterpriseDataPipeline.Controllers
{
    //[Authorize(Roles = "Admin")]
    [Authorize(Roles = "Operator, Admin")]
    //[HandleError]
    public class PuppetPEMController : Controller
    {
        static ApplicationDbContext db = new ApplicationDbContext();
        static int key = 0;

        public ActionResult Index()
        {
            key = 0;
            try
            {
                //var result = new List<dynamic>();

                //var obj = (IDictionary<string, object>)new ExpandoObject();
                //obj.Add("Req_Name", "Ibrahim");
                //obj.Add("Req_Date", "Today");
                //result.Add(obj);
                //ViewBag.Result = result;

                key = db.PuppetServer.FirstOrDefault<PuppetServer>().Id;

                //ApplicationDbContext db = new ApplicationDbContext();
                //return View(db.Storage.OrderBy(s => s.Name).ToList());
                return View();
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

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                try
                {
                    string path = Path.Combine(Server.MapPath("~/SSHKey"),
                                               Path.GetFileName(file.FileName));
                    file.SaveAs(path);

                    string fileNameOnly = file.FileName.Split('\\').Last();

                    DoUpdatePuppetPEM(key, fileNameOnly);

                    ViewBag.Message = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }
            return View();
        }


        //[HttpPost]
        //[AllowAnonymous]
        //public async Task<ActionResult> Index(string sourceTable, string destinationTable)
        //{
        //    JsonResult jsonResult = new JsonResult()
        //    {
        //        Data = new { success = false, val = "" },
        //        JsonRequestBehavior = JsonRequestBehavior.AllowGet
        //        //ContentType = "application/x-www-form-urlencoded;charset=utf-8;"
        //    };
        //    string result = "";

        //    //try
        //    //{
        //    string strClusterSourceIP = "";
        //    string strClusterUserName = "";
        //    string strClusterPassword = "";
        //    string strClusterJobDirectory = "";

        //    string strSourceIP = "";
        //    string strSourceUserName = "";
        //    string strSourcePassword = "";
        //    string strSourceDatabase = "";
        //    string strSourceTable = "";

        //    string strDestinationIP = "";
        //    string strDestinationUserName = "";
        //    string strDestinationPassword = "";
        //    string strDestinationDatabase = "";
        //    string strDestinationTable = "";

        //    string strJobName = "";

        //    string source = Request.Form["Source"];//get the source selected
        //    string destination = Request.Form["Destination"];//get the destination selected

        //    string dbSourceDB = Request.Form["dbSourceDB"];//get the source table selected
        //    string dbDestinationDB = Request.Form["dbDestinationDB"];//get the destination table selected

        //    string dbSourceTable = Request.Form["dbSourceTable"];//get the source table selected
        //    string dbDestinationTable = Request.Form["dbDestinationTable"];//get the destination table selected



        //    string key = Guid.NewGuid().ToString();

        //    //ApplicationDbContext db = new ApplicationDbContext();
        //    //Find the job name
        //    var ss = db.JobTB.Where(i => i.Source == source && i.Destinatiion == destination);
        //    if (ss.ToList().Count > 0)
        //    {
        //        strJobName = ss.ToList()[0].JobName;
        //    }
        //    else //No job to run
        //    {
        //        ;   //do nothing
        //    }

        //    //Job Serve Parameters
        //    var sClusterParameters = db.JobServer;
        //    strClusterSourceIP = sClusterParameters.ToList()[0].IPAddress;
        //    strClusterUserName = sClusterParameters.ToList()[0].UserName;
        //    strClusterPassword = sClusterParameters.ToList()[0].Password;
        //    strClusterJobDirectory = sClusterParameters.ToList()[0].JobDirectory;

        //    if (strJobName.ToUpper().EndsWith("HDFS.JAR") || strJobName.ToUpper().EndsWith("HIVE.JAR"))
        //    {
        //        //Source Parameters
        //        var sParameters = db.Storage
        //            .Where(i => i.Name == destination);
        //        strSourceIP = sParameters.ToList()[0].IPAddress;
        //        strSourceUserName = sParameters.ToList()[0].UserName;
        //        strSourcePassword = sParameters.ToList()[0].Password;
        //        strSourceDatabase = sParameters.ToList()[0].DBName;
        //        strSourceTable = sParameters.ToList()[0].TableName;

        //        //Destination Parameters
        //        var dParameters = db.Storage
        //            .Where(i => i.Name == source);
        //        strDestinationIP = dParameters.ToList()[0].IPAddress;
        //        strDestinationUserName = dParameters.ToList()[0].UserName;
        //        strDestinationPassword = dParameters.ToList()[0].Password;
        //        strDestinationDatabase = dParameters.ToList()[0].DBName;
        //        if (dbSourceDB != null)
        //        {
        //            strDestinationDatabase = dbSourceDB;
        //        }
        //        strDestinationTable = dParameters.ToList()[0].TableName;
        //        if (dbSourceTable != null)
        //        {
        //            strDestinationTable = dbSourceTable;
        //        }

        //        DoCreateJobStatus(key, source, strDestinationIP, strDestinationTable, destination, strSourceIP, strSourceTable, strJobName, db);
        //    }
        //    else if (strJobName.ToUpper().EndsWith("SQL.JAR"))
        //    {
        //        //Source Parameters
        //        var sParameters = db.Storage
        //            .Where(i => i.Name == source);
        //        strSourceIP = sParameters.ToList()[0].IPAddress;
        //        strSourceUserName = sParameters.ToList()[0].UserName;
        //        strSourcePassword = sParameters.ToList()[0].Password;
        //        strSourceDatabase = sParameters.ToList()[0].DBName;
        //        strSourceTable = sParameters.ToList()[0].TableName;

        //        //Destination Parameters
        //        var dParameters = db.Storage
        //            .Where(i => i.Name == destination);
        //        strDestinationIP = dParameters.ToList()[0].IPAddress;
        //        strDestinationUserName = dParameters.ToList()[0].UserName;
        //        strDestinationPassword = dParameters.ToList()[0].Password;
        //        strDestinationDatabase = dParameters.ToList()[0].DBName;
        //        if (dbDestinationDB != null)
        //        {
        //            strDestinationDatabase = dbDestinationDB;
        //        }
        //        strDestinationTable = dParameters.ToList()[0].TableName;
        //        if (dbDestinationTable != null)
        //        {
        //            strDestinationTable = dbDestinationTable;
        //        }

        //        DoCreateJobStatus(key, source, strSourceIP, strSourceTable, destination, strDestinationIP, strDestinationTable, strJobName, db);
        //    }



        //    //Initialize sshCmdClient
        //    SSHCommandClient sshCmdClient = new SSHCommandClient(strSourceIP, strSourceUserName, strSourcePassword,
        //                                                         strSourceDatabase, strSourceTable,
        //                                                         strDestinationIP, strDestinationUserName, strDestinationPassword,
        //                                                         strDestinationDatabase, strDestinationTable, strJobName);

        //    sshCmdClient.ClusterHost = strClusterSourceIP;
        //    sshCmdClient.ClusterUserName = strClusterUserName;
        //    sshCmdClient.ClusterPassword = strClusterPassword;
        //    sshCmdClient.ClusterJobDirectory = strClusterJobDirectory;

        //    //string x = await Task.Run(() => sshCmdClient.RunCmdInRemoteServerAsync());

        //    //string x = sshCmdClient.RunCmdInRemoteServerAsync();

        //    //byte[] gb = Guid.NewGuid().ToByteArray();
        //    //int id = BitConverter.ToInt32(gb, 0);

        //    //DoCreateJobStatus(key, source, strDestinationIP, strDestinationTable, destination, strSourceIP, strSourceTable, strJobName, db);

        //    //db.JobStatus.Add
        //    //(
        //    //    new JobStatus()
        //    //    {
        //    //            Id = i,
        //    //            Source = source,
        //    //            SourceIP = strSourceIP,
        //    //            SourceTable = strSourceTable,
        //    //            Destination = destination,
        //    //            DestinationIP = strDestinationIP,
        //    //            DestinationTable = strDestinationTable,
        //    //            StartDateTime = DateTime.Now,
        //    //            //EndDateTime = DateTime.Now,
        //    //            Status = "Running"
        //    //        }
        //    //    );
        //    //    db.SaveChanges();



        //    var myTask = sshCmdClient.RunCmdInRemoteServerAsync();
        //    result = await myTask;
        //    string tmpResult = result;
        //    //if(result.Contains("SSH connection shutdown"))            
        //    if (result.Contains("Return code: 0"))
        //    {
        //        result = "Transfer Successfully";

        //        jsonResult = new JsonResult()
        //        {
        //            Data = new { success = true, val = result },
        //            JsonRequestBehavior = JsonRequestBehavior.AllowGet
        //            //ContentType = "application/x-www-form-urlencoded;charset=utf-8"
        //        };
        //    }
        //    else if (result.Contains("Return code: 1"))
        //    {
        //        result = "Transfer Failed";

        //        jsonResult = new JsonResult()
        //        {
        //            Data = new { success = false, val = result },
        //            JsonRequestBehavior = JsonRequestBehavior.AllowGet
        //            //ContentType = "application/x-www-form-urlencoded;charset=utf-8"
        //        };
        //    }


        //    //jsonResult = new JsonResult()
        //    //{
        //    //    Data = new { success = true, val = result },
        //    //    JsonRequestBehavior = JsonRequestBehavior.AllowGet
        //    //    //ContentType = "application/x-www-form-urlencoded;charset=utf-8"
        //    //};

        //    //update JobStatus 
        //    DoUpdateJobStatus(key, result, tmpResult);


        //    //result += cmd.Error;

        //    //db = new ApplicationDbContext();
        //    //return View(db.Storage.OrderBy(s => s.Name).ToList());

        //    //return View(model);
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    string strError = ex.Message;
        //    //    //return View("Error");
        //    //    jsonResult = new JsonResult()
        //    //    {
        //    //        Data = new { success = false, val = strError },
        //    //        JsonRequestBehavior = JsonRequestBehavior.AllowGet
        //    //    };
        //    //}

        //    //return View();

        //    return jsonResult;
        //}


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


        private void DoUpdatePuppetPEM(int key, string fileName)
        {
            try
            {
                PuppetServer puppetServer;
                //1. Get PuppetServer from DB
                using (var ctx = new ApplicationDbContext())
                {
                    puppetServer = ctx.PuppetServer.Where(s => s.Id == key).FirstOrDefault<PuppetServer>();
                }

                //2. change JobStatus status in disconnected mode (out of ctx scope)
                if (puppetServer != null)
                {
                    puppetServer.PemFile = fileName;
                }

                //save modified entity using new Context
                using (var dbCtx = new ApplicationDbContext())
                {
                    //3. Mark entity as modified
                    dbCtx.Entry(puppetServer).State = System.Data.Entity.EntityState.Modified;

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