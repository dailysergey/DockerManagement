using Docker.DotNet.Models;
using Newtonsoft.Json;
using PortainerApi.Models;
using PortainerApi.Models.Auth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PortainerApi
{

    class Program
    {

        static async Task Main()
        {
            try
            {
                string hostAddress = "192.168.1.27";
                PortainerApi p = new PortainerApi(hostAddress);
                await p.ExecuteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
    public class PortainerApi
    {
        private string JWT;
        private HttpClient client;
        private string hostAddress;
        private string login = "admin";
        private string password = "Aa123456";
        public PortainerApi(string _hostAddress)
        {
            hostAddress = _hostAddress;
            client = new HttpClient();
            var url = new UriBuilder(hostAddress);
            url.Port = 9000;
            client.BaseAddress =url.Uri;
            PortainerAuthAsync();
        }

        /// <summary>
        /// Testing project 
        /// Main Endpoint of application
        /// <para>1. Получаем список севрисов</para>
        /// <para>2. Для каждого сервиса определяем количество и статусы запущенных задач</para>
        /// <para>3. Для каждой запущенной задачи инспектируем контейнер</para>
        /// </summary>
        /// <param name="hostAddress">Remote IP address</param>
        /// <returns>IEnumerable of MonitorPad with their healthchecks</returns>
        async public Task<IEnumerable<MonitorPad>> ExecuteAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(JWT))
                {
                    Console.WriteLine("JWT токен отсутствует!");
                    throw new Exception("UnAuthorized!");
                }
                if (string.IsNullOrEmpty(hostAddress))
                {
                    Console.WriteLine("Host Address отсутствует!");
                    throw new Exception("HostAddress is empty!");
                }

                List<MonitorPad> monitorStates = new List<MonitorPad>();

                //Получаем все сервисы
                var services = await GetServicesAsync();
                foreach (var service in services)
                {
                    //Заполняем информацию об одном сервисе
                    MonitorPad monitorState = new MonitorPad
                    {

                        ServiceName = service.Spec.Name,
                        ServiceCreatedAt = service.CreatedAt,
                        ServiceUpdatedAt = service.UpdatedAt,
                        ServiceID = service.ID
                    };
                    //Получаем информацию о задачах, которые обрабатывают этот сервис
                    var tasksByID = await GetTaskByServiceIDAsync(service.ID);
                    if (service.Endpoint.Ports != null && service.Endpoint.Ports.Count > 0)
                    {
                        monitorState.ServicePort = service.Endpoint.Ports;
                    }
                    foreach (var task in tasksByID)
                    {
                        if (task.Status.State == TaskState.Running || task.Status.State == TaskState.Complete)
                        {
                            monitorState.taskID = task.ID;
                            monitorState.isAlive = true;
                            monitorState.actualAppCount++;
                            monitorState.ContainerID = task.Status.ContainerStatus.ContainerID;

                            var node = await GetNodeByIDAsync(task.NodeID);
                            monitorState.taskNode = node.Description.Hostname;

                            monitorState.taskState = task.Status.State;
                            if(service.Spec != null && service.Spec.TaskTemplate !=null && service.Spec.TaskTemplate.Placement != null && service.Spec.TaskTemplate.Placement.Constraints != null)
                            monitorState.desiredAppCount += service.Spec.TaskTemplate.Placement.Constraints.Count;

                            //Выставим в качестве времени создания контейнера - время создания задачи
                            monitorState.containerCreatedAt = task.CreatedAt;

                            //Получаем информацию о контейнере рассматриваемой задачи
                            var containerInfo = await GetContainerInfoByIDAsync(task.Status.ContainerStatus.ContainerID);
                            if (containerInfo != null)
                            {
                                monitorState.containerCreatedAt = containerInfo.Created;
                                monitorState.healthStatus = containerInfo.State.Status;

                                //Проверяем присутствие HealthCheck-функционала
                                var healthContainer = containerInfo.State.Health;
                                if (healthContainer != null && healthContainer.Log.Count > 0)
                                {
                                    monitorState.healthOutput = containerInfo.State.Health.Log[healthContainer.Log.Count - 1].Output;
                                    monitorState.lastHealthCheck = containerInfo.State.Health.Log[healthContainer.Log.Count - 1].End;
                                }
                            }
                        }
                        //Проверяем наличие Имени Stack-а и имени образа данного контейнера
                        if (service.Spec.Labels != null && service.Spec.Labels.Count > 0 && service.Spec.Labels.ContainsKey("com.docker.stack.namespace"))
                        {
                            monitorState.StackName = service.Spec.Labels["com.docker.stack.namespace"];
                            if (service.Spec.Labels.ContainsKey("com.docker.stack.image"))
                                monitorState.ImageName = service.Spec.Labels["com.docker.stack.image"];
                            else
                                monitorState.ImageName = "Не указан";
                        }
                    }
                    monitorStates.Add(monitorState);
                }
                return monitorStates;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
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
                    Username = login,
                    Password = password
                };
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var content = JsonConvert.SerializeObject(model);
                HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");
                Uri portainerAddress = new Uri(client.BaseAddress + string.Format("api/auth"));
                var resp = client.PostAsync(portainerAddress, httpContent).Result;
                if (resp.IsSuccessStatusCode)
                {
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
            }
            catch (HttpRequestException e)
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
        /// Getting list of nodes 
        /// </summary>
        /// <returns></returns>
        async private Task<IEnumerable<NodeListResponse>> GetNodesAsync()
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
                    return System.Text.Json.JsonSerializer.Deserialize<List<NodeListResponse>>(bytes);
                }
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Getting node by ID
        /// </summary>
        /// <returns></returns>
        async private Task<NodeListResponse> GetNodeByIDAsync(string nodeID)
        {
            try
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWT);
                var portainerEndpoint = new Uri(client.BaseAddress + $"api/endpoints/1/docker/nodes/{nodeID}");
                var resp = await client.GetAsync(portainerEndpoint);
                if (resp.IsSuccessStatusCode)
                {
                    var bytes = await resp.Content.ReadAsByteArrayAsync();
                    return System.Text.Json.JsonSerializer.Deserialize<NodeListResponse>(bytes);
                }
                return null;
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
                return null;
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
        async private Task<IEnumerable<TaskResponse>> GetTaskByServiceIDAsync(string serviceID)
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
                    var stream = await resp.Content.ReadAsStreamAsync();
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        using (var jsonResult = new JsonTextReader(sr))
                        {
                            JsonSerializer ser = new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore };
                            return ser.Deserialize<IEnumerable<TaskResponse>>(jsonResult);
                        }
                    }
                }
                return null;
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
        async private Task<TaskResponse> GetTaskByIDAsync(string taskID)
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
                    var stream = await resp.Content.ReadAsStreamAsync();
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        using (var jsonResult = new JsonTextReader(sr))
                        {
                            JsonSerializer ser = new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore };
                            return ser.Deserialize<TaskResponse>(jsonResult);
                        }
                    }
                }
                return null;
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
                    var stream = await resp.Content.ReadAsStreamAsync();
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        using (var jsonResult = new JsonTextReader(sr))
                        {
                            JsonSerializer ser = new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore };
                            return ser.Deserialize<ContainerInspectResponse>(jsonResult);
                        }
                    }
                }
                return containerInfo;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

    }
}
