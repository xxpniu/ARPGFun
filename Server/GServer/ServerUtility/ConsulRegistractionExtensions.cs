using System;

namespace ServerUtility
{
    using Consul;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public static class ConsulRegistrationExtensions
    {
        public static IServiceCollection AddConsulConfig(this IServiceCollection services, string serviceName, string serviceAddress, int servicePort)
        {
            // 1. 注册 Consul 客户端
            services.AddSingleton<IConsulClient>(p => new ConsulClient(cfg =>
            {
                cfg.Address = new Uri($"{serviceAddress}:{servicePort}"); // Docker 映射出来的地址
            }));

            return services;
        }

        public static IApplicationBuilder UseConsul(this IApplicationBuilder app, IHostApplicationLifetime lifetime, string serviceName, string serviceAddress, int servicePort)
        {
            var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
            var serviceId = $"{serviceName}-{Guid.NewGuid()}";

            var registration = new AgentServiceRegistration()
            {
                ID = serviceId,
                Name = serviceName,
                Address = serviceAddress,
                Port = servicePort,
                Check = new AgentServiceCheck()
                {
                    // 关键：告诉 Consul 通过 gRPC 协议检查本服务的健康状况
                    GRPC = $"{serviceAddress}:{servicePort}",
                    GRPCUseTLS = false,
                    Interval = TimeSpan.FromSeconds(10),
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(30)
                }
            };

            // 程序启动时注册
            lifetime.ApplicationStarted.Register(() =>
            {
                consulClient.Agent.ServiceRegister(registration).Wait();
            });

            // 程序停止时注销
            lifetime.ApplicationStopped.Register(() =>
            {
                consulClient.Agent.ServiceDeregister(serviceId).Wait();
            });

            return app;
        }
    }
}