using System;
using System.Collections.Generic;
using System.Text;

namespace PortainerApi
{
    public class Credentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class AuthSuccess
    {
        public string JWT { get; set; }
    }
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

    class ContainerState
    {
        public bool Dead { get; set; }
        public bool Paused { get; set; }
        public bool Running { get; set; }
        public bool Restarting { get; set; }
        public string Status { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime FinishedAt { get; set; }
    }
}
