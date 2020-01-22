using Docker.DotNet.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PortainerApi.Models.Container.ContainerState
{
    
    class Container
    {
        public string Id { get; set; }
        public string State { get; set; }
        public string Status { get; set; }

    }
    class ContainerInfo
    {
        public DateTime CreatedAt { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public ContainerState State { get; set; }
        public ContainerConfig Config { get; set; }
    }
    class ContainerConfig
    {
        public Dictionary<string,string> Labels { get; set; }

    }


    class ContainerStatus
    {
        [DataMember(Name = "ContainerID")]
        public string ContainerID { get; set; }
        [DataMember(Name = "ExitCode")]
        public int ExitCode { get; set; }
        [DataMember(Name = "PID")]
        public int PID { get; set; }
    }

    class ContainerState
    {
        public bool Dead { get; set; }
        public bool Paused { get; set; }
        public bool Running { get; set; }
        public bool Restarting { get; set; }
        public string Status { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime FinishedAt { get; set; }
        public ContainerHealth Health { get; set; }
    }
    class ContainerHealth
    {
        [DataMember(Name = "Status", EmitDefaultValue = false)]
        public string Status { get; set; }
        [DataMember(Name = "FailingStreak", EmitDefaultValue = false)]
        public long FailingStreak { get; set; }
        [DataMember(Name = "Log", EmitDefaultValue = false)]
        public IList<HealthcheckResult> Log { get; set; }
    }
    class ContainerHealthLog
    {

    }
}
