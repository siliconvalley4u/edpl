﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
//using DynamicMVC.Business.Attributes;
using DynamicMVC.Annotations;

namespace EnterpriseDataPipeline.Models
{
    [DynamicEntity]
    [DynamicMenuItem("Puppet Server", "Installer Admin")]
    [Bind(Exclude = "Id")]
    public class PuppetServer
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

        [DisplayName("Server Type")]
        [StringLength(1024)]
        public string ServerType { get; set; }

        [DisplayName("PEM File")]
        [StringLength(1024)]
        public string PemFile { get; set; }

        [DisplayName("Job Location")]
        [StringLength(1024)]
        public string JobLocation { get; set; }
    }
}