using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using DynamicMVC.Business.Attributes;


namespace EnterpriseDataPipeline.Models
{
    [DynamicEntity]
    [DynamicMenuItem("PotentialCustomer", "Job Admin")]
    [Bind(Exclude = "Id")]
    public class PotentialCustomer
    {        
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone")]
        public string Phone { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Are you interested in Kids/youth program")]
        public bool LikeKidsYouthProgram { get; set; }

        [Display(Name = "Are you interested in IT training/placement")]
        public bool LikeITtrainingPlacement { get; set; }

        [Display(Name = "Are you interested in BigData/Iot/Cloud Market place")]
        public bool LikeBigDataIotCloudMarketPlace { get; set; }

        public DateTime? CreateDateTime { get; set; }

        public DateTime? UpdateDateTime { get; set; }
    }
}