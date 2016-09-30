using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper;
using EnterpriseDataPipeline.Models;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;

namespace EnterpriseDataPipeline.Repo
{
    public class mydataservice
    {
        public IEnumerable Listdata()
        {
            IEnumerable list = null;
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["MYCON"].ToString()))
                {
                    string Query = @"SELECT
	                            pm.PlanName,
                                SUM(p.PaymentAmount) AS PaymentAmount
                                FROM PaymentDetails p 
                                INNER JOIN PlanMaster PM ON p.PlanID = PM.PlanID 
                                GROUP BY PM.PlanName";
                    // this is query which in in stored procedure


                    //var list = con.Query<Outputclass>("Usp_Getdata").AsEnumerable();
                    list = con.Query<Outputclass>("Usp_Getdata").AsEnumerable();
                }
            }
            catch(Exception ex)
            {
                string str = ex.Message;
            }

            // List of type Outputclass which it will return .
            return list;

        }
    }
}