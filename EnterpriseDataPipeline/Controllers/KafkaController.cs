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


namespace EnterpriseDataPipeline.Controllers
{
    [Authorize(Roles = "Installer, Admin")]
    //[HandleError]
    public class KafkaController : Controller    
    {
        //static ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            try
            {
                ApplicationDbContext db = new ApplicationDbContext();
                //return View(db.ModuleTB.OrderBy(s => s.Name).ToList());

                ViewModelModuleVM vm = new ViewModelModuleVM();
                //vm.allModuleTB = db.ModuleTB.OrderBy(s => s.Name).ToList();
                //vm.allModuleServer = db.ModuleServer.OrderBy(s => s.Name).ToList(); 
                //vm.allKafkaServer = db.KafkaServer.OrderBy(s => s.Name).ToList();
                vm.allKafkaTopics = db.KafkaTopics.OrderBy(s => s.Topics).ToList();

                return View(vm);
            }
            catch (Exception ex)
            {
                string str = ex.Message;
                return View("Error");
            }

            //return View();
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

        public ActionResult StartKafkaProducer(string module, string destinationServerIP)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
            string result = "";
            string key = Guid.NewGuid().ToString();
            string strInstallationState = "Start Kafka Producer Failed";

            try
            {

                string strPuppetServerIP = "";
                string strPuppetServerName = "";
                string strPuppetServerPassword = "";
                string strPuppetServerPEM = "";
                string strJobLocation = "";


                ApplicationDbContext db = new ApplicationDbContext();

                //Find Kafka Server Parameters
                var sPuppetServerParameters = db.KafkaServer;
                strPuppetServerIP = sPuppetServerParameters.ToList()[0].IPAddress;
                strPuppetServerName = sPuppetServerParameters.ToList()[0].UserName;
                strPuppetServerPassword = sPuppetServerParameters.ToList()[0].Password;
                strPuppetServerPEM = sPuppetServerParameters.ToList()[0].PemFile;
                strJobLocation = sPuppetServerParameters.ToList()[0].JobLocation;


                //string strCmd = "python <job_location> -e <private-ec2-server-name> -i <private-ec2-ip> -u <user-name> -p <password> -k <packages>";
                //string strCmd = "<job_location>/bin/kafka-console-producer.sh --broker-list localhost:9092 --topic <TOPIC>";

                //string strCmd = "cd <JOB_LOCATION> ; <JOB_LOCATION>/bin/connect-standalone.sh <JOB_LOCATION>/config/connect-standalone.properties <JOB_LOCATION>/config/connect-file-source.properties <JOB_LOCATION>/config/connect-file-sink.properties";
                //string strCmd = "cd <JOB_LOCATION>; nohup bin/connect-standalone.sh config/connect-standalone.properties config/connect-file-source.properties config/connect-file-sink.properties &; '\x0d'";
                //string strCmd = "cd <JOB_LOCATION>; nohup bin/connect-standalone.sh config/connect-standalone.properties config/connect-file-source.properties config/connect-file-sink.properties &; '\x0d'";
                string strCmd = "cd <JOB_LOCATION>; bin/connect-standalone.sh config/connect-standalone.properties config/connect-file-source.properties config/connect-file-sink.properties";


                //strCmd = strCmd.Replace("<private-ec2-server-name>", strDestinationServerIP).Replace("<user-name>", strDestinationServerUserName).Replace("<password>", strDestinationServerPassword);
                strCmd = strCmd.Replace("<JOB_LOCATION>", strJobLocation);
                //strCmd = strCmd.Replace("<TOPIC>", destinationServerIP);
                //strCmd = strCmd.Replace("<private-ec2-server-name>", strDestinationServerName).Replace("<private-ec2-ip>", strDestinationServerPrivateIP).Replace("<user-name>", strDestinationServerUserName).Replace("<password>", strDestinationServerPassword);
                //strCmd = strCmd.Replace("<packages>", strPackage);


                //var keyFiles = new[] { keyFile };
                var username = strPuppetServerName;

                var methods = new List<AuthenticationMethod>();
                methods.Add(new PasswordAuthenticationMethod(username, strPuppetServerPassword));
                //methods.Add(new PrivateKeyAuthenticationMethod(username, keyFiles));

                //var con = new ConnectionInfo(strPuppetServerIP, 22, strPuppetServerName, methods.ToArray());


                //var privKey = new PrivateKeyFile(strFileName);
                //ConnectionInfo connectionInfo = new PrivateKeyConnectionInfo(strPuppetServerIP, strPuppetServerName, keyFile);

                var connectionInfo = new ConnectionInfo(strPuppetServerIP, 22, strPuppetServerName, methods.ToArray()) { Timeout = new TimeSpan(0, 0, 0, 30) }; 

                //connectionInfo.Timeout = TimeSpan.

                //ConnectionInfo connectionInfo = new PasswordConnectionInfo(strPuppetServerIP, strPuppetServerName, strPuppetServerPassword);

                //ConnectionInfo connectionInfo = new PasswordConnectionInfo(strPuppetServerIP, strPuppetServerName, strPuppetServerPassword);


                using (SshClient client = new SshClient(connectionInfo))
                {
                    if (!client.IsConnected)
                        client.Connect();

                    if (client.IsConnected)
                    {
                        IDictionary<Renci.SshNet.Common.TerminalModes, uint> termkvp = new Dictionary<Renci.SshNet.Common.TerminalModes, uint>();
                        termkvp.Add(Renci.SshNet.Common.TerminalModes.ECHO, 53);

                        ShellStream shellStream = client.CreateShellStream("xterm", 80, 24, 800, 600, 1024, termkvp);

                        //shellStream.ReadTimeout = 300000;   //Read Timeout = 300 sec

                        //string strRegex = "(\\[USER@\\S*\\s*\\S*\\]$)";
                        string strRegex = "(\\[USER@\\S*\\s*~\\][#|$])";
                        //string strRegex = "([\\s*\\S*]*\\[USER@\\S*\\s*\\S*\\]#)";
                        strRegex = strRegex.Replace("USER", strPuppetServerName);

                        result = shellStream.Expect(new Regex(@strRegex)); //expect user prompt 

                        //send command
                        shellStream.WriteLine(strCmd);
                        shellStream.Flush();
                        result = shellStream.Expect(new Regex(@strRegex)); //expect user prompt

                        //shellStream.Flush();
                        result = result.Replace("<", "'<'");

                        //string strCmdConfigMySQL = "sudo sed -i -e 's/\\(bind-address = \\).*/\\$(VARIABLE)/‘ /etc/my.cnf.d/server.cnf";
                        //string strCmdConfigMySQL = "sudo sed -i -e 's/\\(bind-address = \\).*/\\$(VARIABLE)/'  /etc/my.cnf.d/server.cnf";

                        //strCmdConfigMySQL = strCmdConfigMySQL.Replace("$(VARIABLE)", "1172.31.41.128");
                        //shellStream.WriteLine(strCmdConfigMySQL);
                        //shellStream.Flush();
                        //string strTmpByte = shellStream.Expect(new Regex(@strRegex)); //expect user prompt
                        //result = result + "\n" + strTmpByte;
                        //result = result.Replace("<", "'<'");


                        //string strRegex2 = "(\\d*)\\s*\\d*\\s*/hbase/data/default/TABLE_NAME\\b";
                        //strRegex2 = strRegex2.Replace("TABLE_NAME", dbSourceTable.ToLower());
                        //Regex regex = new Regex(@strRegex2);
                        //if (regex.IsMatch(strTmpByte))
                        //{
                        //    Match match = regex.Match(strTmpByte);
                        //    string strByte = match.Groups[1].ToString();
                        //    long myByte = long.Parse(strByte) * 2;
                        //    //strTmpResult = strTmpResult.Replace("0", myByte.ToString());
                        //}

                        jsonResult = new JsonResult()
                        {
                            Data = new { success = true, val = result },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };

                        //string strRegex2 = "(error|Error):\\s\\[Errno\\s\\S*\\]";
                        string strRegex2 = "\\[Errno\\s\\S*\\]";
                        Regex regex = new Regex(@strRegex2);
                        if (!regex.IsMatch(result))
                        {
                            strInstallationState = "Start Kafka Producer Successfully";
                        }
                        //else
                        //{
                        //    strInstallationState = "Installation Successfully";
                        //}
                    }
                }

                //update JobStatus 
                //DoUpdateJobStatus(key, "Installed Successfully", "");
                //DoUpdateJobStatus(key, strInstallationState, "");
            }
            catch (Exception ex)
            {
                result = strInstallationState + ": " +  ex.Message;

                jsonResult = new JsonResult()
                {
                    Data = new { success = true, val = result },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };

                //update JobStatus 
                //DoUpdateJobStatus(key, "Installation Failed", "");
                //DoUpdateJobStatus(key, strInstallationState, "");
            }

