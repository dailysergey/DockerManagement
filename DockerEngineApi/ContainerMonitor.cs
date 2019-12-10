using Docker.DotNet.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DockerEngineApi
{
    class ContainerMonitor
    {
        public string stack;
        
        public string serviceName;
        
        public bool isAlive;
        public DateTime createdAt;
        public DateTime updatedAt;

        public string healthStatus;
        public string healthOutput;
        public DateTime lastHealthCheck;
        public TaskState taskState;

        public ulong desiredAppCount;
        public int actualAppCount;
    }
}
