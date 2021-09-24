using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RevitService.Models
{
    public class KsDTO
    {
        [Required]
        public string urn { get; set; }

        [Required]
        public string downloadurn { get; set; }

        [Required]
        public DateTime start { get; set; }

        [Required]
        public DateTime end { get; set; }

        [Required]
        public string [] roles { get; set; }

        [Required]
        public string name { get; set; }

        [Required]
        public string hubId { get; set; }

        [Required]
        public string projectId { get; set; }

    }
}
