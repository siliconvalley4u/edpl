using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
//using DynamicMVC.Business.Attributes;
using DynamicMVC.Annotations;

namespace EnterpriseDataPipeline.Models
{
    [DynamicEntity]
    [DynamicMenuItem("Kafka Topics", "Kafka Admin")]
    [Bind(Exclude = "Id")]
    public class KafkaTopics
    {
        
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [DisplayName("Topics")]
        [StringLength(1024)]
        public string Topics { get; set; }
    }
}