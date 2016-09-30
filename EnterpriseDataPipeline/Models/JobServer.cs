using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Collections.Generic;
using DynamicMVC.Business.Attributes;
using System.Data.Entity;

namespace EnterpriseDataPipeline.Models
{
    [DynamicEntity]
    [DynamicMenuItem("Job Server", "Job Admin")]
    [Bind(Exclude = "Id")]
    public class JobServer
    {
        
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Required(ErrorMessage = "A Server Name is required")]
        [DisplayName("Server Name")]
        [StringLength(1024)]
        public string Name { get; set; }

        [Required(ErrorMessage = "An IP Address is required")]
        [DisplayName("IP Address")]
        [StringLength(160)]
        public string IPAddress { get; set; }

        [DisplayName("User Name")]
        [StringLength(1024)]
        public string UserName { get; set; }

        [DisplayName("Password")]
        [StringLength(1024)]
        public string Password { get; set; }

        [DisplayName("Job Directory")]
        [StringLength(1024)]
        public string JobDirectory { get; set; }
    }
}