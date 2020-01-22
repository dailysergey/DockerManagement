using Docker.DotNet.Models;
using System;

namespace PortainerApi.Models
{
    public class MonitorPad
    {
        public string StackName { get; set; }
        public string ServiceName { get; set; }
        public DateTime ServiceCreatedAt { get; set; }
        public DateTime ServiceUpdatedAt { get; set; }
        public string ServiceID { get; set; }

        public bool isAlive;
        public DateTime containerCreatedAt;

        public string healthStatus;
        public string healthOutput;
        public DateTime lastHealthCheck;
        public TaskState taskState;
        public string ImageName { get; set; }

        public string ContainerID { get; set; }

        public int desiredAppCount;
        public int actualAppCount;
    }
}