            return jsonResult;
        }

        public ActionResult StartKafkaConsumer(string module, string destinationServerIP)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
            string result = "";
            string key = Guid.NewGuid().ToString();
            string strInstallationState = "Start Consumer Failed";

            try
            {

                string strPuppetServerIP = "";
                string strPuppetServerName = "";
                string strPuppetServerPassword = "";
                string strPuppetServerPEM = "";
                string strJobLocation = "";


                ApplicationDbContext db = new ApplicationDbContext();

                //Find Kafka Server Parameters
                var sPuppetServerParameters = db.KafkaServer;
                strPuppetServerIP = sPuppetServerParameters.ToList()[0].IPAddress;
                strPuppetServerName = sPuppetServerParameters.ToList()[0].UserName;
                strPuppetServerPassword = sPuppetServerParameters.ToList()[0].Password;
                strPuppetServerPEM = sPuppetServerParameters.ToList()[0].PemFile;
                strJobLocation = sPuppetServerParameters.ToList()[0].JobLocation;

                //string strCmd = "<job_location>/bin/kafka-console-consumer.sh --zookeeper localhost:2181 --topic <TOPIC> --from-beginning > <job_location>/testFile &";
                //string strCmd = "cd <JOB_LOCATION>; bin/kafka-console-consumer.sh --zookeeper localhost:2181 --topic <TOPIC> --from-beginning > testFile &";
                string strCmd = "cd <JOB_LOCATION>; bin/kafka-console-consumer.sh --zookeeper localhost:2181 --topic connect-test --from-beginning";

                //strCmd = strCmd.Replace("<private-ec2-server-name>", strDestinationServerIP).Replace("<user-name>", strDestinationServerUserName).Replace("<password>", strDestinationServerPassword);
                strCmd = strCmd.Replace("<JOB_LOCATION>", strJobLocation);
                strCmd = strCmd.Replace("<TOPIC>", destinationServerIP);
                //strCmd = strCmd.Replace("<private-ec2-server-name>", strDestinationServerName).Replace("<private-ec2-ip>", strDestinationServerPrivateIP).Replace("<user-name>", strDestinationServerUserName).Replace("<password>", strDestinationServerPassword);
                //strCmd = strCmd.Replace("<packages>", strPackage);

                //string key = Guid.NewGuid().ToString();
                //DoCreateJobStatus(key, strDestinationServerName, strDestinationServerIP, strPackage, db);
                //DoCreateJobStatus(key, strDestinationServerName, strDestinationServerIP, module, db);

                //ConnectionInfo connectionInfo = new PasswordConnectionInfo(strPuppetServerIP, strPuppetServerName, strPuppetServerPassword);

                //var privKey = new PrivateKeyFile(new MemoryStream(Encoding.ASCII.GetBytes(sshPrivateKeyString)));
                //string strFileName = "";
                //var path = Server.MapPath(@"~/SSHKey/sv4u.pem");

                //var path = Server.MapPath(@"~/SSHKey/" + strPuppetServerPEM);
                //var keyFile = new PrivateKeyFile(path);

                //var keyFiles = new[] { keyFile };
                var username = strPuppetServerName;

                var methods = new List<AuthenticationMethod>();
                methods.Add(new PasswordAuthenticationMethod(username, strPuppetServerPassword));
                //methods.Add(new PrivateKeyAuthenticationMethod(username, keyFiles));

                //var con = new ConnectionInfo(strPuppetServerIP, 22, strPuppetServerName, methods.ToArray());


                //var privKey = new PrivateKeyFile(strFileName);
                //ConnectionInfo connectionInfo = new PrivateKeyConnectionInfo(strPuppetServerIP, strPuppetServerName, keyFile);

                var connectionInfo = new ConnectionInfo(strPuppetServerIP, 22, strPuppetServerName, methods.ToArray());

                //connectionInfo.Timeout = TimeSpan.

                //ConnectionInfo connectionInfo = new PasswordConnectionInfo(strPuppetServerIP, strPuppetServerName, strPuppetServerPassword);

                //ConnectionInfo connectionInfo = new PasswordConnectionInfo(strPuppetServerIP, strPuppetServerName, strPuppetServerPassword);


                using (SshClient client = new SshClient(connectionInfo))
                {
                    if (!client.IsConnected)
                        client.Connect();

                    if (client.IsConnected)
                    {
                        IDictionary<Renci.SshNet.Common.TerminalModes, uint> termkvp = new Dictionary<Renci.SshNet.Common.TerminalModes, uint>();
                        termkvp.Add(Renci.SshNet.Common.TerminalModes.ECHO, 53);

                        ShellStream shellStream = client.CreateShellStream("xterm", 80, 24, 800, 600, 1024, termkvp);

                        //shellStream.ReadTimeout = 300000;   //Read Timeout = 300 sec

                        //string strRegex = "(\\[USER@\\S*\\s*\\S*\\]$)";
                        string strRegex = "(\\[USER@\\S*\\s*~\\][#|$])";
                        //string strRegex = "([\\s*\\S*]*\\[USER@\\S*\\s*\\S*\\]#)";
                        strRegex = strRegex.Replace("USER", strPuppetServerName);

                        result = shellStream.Expect(new Regex(@strRegex)); //expect user prompt 

                        //send command
                        shellStream.WriteLine(strCmd);
                        shellStream.Flush();
                        result = shellStream.Expect(new Regex(@strRegex)); //expect user prompt
                        result = result.Replace("<", "'<'");

                        //string strCmdConfigMySQL = "sudo sed -i -e 's/\\(bind-address = \\).*/\\$(VARIABLE)/‘ /etc/my.cnf.d/server.cnf";
                        //string strCmdConfigMySQL = "sudo sed -i -e 's/\\(bind-address = \\).*/\\$(VARIABLE)/'  /etc/my.cnf.d/server.cnf";

                        //strCmdConfigMySQL = strCmdConfigMySQL.Replace("$(VARIABLE)", "1172.31.41.128");
                        //shellStream.WriteLine(strCmdConfigMySQL);
                        //shellStream.Flush();
                        //string strTmpByte = shellStream.Expect(new Regex(@strRegex)); //expect user prompt
                        //result = result + "\n" + strTmpByte;
                        //result = result.Replace("<", "'<'");


                        //string strRegex2 = "(\\d*)\\s*\\d*\\s*/hbase/data/default/TABLE_NAME\\b";
                        //strRegex2 = strRegex2.Replace("TABLE_NAME", dbSourceTable.ToLower());
                        //Regex regex = new Regex(@strRegex2);
                        //if (regex.IsMatch(strTmpByte))
                        //{
                        //    Match match = regex.Match(strTmpByte);
                        //    string strByte = match.Groups[1].ToString();
                        //    long myByte = long.Parse(strByte) * 2;
                        //    //strTmpResult = strTmpResult.Replace("0", myByte.ToString());
                        //}



                        //string strRegex2 = "(error|Error):\\s\\[Errno\\s\\S*\\]";
                        string strRegex2 = "\\[Errno\\s\\S*\\]";
                        Regex regex = new Regex(@strRegex2);
                        if (!regex.IsMatch(result))
                        {
                            strInstallationState = "Start Consumer Successfully";
                        }
                        //else
                        //{
                        //    strInstallationState = "Installation Successfully";
                        //}

                        jsonResult = new JsonResult()
                        {
                            Data = new { success = true, val = result },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                    }
                }

                //update JobStatus 
                //DoUpdateJobStatus(key, "Installed Successfully", "");
                //DoUpdateJobStatus(key, strInstallationState, "");
            }
            catch (Exception ex)
            {
                result = strInstallationState + ": " + ex.Message;

                jsonResult = new JsonResult()
                {
                    Data = new { success = true, val = result },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };

                //update JobStatus 
                //DoUpdateJobStatus(key, "Installation Failed", "");
                //DoUpdateJobStatus(key, strInstallationState, "");
            }

            return jsonResult;
        }

