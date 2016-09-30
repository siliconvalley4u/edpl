using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Threading;

using Renci.SshNet;
using System.Net.Http;

namespace SSHWrapper
{
    public class SSHCommandClient
    {
        private string clusterHost = "localhost";
        private string clusterPort = "50070";
        private string userName = "";
        private string password = "";

        private string sourceIP = "";
        private string sourceUserName = "";
        private string sourcePassword = "";
        private string sourceDatabase = "";
        private string sourceTable = "";

        private string destinationIP = "";
        private string destinationUserName = "";
        private string destinationPassword = "";
        private string destinationDatabase = "";
        private string destinationTable = "";


        private string jobName = "";

        public SSHCommandClient() { }

        public SSHCommandClient(string clusterHost)
        {
            this.clusterHost = clusterHost;
        }

        public SSHCommandClient(string clusterHost, string clusterPort)
        {
            this.clusterHost = clusterHost;
            this.clusterPort = clusterPort;
        }

        public string ClusterHost { get; set; }
        public string ClusterUserName { get; set; }
        public string ClusterPassword { get; set; }
        public string ClusterJobDirectory { get; set; }


        public string JobName { get; set; }

        public SSHCommandClient(string clusterHost, string userName, string password)
        {
            this.ClusterHost = clusterHost;
            this.ClusterPassword = password;
            this.ClusterUserName = userName;            
        }
        public SSHCommandClient(string clusterHost, string userName, string password, string jobName)
        {
            this.clusterHost = clusterHost;
            this.password = password;
            this.userName = userName;
            this.jobName = jobName;
        }
        public SSHCommandClient(string sourceIP, string sourceUserName, string sourcePassword, 
                                string sourceDatabase, string sourceTable,
                                string destinationIP, string destinationUserName, string destinationPassword, 
                                string destinationDatabase, string destinationTable, string jobName)
        {   //Source Parameters
            this.sourceIP = sourceIP;
            this.sourceUserName = sourceUserName;
            this.sourcePassword = sourcePassword;
            this.sourceDatabase = sourceDatabase;
            this.sourceTable = sourceTable;

            //Destination Parameters
            this.destinationIP = destinationIP;
            this.destinationUserName = destinationUserName;
            this.destinationPassword = destinationPassword;
            this.destinationDatabase = destinationDatabase;
            this.destinationTable = destinationTable;

            //Job Name
            this.jobName = jobName;
        }

        #region "Asyn_Command"
        /// <summary>
        /// Run command in remote server async. 
        /// </summary>
        /// <param name="path"></param>
        /// <returns>string</returns>
        // Three things to note in the signature: 
        //  - The method has an async modifier.  
        //  - The return type is Task or Task<T>. (See "Return Types" section.)
        //    Here, it is Task<string> because the return statement returns an string. 
        //  - The method name ends in "Async."
        public async Task<string> RunCmdInRemoteServerAsync()
        {
            string result = "";

            try
            {
                //string strJobName = JobName;
                string strJobDirectory = ClusterJobDirectory;
               // string strCmd = "java -jar /root/proj/" + jobName;
                string strCmd = "java -jar " + strJobDirectory + "/" + jobName;
                strCmd.Replace("//", "/");  //Added by Anthony Lai on 2015-08-04 to prevent use of "//" in directory path

                string parameter = " " + sourceIP + " " +
                    sourceUserName + " " +
                    sourcePassword + " " +
                    destinationIP + " " +
                    destinationDatabase + " " +
                    destinationUserName + " " +
                    destinationPassword + " " +
                    destinationTable;

                ConnectionInfo connectionInfo = new PasswordConnectionInfo(ClusterHost, ClusterUserName, ClusterPassword);
                using (SshClient client = new SshClient(connectionInfo))
                {
                    if (!client.IsConnected)
                        client.Connect();

                    if (client.IsConnected)
                    {
                        SshCommand cmd = client.CreateCommand(strCmd + parameter); 
                        var myTask = Task.Factory.StartNew(() => cmd.Execute());
                        result = await myTask;
                        result += cmd.Error;
                        client.Disconnect();
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            finally
            {

            }

            return result;
        }
        public async Task<string> RunCmdInRemoteServerAsyncSpark(string query)
        {
            string result = "";

            try
            {
                string strCmd = "sudo -u hdfs hive -e ";
                strCmd.Replace("//", "/");  //Added by Anthony Lai on 2015-08-04 to prevent use of "//" in directory path

                strCmd = strCmd + "\"" + query + "\"";

                ConnectionInfo connectionInfo = new PasswordConnectionInfo(ClusterHost, ClusterUserName, ClusterPassword);
                using (SshClient client = new SshClient(connectionInfo))
                {
                    if (!client.IsConnected)
                        client.Connect();

                    if (client.IsConnected)
                    {
                        SshCommand cmd = client.CreateCommand(strCmd);
                        var myTask = Task.Factory.StartNew(() => cmd.Execute());
                        result = await myTask;
                        result += cmd.Error;
                        client.Disconnect();
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            finally
            {

            }

            return result;
        }
        public async Task<string> RunCmdInRemoteServerAsyncMKDir()
        {
            string result = "";

            try
            {
                string clusterHost = "142.0.252.92";
                string UserName = "hdfs";
                string password = "swatcloud";
                //string strJobName = "sqlToHDFS.jar";
                string strJobName = JobName;

                string strCmd = "mkdir /tmp/tmpproj";

                ConnectionInfo connectionInfo = new PasswordConnectionInfo(clusterHost, UserName, password);
                using (SshClient client = new SshClient(connectionInfo))
                {
                    if (!client.IsConnected)
                        client.Connect();

                    if (client.IsConnected)
                    {
                        SshCommand cmd = client.CreateCommand(strCmd);
                        var myTask = Task.Factory.StartNew(() => cmd.Execute());
                        result = await myTask;
                        result += cmd.Error;
                        client.Disconnect();
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            finally
            {

            }

            return result;
        }
    }

    #endregion "Asyn_Command"
}
