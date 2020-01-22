using Docker.DotNet.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PortainerApi.Models.Task
{
    class DockerTask
    {
        [DataMember(Name = "ID")]
        public string ID { get; set; }
        [DataMember(Name = "CreatedAt")]
        public DateTime CreatedAt { get; set; }
        [DataMember(Name = "UpdatedAt")]
        public DateTime UpdatedAt { get; set; }
        [DataMember(Name = "Status")]
        public StatusTask Status { get; set; }
        [DataMember(Name = "NodeID")]
        public string NodeID { get; set; }
        [DataMember(Name = "ServiceID")]
        public string ServiceID { get; set; }
    }
    class StatusTask
    {
        [DataMember(Name = "Message")]
        public string Message { get; set; }
        [DataMember(Name = "State")]
        public string State { get; set; }
        [DataMember(Name = "ContainerStatus")]
        public ContainerStatus ContainerStatus{get;set;}
    }

}