        public ActionResult SendMessage(string module, string destinationServerIP, string message)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
            string result = "";
            string key = Guid.NewGuid().ToString();
            string strInstallationState = "Send Message Failed";

            try
            {

                string strPuppetServerIP = "";
                string strPuppetServerName = "";
                string strPuppetServerPassword = "";
                string strPuppetServerPEM = "";
                string strJobLocation = "";


                ApplicationDbContext db = new ApplicationDbContext();

                //Find Kafka Server Parameters
                var sPuppetServerParameters = db.KafkaServer;
                strPuppetServerIP = sPuppetServerParameters.ToList()[0].IPAddress;
                strPuppetServerName = sPuppetServerParameters.ToList()[0].UserName;
                strPuppetServerPassword = sPuppetServerParameters.ToList()[0].Password;
                strPuppetServerPEM = sPuppetServerParameters.ToList()[0].PemFile;
                strJobLocation = sPuppetServerParameters.ToList()[0].JobLocation;

                string strCmd = "echo \"<MESSAGE>\" >> <JOB_LOCATION>/test.txt";

                //strCmd = strCmd.Replace("<private-ec2-server-name>", strDestinationServerIP).Replace("<user-name>", strDestinationServerUserName).Replace("<password>", strDestinationServerPassword);
                strCmd = strCmd.Replace("<JOB_LOCATION>", strJobLocation);
                strCmd = strCmd.Replace("<MESSAGE>", message);
                //strCmd = strCmd.Replace("<private-ec2-server-name>", strDestinationServerName).Replace("<private-ec2-ip>", strDestinationServerPrivateIP).Replace("<user-name>", strDestinationServerUserName).Replace("<password>", strDestinationServerPassword);
                //strCmd = strCmd.Replace("<packages>", strPackage);

                //string key = Guid.NewGuid().ToString();
                //DoCreateJobStatus(key, strDestinationServerName, strDestinationServerIP, strPackage, db);
                //DoCreateJobStatus(key, strDestinationServerName, strDestinationServerIP, module, db);

                //ConnectionInfo connectionInfo = new PasswordConnectionInfo(strPuppetServerIP, strPuppetServerName, strPuppetServerPassword);

                //var privKey = new PrivateKeyFile(new MemoryStream(Encoding.ASCII.GetBytes(sshPrivateKeyString)));
                //string strFileName = "";
                //var path = Server.MapPath(@"~/SSHKey/sv4u.pem");

                //var path = Server.MapPath(@"~/SSHKey/" + strPuppetServerPEM);
                //var keyFile = new PrivateKeyFile(path);

                //var keyFiles = new[] { keyFile };
                var username = strPuppetServerName;

                var methods = new List<AuthenticationMethod>();
                methods.Add(new PasswordAuthenticationMethod(username, strPuppetServerPassword));
                //methods.Add(new PrivateKeyAuthenticationMethod(username, keyFiles));

                //var con = new ConnectionInfo(strPuppetServerIP, 22, strPuppetServerName, methods.ToArray());


                //var privKey = new PrivateKeyFile(strFileName);
                //ConnectionInfo connectionInfo = new PrivateKeyConnectionInfo(strPuppetServerIP, strPuppetServerName, keyFile);

                var connectionInfo = new ConnectionInfo(strPuppetServerIP, 22, strPuppetServerName, methods.ToArray());

                //connectionInfo.Timeout = TimeSpan.

                //ConnectionInfo connectionInfo = new PasswordConnectionInfo(strPuppetServerIP, strPuppetServerName, strPuppetServerPassword);

                //ConnectionInfo connectionInfo = new PasswordConnectionInfo(strPuppetServerIP, strPuppetServerName, strPuppetServerPassword);


                using (SshClient client = new SshClient(connectionInfo))
                {
                    if (!client.IsConnected)
                        client.Connect();

                    if (client.IsConnected)
                    {
                        IDictionary<Renci.SshNet.Common.TerminalModes, uint> termkvp = new Dictionary<Renci.SshNet.Common.TerminalModes, uint>();
                        termkvp.Add(Renci.SshNet.Common.TerminalModes.ECHO, 53);

                        ShellStream shellStream = client.CreateShellStream("xterm", 80, 24, 800, 600, 1024, termkvp);

                        //shellStream.ReadTimeout = 300000;   //Read Timeout = 300 sec

                        //string strRegex = "(\\[USER@\\S*\\s*\\S*\\]$)";
                        string strRegex = "(\\[USER@\\S*\\s*~\\][#|$])";
                        //string strRegex = "([\\s*\\S*]*\\[USER@\\S*\\s*\\S*\\]#)";
                        strRegex = strRegex.Replace("USER", strPuppetServerName);

                        result = shellStream.Expect(new Regex(@strRegex)); //expect user prompt 

                        //send command
                        shellStream.WriteLine(strCmd);
                        shellStream.Flush();
                        result = shellStream.Expect(new Regex(@strRegex)); //expect user prompt
                        result = result.Replace("<", "'<'");

                        //string strCmdConfigMySQL = "sudo sed -i -e 's/\\(bind-address = \\).*/\\$(VARIABLE)/‘ /etc/my.cnf.d/server.cnf";
                        //string strCmdConfigMySQL = "sudo sed -i -e 's/\\(bind-address = \\).*/\\$(VARIABLE)/'  /etc/my.cnf.d/server.cnf";

                        //strCmdConfigMySQL = strCmdConfigMySQL.Replace("$(VARIABLE)", "1172.31.41.128");
                        //shellStream.WriteLine(strCmdConfigMySQL);
                        //shellStream.Flush();
                        //string strTmpByte = shellStream.Expect(new Regex(@strRegex)); //expect user prompt
                        //result = result + "\n" + strTmpByte;
                        //result = result.Replace("<", "'<'");


                        //string strRegex2 = "(\\d*)\\s*\\d*\\s*/hbase/data/default/TABLE_NAME\\b";
                        //strRegex2 = strRegex2.Replace("TABLE_NAME", dbSourceTable.ToLower());
                        //Regex regex = new Regex(@strRegex2);
                        //if (regex.IsMatch(strTmpByte))
                        //{
                        //    Match match = regex.Match(strTmpByte);
                        //    string strByte = match.Groups[1].ToString();
                        //    long myByte = long.Parse(strByte) * 2;
                        //    //strTmpResult = strTmpResult.Replace("0", myByte.ToString());
                        //}

                        jsonResult = new JsonResult()
                        {
                            Data = new { success = true, val = result },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };

                        //string strRegex2 = "(error|Error):\\s\\[Errno\\s\\S*\\]";
                        string strRegex2 = "\\[Errno\\s\\S*\\]";
                        Regex regex = new Regex(@strRegex2);
                        if (!regex.IsMatch(result))
                        {
                            strInstallationState = "Send Message Successfully";
                        }
                        //else
                        //{
                        //    strInstallationState = "Installation Successfully";
                        //}
                    }
                }

                //update JobStatus 
                //DoUpdateJobStatus(key, "Installed Successfully", "");
                DoUpdateJobStatus(key, strInstallationState, "");
            }
            catch (Exception ex)
            {
                result = ex.Message;

                jsonResult = new JsonResult()
                {
                    Data = new { success = true, val = result },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };

                //update JobStatus 
                //DoUpdateJobStatus(key, "Installation Failed", "");
                DoUpdateJobStatus(key, strInstallationState, "");
            }

