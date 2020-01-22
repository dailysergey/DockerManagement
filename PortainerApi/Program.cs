using Docker.DotNet.Models;
using Newtonsoft.Json;
using PortainerApi.Models;
using PortainerApi.Models.Auth;
using PortainerApi.Models.Node;
using PortainerApi.Models.Task;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace PortainerApi
{
    
    class Program
    {
        private string JWT;
        private HttpClient client;
        static async Task Main()
        {
            try
            {
                string hostAddress = "192.168.1.30";//"ip_ubuntu_machine";
                Program p = new Program();
                await p.QueryPerform(hostAddress);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }


        /// <summary>
        /// Authorization via Portainer API
        /// </summary>
        /// <returns></returns>
        async private void PortainerAuthAsync()
        {
            try
            {
                var model = new Credentials
                {
                    Username = "admin",
                    Password = "Aa123456"
                };
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var content = JsonConvert.SerializeObject(model);
                Console.WriteLine(content);
                HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");
                Uri portainerAddress = new Uri(client.BaseAddress + string.Format("api/auth"));
                Console.WriteLine(portainerAddress.ToString());
                var resp = client.PostAsync(portainerAddress, httpContent).Result;
                if (resp.IsSuccessStatusCode)
                {
                    Console.WriteLine("In Success Status Code");
                    var stream = await resp.Content.ReadAsStreamAsync();
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        using (var jsonResult = new JsonTextReader(sr))
                        {
                            JsonSerializer ser = new JsonSerializer();
                            JWT = ser.Deserialize<AuthSuccess>(jsonResult).JWT;
                        }
                    }
                }
                Console.WriteLine("NO Success. PortainerAuthAsync:"+resp.ReasonPhrase);
            }
            catch(HttpRequestException e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Getting list of containers 
        /// </summary>
        /// <returns></returns>
        //async private Task<List<Containers>> GetContainersAsync()
        //{
        //    try
        //    {
        //        client.DefaultRequestHeaders.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWT);
        //        var portainerEndpoint = new Uri(client.BaseAddress + "api/endpoints/1/docker/containers/json?all=1");
        //        var resp = await client.GetAsync(portainerEndpoint);
        //        if (resp.IsSuccessStatusCode)
        //        {
        //            var stream = await resp.Content.ReadAsStreamAsync();
        //            using (StreamReader sr = new StreamReader(stream))
        //            {
        //                using (var jsonResult = new JsonTextReader(sr))
        //                {
        //                    JsonSerializer ser = new JsonSerializer();
        //                    List<Container> containers = ser.Deserialize<List<Container>>(jsonResult);
        //                    Console.WriteLine("Количество контейнеров: " + containers.Count);
        //                    return containers;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            Console.WriteLine("No Success. GetContainersAsync: "+resp.ReasonPhrase);
        //            return null;
        //        }
                
        //    }
        //    catch(Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //        throw;
        //    }
        //}

        /// <summary>
        /// Getting list of nodes 
        /// </summary>
        /// <param name="jwt">Token for authorization</param>
        /// <returns></returns>
        async private Task<List<Node>> GetNodesAsync()
        {
            try
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWT);
                var portainerEndpoint = new Uri(client.BaseAddress + "api/endpoints/1/docker/nodes");
                var resp = await client.GetAsync(portainerEndpoint);
                if (resp.IsSuccessStatusCode)
                {
                    var bytes = await resp.Content.ReadAsByteArrayAsync();
                    return System.Text.Json.JsonSerializer.Deserialize<List<Node>>(bytes);
                }
                else
                {
                    Console.WriteLine("No Success. GetNodeAsync: " + resp.ReasonPhrase);
                    return null;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Getting list of services 
        /// </summary>
        /// <param name="jwt">Token for authorization</param>
        /// <returns></returns>
        async private Task<IEnumerable<SwarmService>> GetServicesAsync()
        {
            try
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWT);
                var portainerEndpoint = new Uri(client.BaseAddress + "api/endpoints/1/docker/services");
                var resp = await client.GetAsync(portainerEndpoint);
                if (resp.IsSuccessStatusCode)
                {
                    var stream = await resp.Content.ReadAsStreamAsync();
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        using (var jsonResult = new JsonTextReader(sr))
                        {
                            JsonSerializer ser = new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore };
                            return ser.Deserialize<IEnumerable<SwarmService>>(jsonResult);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No Success. GetServicesAsync: " + resp.ReasonPhrase);
                    return null;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Getting list of services 
        /// </summary>
        /// <param name="jwt">Token for authorization</param>
        /// <returns></returns>
        async private Task<List<DockerTask>> GetTasksAsync()
        {
            try
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWT);
                var portainerEndpoint = new Uri(client.BaseAddress + "api/endpoints/1/docker/tasks?filters=%7B%7D");
                var resp = await client.GetAsync(portainerEndpoint);
                if (resp.IsSuccessStatusCode)
                {
                    var bytes = await resp.Content.ReadAsByteArrayAsync();
                    List<DockerTask> taskJson = System.Text.Json.JsonSerializer.Deserialize<List<DockerTask>>( bytes);
                    return taskJson;
                }
                else
                {
                    Console.WriteLine("No Success. GetServicesAsync: " + resp.ReasonPhrase);
                    return null;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Getting specific tasks of service by serviceName
        /// </summary>
        /// <param name="serviceName">Service ID for searching</param>
        /// <returns></returns>
        async private Task<List<DockerTask>> GetTaskByServiceIDAsync(string serviceID)
        {
            try
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWT);
                var query = "api/endpoints/1/docker/tasks?filters={\"service\":[\"" + serviceID + "\"]}";
                var portainerEndpoint = new Uri(client.BaseAddress + query);
                var resp = await client.GetAsync(portainerEndpoint);
                if (resp.IsSuccessStatusCode)
                {
                    var bytes = await resp.Content.ReadAsByteArrayAsync();
                    List<DockerTask> taskJson = System.Text.Json.JsonSerializer.Deserialize<List<DockerTask>>(bytes);
                    return taskJson;
                }
                else
                {
                    Console.WriteLine("No Success. GetTaskByIDAsync: " + resp.ReasonPhrase);
                    return null;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Getting specific tasks of service by taskID
        /// </summary>
        /// <param name="taskID">taskId for searching</param>
        /// <returns></returns>
        async private Task<DockerTask> GetTaskByIDAsync(string taskID)
        {
            try
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWT);
                var query = $"api/endpoints/1/docker/tasks/{taskID}";
                var portainerEndpoint = new Uri(client.BaseAddress + query);
                var resp = await client.GetAsync(portainerEndpoint);
                if (resp.IsSuccessStatusCode)
                {
                    var bytes = await resp.Content.ReadAsByteArrayAsync();
                    return  System.Text.Json.JsonSerializer.Deserialize<DockerTask>(bytes);
                }
                else
                {
                    Console.WriteLine("No Success. GetTaskByIDAsync: " + resp.ReasonPhrase);
                    return null;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Get INFO about container by Container ID
        /// </summary>
        /// <param name="client">httpclient</param>
        /// <param name="id">Container ID</param>
        /// <param name="jwt">Token authorization</param>
        /// <returns></returns>
        async private Task<ContainerInspectResponse> GetContainerInfoByIDAsync(string id)
        {
            try
            {
                ContainerInspectResponse containerInfo = null;
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWT);
                var portainerEndpoint = new Uri(client.BaseAddress + $"api/endpoints/1/docker/containers/{id}/json");
                var resp = await client.GetAsync(portainerEndpoint);
                if (resp.IsSuccessStatusCode)
                {
                    Console.WriteLine("OK");
                    var stream = await resp.Content.ReadAsStreamAsync();
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        using (var jsonResult = new JsonTextReader(sr))
                        {
                            JsonSerializer ser = new JsonSerializer() { NullValueHandling=NullValueHandling.Ignore};
                            return ser.Deserialize<ContainerInspectResponse>(jsonResult);
                        }
                    }
                }
                else
                {
                    Console.WriteLine(resp.ReasonPhrase);
                    Console.WriteLine("No success. GetContainerInfoByIDAsync" + resp.ReasonPhrase);
                }
                return containerInfo;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Testing project 
        /// Main Endpoint of application
        /// <para>1. Authorize</para>
        /// <para>2. Get list of all working containers</para>
        /// <para>3. Foreach container inspect detailed healthcheck</para>
        /// </summary>
        /// <param name="hostAddress">Remote IP address</param>
        /// <returns>List of containers with their healthchecks</returns>
        async private Task<List<MonitorPad>> QueryPerform(string hostAddress)
        {
            try
            {
                using ( client = new HttpClient())
                {
                    client.BaseAddress = new Uri($"http://{hostAddress}:9000");
                    PortainerAuthAsync();
                    if (string.IsNullOrEmpty(JWT))
                        throw new Exception("UnAuthorized!");
                    var nodes = await GetNodesAsync();
                    List <MonitorPad> monitorStates = new List<MonitorPad>();
                    
                    var services = await GetServicesAsync();
                    foreach (var service in services)
                    {
                        MonitorPad monitorState = new MonitorPad();
                        monitorState.ServiceName = service.Spec.Name;
                        monitorState.ServiceCreatedAt = service.CreatedAt;
                        monitorState.ServiceID = service.ID;

                        var tasksByID = await GetTaskByServiceIDAsync(service.ID);
                        monitorState.desiredAppCount = tasksByID.Count > 0 ? tasksByID.Count : 0;
                        foreach (var task in tasksByID)
                        {
                            if (task.Status.State.Equals("running"))
                            {
                                monitorState.isAlive = true;
                                monitorState.actualAppCount++;
                                monitorState.StackName = nodes.Find(x => x.ID.Equals(task.NodeID)).Description.Hostname;
                                monitorState.ContainerID = task.Status.ContainerStatus.ContainerID;
                                var containerInfo = await GetContainerInfoByIDAsync(task.Status.ContainerStatus.ContainerID);
                                if (containerInfo != null)
                                {
                                    monitorState.containerCreatedAt = containerInfo.Created;
                                    monitorState.healthStatus = containerInfo.State.Status;
                                    var healthContainer = containerInfo.State.Health;
                                    if (healthContainer != null && healthContainer.Log.Count>0)
                                    {
                                        monitorState.healthOutput = containerInfo.State.Health.Log[healthContainer.Log.Count - 1].Output;
                                        monitorState.lastHealthCheck = containerInfo.State.Health.Log[healthContainer.Log.Count - 1].End;
                                    }

                                }
                                else
                                {
                                    var taskContainer = await GetTaskByIDAsync(task.ID);
                                    monitorState.containerCreatedAt = taskContainer.CreatedAt;
                                    monitorState.healthStatus = task.Status.State;
                                }
                            }
                        }
                        if (service.Spec.Labels != null && service.Spec.Labels.Count > 0 && service.Spec.Labels.ContainsKey("com.docker.stack.namespace"))
                        {
                            monitorState.StackName = service.Spec.Labels["com.docker.stack.namespace"];
                            if (service.Spec.Labels.ContainsKey("com.docker.stack.image"))
                                monitorState.ImageName = service.Spec.Labels["com.docker.stack.image"];
                        }
                        monitorStates.Add(monitorState);
                    }
                    return monitorStates;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}
