using Docker.DotNet.Models;
using System;
using System.Collections.Generic;

namespace PortainerApi.Models
{
    public class MonitorPad
    {
        #region Service INFO
        public string StackName;
        public string ServiceName;
        public DateTime ServiceCreatedAt;
        public DateTime ServiceUpdatedAt;
        public string ServiceID;
        public IList<PortConfig> ServicePort;
        public ServiceMode ServiceType;
        #endregion
        #region Container INFO
        public bool isAlive;
        public DateTime containerCreatedAt;
        public string healthStatus;
        public string healthOutput;
        public DateTime lastHealthCheck;
        public string ContainerID;
        public string ImageName;
        #endregion

        #region taskINFO
        public TaskState taskState;
        public string taskID;
        public string taskNode;
        #endregion

        public int desiredAppCount;
        public int actualAppCount;
    }
}
