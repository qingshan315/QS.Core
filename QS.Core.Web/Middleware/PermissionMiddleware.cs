﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QS.Core.Web.Authorization;
using QS.ServiceLayer.Permission;
using System.Threading.Tasks;

namespace QS.Core.Web.Permission
{
    /// <summary>
    /// 权限中间件
    /// </summary>
    public class PermissionMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly IConfiguration _configuration;

        public PermissionMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        // IMyScopedService is injected into Invoke
        public async Task Invoke(HttpContext httpContext)
        {
            if (_configuration["Config:InitModule"] == "1")
            {
                var moduleService = httpContext.RequestServices.GetService<IModuleService>();
                var moduleManager = httpContext.RequestServices.GetService<IModuleManager>();

                await moduleService.CreateModules(moduleManager.GetModules());
            }
            await _next(httpContext);
        }
    }
}
