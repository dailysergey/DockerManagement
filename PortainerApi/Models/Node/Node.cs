using System;
using System.Runtime.Serialization;

namespace PortainerApi.Models.Node
{
    class Node
    {
        [DataMember(Name = "ID")]
        public string ID { get; set; }
        [DataMember(Name = "CreatedAt")]
        public DateTime CreatedAt { get; set; }
        [DataMember(Name = "UpdatedAt")]
        public DateTime UpdatedAt { get; set; }
        [DataMember(Name = "Description")]
        public DescriptionNode Description { get; set; }
        [DataMember(Name = "Status")]
        public StatusNode Status { get; set; }
        [DataMember(Name = "Spec")]
        public SpecNode Spec { get; set; }
    }
    class SpecNode
    {
        [DataMember(Name = "Role")]
        public string Role { get; set; }
        [DataMember(Name = "Availability")]
        public string Availability { get; set; }
    }
    class StatusNode
    {
        [DataMember(Name = "State")]
        public string State { get; set; }
        [DataMember(Name = "Addr")]
        public string Addr { get; set; }
    }

    class DescriptionNode
    {
        [DataMember(Name = "Hostname")]
        public string Hostname { get; set; }
    }
}
