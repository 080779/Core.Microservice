﻿using Consul;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RestTemplateCore
{
    /// <summary>     
    ///  会自动到Consul中解析服务的Rest客户端，能把"http://ProductService/api/Product/" 这样的虚拟地址     
    ///  按照客户端负载均衡算法解析为 http://192.168.1.10:8080/api/Product/这样的真实 地址     
    ///  </summary>     
    public class RestTemplate
    {
        public String ConsulServerUrl { get; set; } = "http://127.0.0.1:8500";
        private HttpClient httpClient;

        public RestTemplate(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <summary>         
        /// 获取服务的第一个实现地址         
        /// </summary>         
        /// <param name="consulClient"></param>         
        /// <param name="serviceName"></param>         
        /// <returns></returns>
        private async Task<String> ResolveRootUrlAsync(String serviceName)
        {
            using (var consulClient = new ConsulClient(c => c.Address = new Uri(ConsulServerUrl)))
            {
                Dictionary<string, AgentService>.ValueCollection services = (await consulClient.Agent.Services()).Response.Values;
                List<AgentService> agentServices = new List<AgentService>();
                foreach(AgentService service in services)
                {
                    if(service.Service.Equals(serviceName,StringComparison.OrdinalIgnoreCase))
                    {
                        agentServices.Add(service);
                    }
                }
                if(agentServices.Count()<=0)
                {
                    throw new ArgumentException($"找不到服务{serviceName }的任何实例");
                }
                else
                {
                    //根据当前时钟毫秒数对可用服务个数取模，取出一台机器使用                    
                    var service = agentServices.ElementAt(Environment.TickCount % agentServices.Count());
                    return $"{service.Address}:{service.Port}";
                }
            }
        }

        //把 http://apiservice1/api/values 转换为 http://192.168.1.1:5000/api/values 
        private async Task<String> ResolveUrlAsync(String url)
        {
            Uri uri = new Uri(url);
            string serviceName = uri.Host;//apiservice1 
            string realRootUrl = await ResolveRootUrlAsync(serviceName);// 查 询 出 来 apiservice1 对应的服务器地址 192.168.1.1:5000

            //uri.Scheme=http,realRootUrl =192.168.1.1:5000,PathAndQuery=/api/values 
            return uri.Scheme + "://" + realRootUrl + uri.PathAndQuery;
        }

        /// <summary>
        /// 发出 Get 请求 
        /// </summary>
        /// <typeparam name="T">响应报文反序列类型</typeparam>
        /// <param name="url">请求路径</param>
        /// <param name="requestHeaders">请求额外的报文头信息</param>
        /// <returns></returns>
        public async Task<RestResponseWithBody<T>> GetForEntityAsync<T>(String url, HttpRequestHeaders requestHeaders = null)
        {
            using (HttpRequestMessage requestMsg = new HttpRequestMessage())
            {
                if (requestHeaders != null)
                {
                    foreach(var header in requestHeaders)
                    {
                        requestMsg.Headers.Add(header.Key, header.Value);
                    }
                }
                requestMsg.Method = HttpMethod.Get;
                //http://apiservice1/api/values 转换为 http://192.168.1.1:5000/api/values 
                requestMsg.RequestUri = new Uri(await ResolveUrlAsync(url));
                RestResponseWithBody<T> respEntity = await SendForEntityAsync<T>(requestMsg);
                return respEntity;
            }
        }

        public async Task<RestResponse> PostAsync(String url, object body = null, HttpRequestHeaders requestHeaders = null)
        {
            using (HttpRequestMessage requestMsg = new HttpRequestMessage())
            {
                if (requestHeaders != null)
                {
                    foreach (var header in requestHeaders)
                    {
                        requestMsg.Headers.Add(header.Key, header.Value);
                    }
                }
                requestMsg.Method = HttpMethod.Post;

                //http://apiservice1/api/values 转换为 http://192.168.1.1:5000/api/values 
                requestMsg.RequestUri = new Uri(await ResolveUrlAsync(url));
                requestMsg.Content = new StringContent(JsonConvert.SerializeObject(body));
                requestMsg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                RestResponse resp = await SendAsync(requestMsg);
                return resp;
            }
        }

        public async Task<RestResponseWithBody<T>> PostForEntityAsync<T>(String url, object body = null, HttpRequestHeaders requestHeaders = null)
        {
            using (HttpRequestMessage requestMsg = new HttpRequestMessage())
            {
                if(requestHeaders!=null)
                {
                    foreach (var header in requestHeaders)
                    {
                        requestMsg.Headers.Add(header.Key,header.Value);
                    }
                }
                requestMsg.Method = HttpMethod.Post;

                //http://apiservice1/api/values 转换为 http://192.168.1.1:5000/api/values 
                requestMsg.RequestUri = new Uri(await ResolveUrlAsync(url));
                requestMsg.Content = new StringContent(JsonConvert.SerializeObject(body));
                requestMsg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                RestResponseWithBody<T> respEntity = await SendForEntityAsync<T>(requestMsg);
                return respEntity;   
            }
        }

        public async Task<RestResponse> PutAsync(String url, object body = null, HttpRequestHeaders requestHeaders = null)
        {
            using (HttpRequestMessage requestMsg = new HttpRequestMessage())
            {
                if (requestHeaders != null)
                {
                    foreach (var header in requestHeaders)
                    {
                        requestMsg.Headers.Add(header.Key, header.Value);
                    }
                }
                requestMsg.Method = HttpMethod.Put;

                //http://apiservice1/api/values 转换为 http://192.168.1.1:5000/api/values 
                requestMsg.RequestUri = new Uri(await ResolveUrlAsync(url));
                requestMsg.Content = new StringContent(JsonConvert.SerializeObject(body));
                requestMsg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                RestResponse resp = await SendAsync(requestMsg);
                return resp;
            }
        }

        public async Task<RestResponseWithBody<T>> PutForEntityAsync<T>(String url, object body = null, HttpRequestHeaders requestHeaders = null)
        {
            using (HttpRequestMessage requestMsg = new HttpRequestMessage())
            {
                if (requestHeaders != null)
                {
                    foreach (var header in requestHeaders)
                    {
                        requestMsg.Headers.Add(header.Key, header.Value);
                    }
                }
                requestMsg.Method = HttpMethod.Put;

                //http://apiservice1/api/values 转换为 http://192.168.1.1:5000/api/values 
                requestMsg.RequestUri = new Uri(await ResolveUrlAsync(url));
                requestMsg.Content = new StringContent(JsonConvert.SerializeObject(body));
                requestMsg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                RestResponseWithBody<T> respEntity = await SendForEntityAsync<T>(requestMsg);
                return respEntity;
            }
        }

        public async Task<RestResponse> DeleteAsync(String url, HttpRequestHeaders requestHeaders = null)
        {
            using (HttpRequestMessage requestMsg = new HttpRequestMessage())
            {
                if (requestHeaders != null)
                {
                    foreach (var header in requestHeaders)
                    {
                        requestMsg.Headers.Add(header.Key, header.Value);
                    }
                }
                requestMsg.Method = HttpMethod.Delete;

                //http://apiservice1/api/values 转换为 http://192.168.1.1:5000/api/values 
                requestMsg.RequestUri = new Uri(await ResolveUrlAsync(url));
                RestResponse resp = await SendAsync(requestMsg);
                return resp;
            }
        }

        public async Task<RestResponseWithBody<T>> DeleteForEntityAsync<T>(String url, HttpRequestHeaders requestHeaders = null)
        {
            using (HttpRequestMessage requestMsg = new HttpRequestMessage())
            {
                if (requestHeaders != null)
                {
                    foreach (var header in requestHeaders)
                    {
                        requestMsg.Headers.Add(header.Key, header.Value);
                    }
                }
                requestMsg.Method = HttpMethod.Delete;

                //http://apiservice1/api/values 转换为 http://192.168.1.1:5000/api/values 
                requestMsg.RequestUri = new Uri(await ResolveUrlAsync(url));
                RestResponseWithBody<T> respEntity = await SendForEntityAsync<T>(requestMsg);
                return respEntity;
            }
        }

        public async Task<RestResponse> SendAsync(HttpRequestMessage requestMsg)
        {
            var result = await httpClient.SendAsync(requestMsg);
            RestResponse response = new RestResponse();
            response.StatusCode = result.StatusCode;
            response.Headers = result.Headers;
            return response;
        }

        public async Task<RestResponseWithBody<T>> SendForEntityAsync<T>(HttpRequestMessage requestMsg)
        {
            var result = await httpClient.SendAsync(requestMsg);
            RestResponseWithBody<T> respEntity = new RestResponseWithBody<T>();
            respEntity.StatusCode = result.StatusCode;
            respEntity.Headers = respEntity.Headers;
            string bodyStr = await result.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(bodyStr))
            {
                respEntity.Body = JsonConvert.DeserializeObject<T>(bodyStr);
            }
            return respEntity;
        }        
    }
}
