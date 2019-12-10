using Newtonsoft.Json;
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
        static async Task Main()
        {
            try
            {
                string hostAddress = "ip_ubuntu_machine";
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
        async private Task<string> PortainerAuthAsync(HttpClient client)
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
                var resp = await client.PostAsync(portainerAddress, httpContent);
                if (resp.IsSuccessStatusCode)
                {
                    Console.WriteLine("In Success Status Code");
                    var stream = await resp.Content.ReadAsStreamAsync();
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        using (var jsonResult = new JsonTextReader(sr))
                        {
                            JsonSerializer ser = new JsonSerializer();
                            var jwt = ser.Deserialize<AuthSuccess>(jsonResult).JWT;
                            Console.WriteLine(jwt);
                            return jwt;
                        }
                    }
                }
                Console.WriteLine("NO Success. PortainerAuthAsync:"+resp.ReasonPhrase);
                return null;
            }
            catch(HttpRequestException e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Getting list of containers 
        /// </summary>
        /// <param name="jwt">Token for authorization</param>
        /// <returns></returns>
        async private Task<List<Container>> GetContainersAsync(HttpClient client,string jwt)
        {
            try
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
                var portainerEndpoint = new Uri(client.BaseAddress + "api/endpoints/1/docker/containers/json?all=1");
                var resp = await client.GetAsync(portainerEndpoint);
                if (resp.IsSuccessStatusCode)
                {
                    var stream = await resp.Content.ReadAsStreamAsync();
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        using (var jsonResult = new JsonTextReader(sr))
                        {
                            JsonSerializer ser = new JsonSerializer();
                            List<Container> containers = ser.Deserialize<List<Container>>(jsonResult);
                            Console.WriteLine("Количество контейнеров: " + containers.Count);
                            return containers;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No Success. GetContainersAsync: "+resp.ReasonPhrase);
                    return null;
                }
                
            }
            catch(Exception e)
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
        async private Task<ContainerInfo> GetContainerInfoAsync(HttpClient client,string id,string jwt)
        {
            ContainerInfo containerInfo = null;
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
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
                        JsonSerializer ser = new JsonSerializer();
                        containerInfo = ser.Deserialize<ContainerInfo>(jsonResult);
                    }
                }
            }
            else
            {
                Console.WriteLine(resp.ReasonPhrase);
                Console.WriteLine("No success. GetContainerInfoAsync"+ resp.ReasonPhrase);
            }
            return containerInfo;
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
        async private Task<List<ContainerInfo>> QueryPerform(string hostAddress)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri($"http://{hostAddress}:9000");
                    var jwt = await PortainerAuthAsync(client);
                    if (string.IsNullOrEmpty(jwt))
                        throw new Exception("UnAuthorized!");
                    var containers = await GetContainersAsync(client, jwt);
                    List<ContainerInfo> states = null;
                    if (containers != null)
                    {
                        states = new List<ContainerInfo>();
                        foreach (var container in containers)
                        {
                            if (container.State == "running")
                            {
                                var containerInfo = await GetContainerInfoAsync(client, container.Id, jwt);
                                if (containerInfo != null)
                                {
                                    Console.WriteLine("======Container INFO====");
                                    Console.WriteLine("ID:  " + containerInfo.Id);
                                    Console.WriteLine("Name:  " + containerInfo.Name);
                                    Console.WriteLine("State.Dead:  " + containerInfo.State.Dead);
                                    Console.WriteLine("State.FinishedAt:  " + containerInfo.State.FinishedAt);
                                    Console.WriteLine("State.Paused:  " + containerInfo.State.Paused);
                                    Console.WriteLine("State.Restarting:  " + containerInfo.State.Restarting);
                                    Console.WriteLine("State.Running:  " + containerInfo.State.Running);
                                    Console.WriteLine("State.StartedAt:  " + containerInfo.State.StartedAt);
                                    Console.WriteLine("State.Status:  " + containerInfo.State.Status);
                                    Console.WriteLine("=========Labels:========");
                                    foreach(var item in containerInfo.Config.Labels)
                                    {
                                        Console.WriteLine("Key: " + item.Key);
                                        Console.WriteLine("Value: " + item.Value);
                                    }
                                    Console.WriteLine("DateCreated: " + containerInfo.CreatedAt);
                                    Console.WriteLine("============");
                                    states.Add(containerInfo);
                                }
                            }
                        }
                    }
                    return states;
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
