using Docker.DotNet;
using Docker.DotNet.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DockerEngineApi
{
    class Program
    {
        static async Task Main()
        {
            Program p = new Program();
            using (DockerClient client = new DockerClientConfiguration(new Uri("http://192.168.1.30:2375")).CreateClient())
                await p.QueryPerform(client);
        }

        /// <summary>
        /// Simple way to get stack name, if you know all of them
        /// </summary>
        /// <param name="name">task name</param>
        /// <returns></returns>
        private string GetStackName(string name)
        {
            string[] stacks = new string[] { "stack1", "stack2"};
            foreach (var stack in stacks)
            {
                if (name.Contains(stack))
                    return stack;
            }
            return string.Empty;
        }

        /// <summary>
        /// Handmade metod to get count of services
        /// </summary>
        /// <returns></returns>
        private async Task<int> GetServicesCount(DockerClient client)
        {
            IEnumerable<SwarmService> services = await client.Swarm.ListServicesAsync();
            var servicesCount = 0;
            foreach(var service in services)
                servicesCount++;
            return servicesCount;
        }

        /// <summary>
        /// Testing project 
        /// Main Endpoint of application
        /// </summary>
        /// <returns>List of containers with their healthchecks</returns>
        async private Task<List<ContainerMonitor>> QueryPerform(DockerClient client)
        {
            try
            {
                var servicesCount = await GetServicesCount(client);
                Console.WriteLine("Entered into QueryPerform!");
                Console.WriteLine("SERVICES:" + servicesCount);
                List<ContainerMonitor> monitor = new List<ContainerMonitor>();

                IEnumerable<SwarmService> services = await client.Swarm.ListServicesAsync();
                IList<TaskResponse> tasks = await client.Tasks.ListAsync();
                bool isAlive = false;
                
                foreach (var service in services)
                {
                    ContainerMonitor monitor_service = new ContainerMonitor();
                    var workingTasks = 0;
                    foreach(var task in tasks)
                    {
                        foreach(var key in task.Labels.Keys)
                        {
                            Console.WriteLine("Label KEY AND VALUE:");
                            Console.WriteLine(key);
                            Console.WriteLine(task.Labels[key]);
                            
                        }
                        if (task.ServiceID == service.ID)
                        {
                            Console.WriteLine("==============");
                            Console.WriteLine("TASK ID: " + task.ID);
                            Console.WriteLine("TASK STATE: " + task.Status.State);
                            Console.WriteLine("TASK IMAGE: " + task.Spec.ContainerSpec.Image);
                            Console.WriteLine("==============");
                            if (task.Status.State == TaskState.Complete)
                            {
                                monitor_service.isAlive = true;
                                monitor_service.taskState = TaskState.Complete;
                            }
                            if (task.Status.State == TaskState.Running )
                            {
                                isAlive = true;
                                workingTasks++;
                                monitor_service.taskState = task.Status.State;
                                
                                if (task.Status.ContainerStatus != null)
                                {
                                    if (!string.IsNullOrEmpty(task.Status.ContainerStatus.ContainerID))
                                    {
                                        Console.WriteLine("============================");
                                        Console.WriteLine("ContainerStatus.ID : " + task.Status.ContainerStatus.ContainerID);
                                        Console.WriteLine("ContainerStatus.PID : " + task.Status.ContainerStatus.PID);
                                        Console.WriteLine($"{service.Spec.Name}");
                                        ContainerInspectResponse containerHealthInspect = null;
                                        try {
                                            containerHealthInspect = await client.Containers.InspectContainerAsync(task.Status.ContainerStatus.ContainerID);
                                            foreach (var label in containerHealthInspect.Config.Labels)
                                            {
                                                Console.WriteLine("Ключ");
                                                Console.WriteLine(label.Key);
                                                Console.WriteLine("Value");
                                                Console.WriteLine(label.Value);
                                            }
                                        }
                                        catch (DockerApiException e)
                                        {
                                            //Error can happen here if container isn't  on the same machine as leader node
                                            Console.WriteLine(e.Message);
                                            continue;
                                        }

                                        if (!string.IsNullOrEmpty(containerHealthInspect.State.Status))
                                        {
                                            Console.WriteLine("Status: " + containerHealthInspect.State.Status);
                                        }
                                        else
                                            Console.WriteLine("No STATUS");
                                        if (containerHealthInspect.State.Running)
                                        {
                                            if(containerHealthInspect.State != null)
                                            {
                                                var health = containerHealthInspect.State.Health;
                                                if (health != null)
                                                {
                                                    var healthCheckCount = containerHealthInspect.State.Health.Log.Count;
                                                    if (healthCheckCount > 0)
                                                    {
                                                        monitor_service.healthOutput = health.Log[healthCheckCount - 1].Output;
                                                        monitor_service.healthStatus = health.Status;
                                                        monitor_service.lastHealthCheck = health.Log[healthCheckCount - 1].End;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("ContainerStatus: EMPTY!");
                                }
                                Console.WriteLine("Err: " + task.Status.Err);
                                Console.WriteLine("Message: " + task.Status.Message);
                                if (task.Status.PortStatus != null)
                                {
                                    if (task.Status.PortStatus.Ports != null)
                                    {
                                        foreach (var port in task.Status.PortStatus.Ports)
                                        {
                                            Console.WriteLine("PortStatus: " + port.PublishedPort);
                                        }
                                    }
                                }
                                Console.WriteLine("State: " + task.Status.State);
                                Console.WriteLine("Timestamp: " + task.Status.Timestamp);
                            }
                        }
                    }
                    //If service is global, it means there will be 1 task
                    if (service.Spec.Mode.Global != null)
                    {
                        monitor_service.desiredAppCount = 1;
                    }
                    //If service is replicated, it means there will be count of replicas
                    if (service.Spec.Mode.Replicated != null)
                        monitor_service.desiredAppCount = service.Spec.Mode.Replicated.Replicas.Value;

                    monitor_service.actualAppCount = workingTasks;
                    monitor_service.createdAt = service.CreatedAt;
                    monitor_service.updatedAt = service.UpdatedAt;
                    monitor_service.isAlive = isAlive;
                    monitor_service.serviceName = service.Spec.Name;
                    monitor_service.stack = GetStackName(service.Spec.Name);
                    monitor.Add(monitor_service);

                    //Cleaned up
                    isAlive = false;
                    workingTasks = 0;
                }
                Console.WriteLine("SERVICES:" + servicesCount);
                foreach (var mon in monitor)
                {
                    Console.WriteLine(mon.createdAt);
                    Console.WriteLine(mon.updatedAt);
                    Console.WriteLine("Tasks: "+ mon.actualAppCount+"/"+ mon.desiredAppCount);
                    Console.WriteLine("Service name: "+mon.serviceName);
                    Console.WriteLine("Stack: "+mon.stack);
                    Console.WriteLine("Task STATE: " + mon.taskState);
                    if (!string.IsNullOrEmpty(mon.healthStatus))
                    {
                        Console.WriteLine("Health Status: " + mon.healthStatus);
                        Console.WriteLine("Health Output: " + mon.healthOutput);
                        Console.WriteLine("Is Working: " + mon.isAlive);
                        Console.WriteLine("Last Health Check: " + mon.lastHealthCheck);
                    }
                }
                return monitor;
            }
            catch(DockerApiException eApi)
            {
                Console.Write(eApi.Message);
                throw;
            }
            catch(TaskCanceledException eCancel)
            {
                Console.Write(eCancel.Message);
                throw;
            }
            catch(ArgumentNullException eNull)
            {
                Console.Write(eNull.Message);
                throw;
            }

        }
    }
}
