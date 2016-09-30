//using System;
//using System.ComponentModel;
//using System.ComponentModel.DataAnnotations;
//using System.Web.Mvc;
//using System.Collections.Generic;
//using DynamicMVC.Business.Attributes;

//namespace EnterpriseDataPipeline.Models
//{
//    [DynamicEntity]
//    [DynamicMenuItem("Job Status Log", "Job Admin")]
//    [DisplayName("Job Status Log")]
//    [Bind(Exclude = "Id")]
//    public class JobStatusLog
//    {        
//        [ScaffoldColumn(false)]
//        public int Id { get; set; }

//        public int JobStatusId { get; set; }

//        [StringLength(40960)]
//        public string TransferLog { get; set; }
//    }
//}