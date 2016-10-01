using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using DynamicMVC.Business.Attributes;

namespace EnterpriseDataPipeline.Models
{
    [DynamicEntity]
    [DynamicMenuItem("Module", "Installer Admin")]
    [Bind(Exclude = "Id")]
    public class ModuleTB
    {
        
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Required(ErrorMessage = "A Module Name is required")]
        [DisplayName("Module Name")]
        [StringLength(1024)]
        public string Name { get; set; }

        [DisplayName("Order Id")]
        public int OrderId { get; set; }

    }
}