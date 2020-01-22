using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PortainerApi.Models.Services
{
    class Service
    {
        [DataMember(Name = "ID")]
        public string ID { get; set; }
        [DataMember(Name = "CreatedAt")]
        public DateTime CreatedAt { get; set; }
        [DataMember(Name = "UpdatedAt")]
        public DateTime UpdatedAt { get; set; }
        [DataMember(Name = "Spec")]
        public SpecService Spec { get; set; }
    }
    class SpecService
    {
        [DataMember(Name="Name")]
        public string Name { get; set; }
    }

}
