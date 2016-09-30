using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

using EnterpriseDataPipeline.Models;
using System;
using System.Linq;
using System.Web;

namespace EnterpriseDataPipeline.ViewModel
{
    public class ViewModelModuleVM
    {
        public List<ModuleTB> allModuleTB { get; set; }

        public List<ModuleServer> allModuleServer { get; set; }

        public List<KafkaServer> allKafkaServer { get; set; }

        public List<KafkaTopics> allKafkaTopics { get; set; }
    }
}