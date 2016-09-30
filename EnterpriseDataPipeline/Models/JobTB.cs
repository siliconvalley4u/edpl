using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Collections.Generic;
using DynamicMVC.Business.Attributes;

namespace EnterpriseDataPipeline.Models
{
    [DynamicEntity]
    [DynamicMenuItem("Job", "Job Admin")]    
    [Bind(Exclude = "Id")]
    public class JobTB
    {
        
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Required(ErrorMessage = "An Storage Name is required")]
        [StringLength(1024)]
        public string Source { get; set; }

        [Required(ErrorMessage = "An Storage Name is required")]
        [StringLength(1024)]
        public string Destinatiion { get; set; }

        [DisplayName("Job Name")]
        [StringLength(1024)]
        public string JobName { get; set; }
    }
}