using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using System.Collections.ObjectModel;
using System.Web.Mvc;

namespace EnterpriseDataPipeline.ViewModel
{
    public class StorageViewModel
    {        
        public int SourceId { get; set; }
        public int DestinationId { get; set; }

        [Required]
        public SelectList Storage { get; set; }

        [Required]
        public string DBConnectionError { get; set; }

    }
}
