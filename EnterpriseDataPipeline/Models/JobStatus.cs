using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Collections.Generic;
using DynamicMVC.Business.Attributes;

namespace EnterpriseDataPipeline.Models
{
    [DynamicEntity]
    [DynamicMenuItem("Job Status", "Job Admin")]
    [DisplayName("Job Status")]
    [Bind(Exclude = "Id")]
    public class JobStatus
    {
        
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [ScaffoldColumn(false)]
        public string Key { get; set; }

        [Required(ErrorMessage = "A Source Name is required")]
        [StringLength(1024)]
        public string Source { get; set; }

        [Required(ErrorMessage = "A Source IP Name is required")]
        [StringLength(1024)]
        public string SourceIP { get; set; }

        [StringLength(1024)]
        public string SourceTable { get; set; }

        [Required(ErrorMessage = "A Destination Name is required")]
        [StringLength(1024)]
        public string Destination { get; set; }

        [Required(ErrorMessage = "A Destination IP Name is required")]
        [StringLength(1024)]
        public string DestinationIP { get; set; }

        [StringLength(1024)]
        public string DestinationTable { get; set; }

        [ScaffoldColumn(false)]
        [StringLength(1024)]
        public string JobName { get; set; }

        public DateTime? StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }

        public string ByteTransfer { get; set; }

        public string TimeTaken { get; set; }
        //[DisplayFormat(DataFormatString = "{0:hh\\:mm\\:ss}", ApplyFormatInEditMode = true)]
        //[DisplayFormat(DataFormatString = "{0:hh\\:mm\\:ss}")]
        //public TimeSpan? TimeTaken { get; set; }

        [StringLength(1024)]
        public string Status { get; set; }

        //[StringLength(40960)]
        //public string TransferLog { get; set; }

    }
}