            return jsonResult;
        }

        //public ActionResult RunLoadMessage(string module, string destinationServerIP)        
        //{
        //    JsonResult jsonResult = new JsonResult()
        //    {
        //        Data = new { success = false, val = "" },
        //        JsonRequestBehavior = JsonRequestBehavior.AllowGet
        //    };
        //    string result = "";
        //    string key = Guid.NewGuid().ToString();
        //    string strInstallationState = "Load Message Failed";

        //    try
        //    {

        //        string strPuppetServerIP = "";
        //        string strPuppetServerName = "";
        //        string strPuppetServerPassword = "";
        //        string strPuppetServerPEM = "";
        //        string strJobLocation = "";

        //        //string strDestinationServerIP = "";
        //        //string strDestinationServerPrivateIP = "";
        //        //string strDestinationServerName = "";
        //        //string strDestinationServerUserName = "";
        //        //string strDestinationServerPassword = "";

        //        //module = module.Remove(module.Length-1);
        //        //string[] str = module.Split(',');
        //        //string strPackage = "";

        //        //if (str.Length == 1)
        //        //{
        //        //    strPackage += str[0];
        //        //    strPackage = strPackage.ToLower();
        //        //}
        //        //else
        //        //{
        //        //    strPackage = "<";
        //        //    for (int i = 0; i < str.Length; i++)
        //        //    {
        //        //        strPackage += str[i] + ":";
        //        //    }

        //        //    strPackage = strPackage.Remove(strPackage.Length - 1);
        //        //    strPackage = strPackage.ToLower() + ">";

        //        //    if(!strPackage.Contains(","))
        //        //    {
        //        //       strPackage = strPackage.Replace("<", "").Replace(">", "");
        //        //    }
        //        //}

        //        ////special handling for MySQL server installation
        //        //strPackage = strPackage.Replace("mysql", "'::mysql::server'");

        //        ApplicationDbContext db = new ApplicationDbContext();

        //        //Find Kafka Server Parameters
        //        var sPuppetServerParameters = db.KafkaServer;
        //        strPuppetServerIP = sPuppetServerParameters.ToList()[0].IPAddress;
        //        strPuppetServerName = sPuppetServerParameters.ToList()[0].UserName;
        //        strPuppetServerPassword = sPuppetServerParameters.ToList()[0].Password;
        //        strPuppetServerPEM = sPuppetServerParameters.ToList()[0].PemFile;
        //        strJobLocation = sPuppetServerParameters.ToList()[0].JobLocation;

        //        //Find ModuleServer Parameters
        //        //var sClusterParameters = db.ModuleServer.Where(i => i.IPAddress == destinationServerIP);
        //        //strDestinationServerPrivateIP = sClusterParameters.ToList()[0].PrivateIPAddress;
        //        //strDestinationServerIP = sClusterParameters.ToList()[0].IPAddress;
        //        //strDestinationServerName = sClusterParameters.ToList()[0].Name;
        //        //strDestinationServerUserName = sClusterParameters.ToList()[0].UserName;
        //        //strDestinationServerPassword = sClusterParameters.ToList()[0].Password;

        //        //string strCmd = "python <job_location> -e <private-ec2-server-name> -i <private-ec2-ip> -u <user-name> -p <password> -k <packages>";

        //        string strCmd = "java -jar <job_location>";

        //        //strCmd = strCmd.Replace("<private-ec2-server-name>", strDestinationServerIP).Replace("<user-name>", strDestinationServerUserName).Replace("<password>", strDestinationServerPassword);
        //        strCmd = strCmd.Replace("<job_location>", strJobLocation);
        //        //strCmd = strCmd.Replace("<private-ec2-server-name>", strDestinationServerName).Replace("<private-ec2-ip>", strDestinationServerPrivateIP).Replace("<user-name>", strDestinationServerUserName).Replace("<password>", strDestinationServerPassword);
        //        //strCmd = strCmd.Replace("<packages>", strPackage);

        //        //string key = Guid.NewGuid().ToString();
        //        //DoCreateJobStatus(key, strDestinationServerName, strDestinationServerIP, strPackage, db);
        //        //DoCreateJobStatus(key, strDestinationServerName, strDestinationServerIP, module, db);

        //        //ConnectionInfo connectionInfo = new PasswordConnectionInfo(strPuppetServerIP, strPuppetServerName, strPuppetServerPassword);

        //        //var privKey = new PrivateKeyFile(new MemoryStream(Encoding.ASCII.GetBytes(sshPrivateKeyString)));
        //        //string strFileName = "";
        //        //var path = Server.MapPath(@"~/SSHKey/sv4u.pem");

        //        //var path = Server.MapPath(@"~/SSHKey/" + strPuppetServerPEM);
        //        //var keyFile = new PrivateKeyFile(path);

        //        //var keyFiles = new[] { keyFile };
        //        var username = strPuppetServerName;

        //        var methods = new List<AuthenticationMethod>();
        //        methods.Add(new PasswordAuthenticationMethod(username, strPuppetServerPassword));
        //        //methods.Add(new PrivateKeyAuthenticationMethod(username, keyFiles));

        //        //var con = new ConnectionInfo(strPuppetServerIP, 22, strPuppetServerName, methods.ToArray());


        //        //var privKey = new PrivateKeyFile(strFileName);
        //        //ConnectionInfo connectionInfo = new PrivateKeyConnectionInfo(strPuppetServerIP, strPuppetServerName, keyFile);

        //        var connectionInfo = new ConnectionInfo(strPuppetServerIP, 22, strPuppetServerName, methods.ToArray());

        //        //connectionInfo.Timeout = TimeSpan.

        //        //ConnectionInfo connectionInfo = new PasswordConnectionInfo(strPuppetServerIP, strPuppetServerName, strPuppetServerPassword);

        //        //ConnectionInfo connectionInfo = new PasswordConnectionInfo(strPuppetServerIP, strPuppetServerName, strPuppetServerPassword);


        //        using (SshClient client = new SshClient(connectionInfo))
        //        {
        //            if (!client.IsConnected)
        //                client.Connect();

        //            if (client.IsConnected)
        //            {
        //                IDictionary<Renci.SshNet.Common.TerminalModes, uint> termkvp = new Dictionary<Renci.SshNet.Common.TerminalModes, uint>();
        //                termkvp.Add(Renci.SshNet.Common.TerminalModes.ECHO, 53);

        //                ShellStream shellStream = client.CreateShellStream("xterm", 80, 24, 800, 600, 1024, termkvp);

        //                //shellStream.ReadTimeout = 300000;   //Read Timeout = 300 sec

        //                //string strRegex = "(\\[USER@\\S*\\s*\\S*\\]$)";
        //                string strRegex = "(\\[USER@\\S*\\s*~\\][#|$])";
        //                //string strRegex = "([\\s*\\S*]*\\[USER@\\S*\\s*\\S*\\]#)";
        //                strRegex = strRegex.Replace("USER", strPuppetServerName);

        //                result = shellStream.Expect(new Regex(@strRegex)); //expect user prompt 

        //                //send command
        //                shellStream.WriteLine(strCmd);
        //                shellStream.Flush();
        //                result = shellStream.Expect(new Regex(@strRegex)); //expect user prompt
        //                result = result.Replace("<", "'<'");

        //                //string strCmdConfigMySQL = "sudo sed -i -e 's/\\(bind-address = \\).*/\\$(VARIABLE)/‘ /etc/my.cnf.d/server.cnf";
        //                //string strCmdConfigMySQL = "sudo sed -i -e 's/\\(bind-address = \\).*/\\$(VARIABLE)/'  /etc/my.cnf.d/server.cnf";

        //                //strCmdConfigMySQL = strCmdConfigMySQL.Replace("$(VARIABLE)", "1172.31.41.128");
        //                //shellStream.WriteLine(strCmdConfigMySQL);
        //                //shellStream.Flush();
        //                //string strTmpByte = shellStream.Expect(new Regex(@strRegex)); //expect user prompt
        //                //result = result + "\n" + strTmpByte;
        //                //result = result.Replace("<", "'<'");


        //                //string strRegex2 = "(\\d*)\\s*\\d*\\s*/hbase/data/default/TABLE_NAME\\b";
        //                //strRegex2 = strRegex2.Replace("TABLE_NAME", dbSourceTable.ToLower());
        //                //Regex regex = new Regex(@strRegex2);
        //                //if (regex.IsMatch(strTmpByte))
        //                //{
        //                //    Match match = regex.Match(strTmpByte);
        //                //    string strByte = match.Groups[1].ToString();
        //                //    long myByte = long.Parse(strByte) * 2;
        //                //    //strTmpResult = strTmpResult.Replace("0", myByte.ToString());
        //                //}

        //                jsonResult = new JsonResult()
        //                {
        //                    Data = new { success = true, val = result },
        //                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
        //                };

        //                //string strRegex2 = "(error|Error):\\s\\[Errno\\s\\S*\\]";
        //                string strRegex2 = "\\[Errno\\s\\S*\\]";
        //                Regex regex = new Regex(@strRegex2);
        //                if (!regex.IsMatch(result))
        //                {
        //                    strInstallationState = "Load Message Successfully";
        //                }
        //                //else
        //                //{
        //                //    strInstallationState = "Installation Successfully";
        //                //}
        //            }
        //        }

        //        //update JobStatus 
        //        //DoUpdateJobStatus(key, "Installed Successfully", "");
        //        DoUpdateJobStatus(key, strInstallationState, "");
        //    }
        //    catch (Exception ex)
        //    {
        //        result = ex.Message;

        //        jsonResult = new JsonResult()
        //        {
        //            Data = new { success = true, val = result },
        //            JsonRequestBehavior = JsonRequestBehavior.AllowGet
        //        };

        //        //update JobStatus 
        //        //DoUpdateJobStatus(key, "Installation Failed", "");
        //        DoUpdateJobStatus(key, strInstallationState, "");
        //    }

        //    return jsonResult;
        //}

        public ActionResult RunDisplayMessage(string module, string destinationServerIP)
        {
            JsonResult jsonResult = new JsonResult()
            {
                Data = new { success = false, val = "" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
            string result = "";
            string key = Guid.NewGuid().ToString();
            string strInstallationState = "Display Message Failed";

            try
            {

                string strPuppetServerIP = "";
                string strPuppetServerName = "";
                string strPuppetServerPassword = "";
                string strPuppetServerPEM = "";
                string strJobLocation = "";

                //string strDestinationServerIP = "";
                //string strDestinationServerPrivateIP = "";
                //string strDestinationServerName = "";
                //string strDestinationServerUserName = "";
                //string strDestinationServerPassword = "";

                //module = module.Remove(module.Length-1);
                //string[] str = module.Split(',');
                //string strPackage = "";

                //if (str.Length == 1)
                //{
                //    strPackage += str[0];
                //    strPackage = strPackage.ToLower();
                //}
                //else
                //{
                //    strPackage = "<";
                //    for (int i = 0; i < str.Length; i++)
                //    {
                //        strPackage += str[i] + ":";
                //    }

                //    strPackage = strPackage.Remove(strPackage.Length - 1);
                //    strPackage = strPackage.ToLower() + ">";

                //    if(!strPackage.Contains(","))
                //    {
                //       strPackage = strPackage.Replace("<", "").Replace(">", "");
                //    }
                //}

                ////special handling for MySQL server installation
                //strPackage = strPackage.Replace("mysql", "'::mysql::server'");

                ApplicationDbContext db = new ApplicationDbContext();

                //Find Kafka Server Parameters
                var sPuppetServerParameters = db.KafkaServer;
                strPuppetServerIP = sPuppetServerParameters.ToList()[0].IPAddress;
                strPuppetServerName = sPuppetServerParameters.ToList()[0].UserName;
                strPuppetServerPassword = sPuppetServerParameters.ToList()[0].Password;
                strPuppetServerPEM = sPuppetServerParameters.ToList()[0].PemFile;
                strJobLocation = sPuppetServerParameters.ToList()[0].JobLocation;

                //Find ModuleServer Parameters
                //var sClusterParameters = db.ModuleServer.Where(i => i.IPAddress == destinationServerIP);
                //strDestinationServerPrivateIP = sClusterParameters.ToList()[0].PrivateIPAddress;
                //strDestinationServerIP = sClusterParameters.ToList()[0].IPAddress;
                //strDestinationServerName = sClusterParameters.ToList()[0].Name;
                //strDestinationServerUserName = sClusterParameters.ToList()[0].UserName;
                //strDestinationServerPassword = sClusterParameters.ToList()[0].Password;

                //string strCmd = "python <job_location> -e <private-ec2-server-name> -i <private-ec2-ip> -u <user-name> -p <password> -k <packages>";

                //string strCmd = "/root/kafka_2.11-0.9.0.0/bin/kafka-console-consumer.sh --zookeeper localhost:2181 --topic test --from-beginning";
                //string strCmd = "cat /root/kafka_2.11-0.9.0.0/test.sink.txt";
                string strCmd = "cat /tmp/test.sink.txt";


                //strCmd = strCmd.Replace("<private-ec2-server-name>", strDestinationServerIP).Replace("<user-name>", strDestinationServerUserName).Replace("<password>", strDestinationServerPassword);
                //strCmd = strCmd.Replace("<job_location>", strJobLocation);
                //strCmd = strCmd.Replace("<private-ec2-server-name>", strDestinationServerName).Replace("<private-ec2-ip>", strDestinationServerPrivateIP).Replace("<user-name>", strDestinationServerUserName).Replace("<password>", strDestinationServerPassword);
                //strCmd = strCmd.Replace("<packages>", strPackage);

                //string key = Guid.NewGuid().ToString();
                //DoCreateJobStatus(key, strDestinationServerName, strDestinationServerIP, strPackage, db);
                //DoCreateJobStatus(key, strDestinationServerName, strDestinationServerIP, module, db);

                //ConnectionInfo connectionInfo = new PasswordConnectionInfo(strPuppetServerIP, strPuppetServerName, strPuppetServerPassword);

                //var privKey = new PrivateKeyFile(new MemoryStream(Encoding.ASCII.GetBytes(sshPrivateKeyString)));
                //string strFileName = "";
                //var path = Server.MapPath(@"~/SSHKey/sv4u.pem");

                //var path = Server.MapPath(@"~/SSHKey/" + strPuppetServerPEM);
                //var keyFile = new PrivateKeyFile(path);

                //var keyFiles = new[] { keyFile };
                var username = strPuppetServerName;

                var methods = new List<AuthenticationMethod>();
                methods.Add(new PasswordAuthenticationMethod(username, strPuppetServerPassword));
                //methods.Add(new PrivateKeyAuthenticationMethod(username, keyFiles));

                //var con = new ConnectionInfo(strPuppetServerIP, 22, strPuppetServerName, methods.ToArray());


                //var privKey = new PrivateKeyFile(strFileName);
                //ConnectionInfo connectionInfo = new PrivateKeyConnectionInfo(strPuppetServerIP, strPuppetServerName, keyFile);

                //var connectionInfo = new ConnectionInfo(strPuppetServerIP, 22, strPuppetServerName, methods.ToArray());

                ConnectionInfo connectionInfo = new PasswordConnectionInfo(strPuppetServerIP, strPuppetServerName, strPuppetServerPassword) { Timeout = new TimeSpan(0, 0, 0, 60) };

                using (SshClient client = new SshClient(connectionInfo))
                {
                    if (!client.IsConnected)
                        client.Connect();

                    if (client.IsConnected)
                    {
                        IDictionary<Renci.SshNet.Common.TerminalModes, uint> termkvp = new Dictionary<Renci.SshNet.Common.TerminalModes, uint>();
                        termkvp.Add(Renci.SshNet.Common.TerminalModes.ECHO, 53);

                        ShellStream shellStream = client.CreateShellStream("xterm", 80, 24, 800, 600, 1024, termkvp);

                        //string strRegex = "(\\[USER@\\S*\\s*\\S*\\]$)";
                        string strRegex = "(\\[USER@\\S*\\s*~\\][#|$])";
                        //string strRegex = "([\\s*\\S*]*\\[USER@\\S*\\s*\\S*\\]#)";
                        strRegex = strRegex.Replace("USER", strPuppetServerName);

                        result = shellStream.Expect(new Regex(@strRegex)); //expect user prompt 

                        //send command
                        shellStream.WriteLine(strCmd);
                        shellStream.Flush();
                        result = shellStream.Expect(new Regex(@strRegex)); //expect user prompt
                        result = result.Replace("<", "'<'");

                        //string strCmdConfigMySQL = "sudo sed -i -e 's/\\(bind-address = \\).*/\\$(VARIABLE)/‘ /etc/my.cnf.d/server.cnf";
                        //string strCmdConfigMySQL = "sudo sed -i -e 's/\\(bind-address = \\).*/\\$(VARIABLE)/'  /etc/my.cnf.d/server.cnf";

                        //strCmdConfigMySQL = strCmdConfigMySQL.Replace("$(VARIABLE)", "1172.31.41.128");
                        //shellStream.WriteLine(strCmdConfigMySQL);
                        //shellStream.Flush();
                        //string strTmpByte = shellStream.Expect(new Regex(@strRegex)); //expect user prompt
                        //result = result + "\n" + strTmpByte;
                        //result = result.Replace("<", "'<'");


                        //string strRegex2 = "(\\d*)\\s*\\d*\\s*/hbase/data/default/TABLE_NAME\\b";
                        //strRegex2 = strRegex2.Replace("TABLE_NAME", dbSourceTable.ToLower());
                        //Regex regex = new Regex(@strRegex2);
                        //if (regex.IsMatch(strTmpByte))
                        //{
                        //    Match match = regex.Match(strTmpByte);
                        //    string strByte = match.Groups[1].ToString();
                        //    long myByte = long.Parse(strByte) * 2;
                        //    //strTmpResult = strTmpResult.Replace("0", myByte.ToString());
                        //}



                        //string strRegex2 = "(error|Error):\\s\\[Errno\\s\\S*\\]";
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
                }

                //update JobStatus 
                //DoUpdateJobStatus(key, "Installed Successfully", "");
                //DoUpdateJobStatus(key, strInstallationState, "");
            }
            catch (Exception ex)
            {
                result = strInstallationState + ": " + ex.Message;

                jsonResult = new JsonResult()
                {
                    Data = new { success = true, val = result },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };

                //update JobStatus 
                //DoUpdateJobStatus(key, "Installation Failed", "");
                //DoUpdateJobStatus(key, strInstallationState, "");
            }

            return jsonResult;
        }

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

                //DoCreateJobStatus(key, source, strDestinationIP, strDestinationTable, destination, strSourceIP, strSourceTable, strJobName, db);
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

                //DoCreateJobStatus(key, source, strSourceIP, strSourceTable, destination, strDestinationIP, strDestinationTable, strJobName, db);
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
                result = "Transfer Successfully";

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

        private void DoCreateJobStatus(string key, string strDestinationServerName, string strDestinationServerIP, string strPackage, ApplicationDbContext db)
        {
            try
            {
                //DateTime now = DateTime.Now;
                //DateTime now = DateTime.UtcNow;
                db.ModuleStatus.Add
                (
                    new ModuleStatus()
                    {
                        Key = key,
                        ServerName = strDestinationServerName,
                        IPAddress = strDestinationServerIP,
                        ModuleName = strPackage,
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
            try
            {
                ModuleStatus moduleStatus;
                //1. Get JobStatus from DB
                using (var ctx = new ApplicationDbContext())
                {
                    moduleStatus = ctx.ModuleStatus.Where(s => s.Key == key).FirstOrDefault<ModuleStatus>();
                }

                //2. change JobStatus status in disconnected mode (out of ctx scope)
                if (moduleStatus != null)
                {
                    //string strByteTransfter = "";
                    ////Regex r = new Regex(@"mapreduce.Import|ExportJobBase: Transferred\s(\S*\sbytes|\S*\sKB)");
                    //string pattern = @"mapreduce.[ImportJobBase|ExportJobBase]*:\sTransferred\s(\S*\s[PB|TB|GB|MB|KB|bytes]*)";
                    ////strByteTransfter = r.Match(@tmpResult).Groups[1].Value;
                    //MatchCollection matches = Regex.Matches(tmpResult, pattern);
                    //double myNum = 0.0;

                    //foreach (Match match in matches)
                    //{
                    //    GroupCollection groups = match.Groups;
                    //    string strVal = groups[1].Value;
                    //    if (strVal.Contains("bytes"))
                    //    {
                    //        myNum += double.Parse(strVal.Replace("bytes", "").Trim());
                    //    }
                    //    else if (strVal.Contains("KB"))
                    //    {
                    //        myNum += double.Parse(strVal.Replace("KB", "").Trim()) * 1024;
                    //    }
                    //    else if (strVal.Contains("MB"))
                    //    {
                    //        myNum += double.Parse(strVal.Replace("MB", "").Trim()) * 1024 * 1024;
                    //    }
                    //    else if (strVal.Contains("GB"))
                    //    {
                    //        myNum += double.Parse(strVal.Replace("GB", "").Trim()) * 1024 * 1024 * 1024;
                    //    }
                    //    else if (strVal.Contains("TB"))
                    //    {
                    //        myNum += double.Parse(strVal.Replace("TB", "").Trim()) * 1024 * 1024 * 1024 * 1024;
                    //    }
                    //    else if (strVal.Contains("PB"))
                    //    {
                    //        myNum += double.Parse(strVal.Replace("PB", "").Trim()) * 1024 * 1024 * 1024 * 1024 * 1024;
                    //    }
                    //    else
                    //    {
                    //        ;   //do nothing
                    //    }
                    //}
                    ////long mylong = (long) myNum / 2;
                    //double num = myNum / 2; //divided by 2 because each item count to two times.
                    //if (num >= ((long)1024 * 1024 * 1024 * 1024))
                    //    strByteTransfter = String.Format("{0:0.0000} TB", num / ((long)1024 * 1024 * 1024 * 1024));
                    //else if (num >= (1024 * 1024 * 1024))
                    //    strByteTransfter = String.Format("{0:0.0000} GB", num / (1024 * 1024 * 1024));
                    //else if (num >= (1024 * 1024))
                    //    strByteTransfter = String.Format("{0:0.0000} MB", num / (1024 * 1024));
                    //else if (num >= 1024)
                    //    strByteTransfter = String.Format("{0:0.0000} KB", num / 1024);
                    //else
                    //    strByteTransfter = String.Format("{0} bytes", num);

                    //jobStatus.ByteTransfer = strByteTransfter;

                    moduleStatus.Status = status;
                    moduleStatus.EndDateTime = DateTime.UtcNow.ToLocalTime();
                    TimeSpan? ts = moduleStatus.EndDateTime - moduleStatus.StartDateTime;
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
                    moduleStatus.TimeTaken = str;
                }

                //save modified entity using new Context
                using (var dbCtx = new ApplicationDbContext())
                {
                    //3. Mark entity as modified
                    dbCtx.Entry(moduleStatus).State = System.Data.Entity.EntityState.Modified;

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