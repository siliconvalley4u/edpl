using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Collections.Generic;
using DynamicMVC.Business.Attributes;

namespace EnterpriseDataPipeline.Models
{
    [DynamicEntity]
    [DynamicMenuItem("Module Status", "Installer Admin")]
    [Bind(Exclude = "Id")]
    public class ModuleStatus
    {
        
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [ScaffoldColumn(false)]
        public string Key { get; set; }

        [Required(ErrorMessage = "A Server Name is required")]
        [DisplayName("Server Name")]
        [StringLength(1024)]
        public string ServerName { get; set; }

        [Required(ErrorMessage = "An IP Address is required")]
        [DisplayName("IP Address")]
        [StringLength(1024)]
        public string IPAddress { get; set; }

        [StringLength(1024)]
        public string ModuleName { get; set; }

        public DateTime? StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }

        public string TimeTaken { get; set; }

        [StringLength(1024)]
        public string Status { get; set; }

    }
}