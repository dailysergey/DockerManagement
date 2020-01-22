﻿using System;
using System.Collections.Generic;
using System.Text;

namespace PortainerApi.Models
{
    class MonitorPad
    {
        public string StackName { get; set; }
        public string ServiceName { get; set; }
        public DateTime ServiceCreatedAt { get; set; }
        public string ServiceID { get; set; }

        public bool isAlive;
        public DateTime containerCreatedAt;

        public string healthStatus;
        public string healthOutput;
        public DateTime lastHealthCheck;
        public string ImageName { get; set; }

        public string ContainerID { get; set; }

        public int desiredAppCount;
        public int actualAppCount;
    }
}