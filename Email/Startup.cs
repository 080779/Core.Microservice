using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Email
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            string ip = Configuration["ip"];
            string strport = Configuration["port"];
            int port;
            int.TryParse(strport,out port);

            string serviceName = "EmailService";
            string serviceId = serviceName + Guid.NewGuid();

            using (var client = new ConsulClient(ConsulConfig))
            {
                client.Agent.ServiceRegister(new AgentServiceRegistration() {
                    ID = serviceId,
                    Name = serviceName,
                    Address = ip,
                    Port = port,
                    Check = new AgentServiceCheck()
                    {
                        DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),//服务停止多久后反注册（注销）
                        Interval = TimeSpan.FromSeconds(10),//健康检查时间间隔，或者称为心跳间隔
                        HTTP= $"http://{ip}:{port}/api/health",//健康检查地址
                        Timeout=TimeSpan.FromSeconds(5)
                    }
                }).Wait();//Consult 客户端的所有方法几乎都是异步方法，但是都没按照规范加上 Async 后缀，所以容易误导。记得调用后要 Wait()或者 await
            }

            //程序正常退出的时候从 Consul 注销服务  
            //要通过方法参数注入 IApplicationLifetime 
            applicationLifetime.ApplicationStopped.Register(()=> {
                using (var client =new ConsulClient(ConsulConfig))
                {
                    client.Agent.ServiceDeregister(serviceId).Wait();
                }
            });
        }

        private void ConsulConfig(ConsulClientConfiguration c)
        {
            c.Address = new Uri("http://127.0.0.1:8500");
            c.Datacenter = "dc1";
        }
    }
}
