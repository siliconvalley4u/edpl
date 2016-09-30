using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Collections.Generic;
using DynamicMVC.Business.Attributes;
using System.Data.Entity;

using System.ComponentModel.DataAnnotations.Schema;


namespace EnterpriseDataPipeline.Models
{
    [DynamicEntity]
    [DynamicMenuItem("Storage", "Job Admin")]
    [Bind(Exclude = "Id")]
    public class Storage
    {
        
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Required(ErrorMessage = "An Storage Name is required")]
        [DisplayName("Storage Name")]
        [StringLength(160)]
        public string Name { get; set; }

        [Required(ErrorMessage = "An Storage Type is required")]
        [DisplayName("Storage Type")]
        [StringLength(160)]
        [Index("IX_Name_StorageType", IsClustered = false, Order = 1)]
        public string Type { get; set; }

        [DisplayName("IP Address")]
        [StringLength(1024)]
        public string IPAddress { get; set; }

        [DisplayName("User Name")]
        [StringLength(1024)]
        public string UserName { get; set; }

        [DisplayName("Password")]
        [StringLength(1024)]
        public string Password { get; set; }

        [StringLength(160)]
        [DisplayName("Database Name")]
        public string DBName { get; set; }

        [StringLength(160)]
        [DisplayName("Table Name")]
        public string TableName { get; set; }

        [StringLength(160)]
        [DisplayName("Port")]
        public string Port { get; set; }
    }
}