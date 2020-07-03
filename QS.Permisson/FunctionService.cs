﻿// -----------------------------------------------------------------------
//  <copyright file="FunctionService.cs" company="OSharp开源团队">
//      Copyright (c) 2014-2018 OSharp. All rights reserved.
//  </copyright>
//  <site>http://www.osharp.org</site>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2018-06-23 17:23</last-date>
// -----------------------------------------------------------------------

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QS.Core.Attributes;
using QS.Core.Data.Enums;
using QS.Core.Dependency;
using QS.Core.Extensions;
using QS.Core.Reflection;
using QS.DataLayer.Entities;
using QS.Permission.Modules;
using QS.ServiceLayer.Permission;
using QS.ServiceLayer.Permission.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace QS.Permission
{
    /// <summary>
    /// 方法服务
    /// </summary>
    public class FunctionService:IFunctionService,IScopeDependency
    {
        private readonly List<Function> _functions = new List<Function>();

        private readonly IPermissionService _permissionService;
        private readonly IAssemblyFinder _assemblyFinder;

        private readonly MethodInfo[] _methods;
        public FunctionService(IPermissionService permissionService, IAssemblyFinder assemblyFinder)
        {
            _permissionService = permissionService;
            _assemblyFinder = assemblyFinder;
            _methods = _assemblyFinder.Find(o => o.FullName.Contains("QS.Core.Web")).FirstOrDefault()
                    .GetTypes().SelectMany(m => m.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    .Where(type => type.HasAttribute<ModuleInfoAttribute>()).ToArray();
        }

        /// <summary>
        /// 判断权限值是否被重复使用
        /// </summary>
        public void ValidPermissions()
        {
            var codes = Enum.GetValues(typeof(PermCode)).Cast<int>();
            var dic = new Dictionary<int, int>();
            foreach (var code in codes)
            {
                if (!dic.ContainsKey(code))
                    dic.Add(code, 1);
                else
                    throw new Exception($"权限值 {code} 被重复使用，请检查 PermCode 的定义");
            }
        }

        #region 方法

        public Function[] PickupFunctions()
        {
            var types = _assemblyFinder.Find(o => o.FullName.Contains("QS.Core.Web")).FirstOrDefault()
                    .GetTypes().Where(m => m.HasAttribute<ControllerAttribute>()).ToArray();
            return GetFunctions(types);
        }

        /// <summary>
        /// 从功能类型中获取功能信息
        /// </summary>
        /// <param name="functionTypes">功能类型集合</param>
        /// <returns></returns>
        protected Function[] GetFunctions(Type[] functionTypes)
        {
            List<Function> functions = new List<Function>();
            foreach (Type type in functionTypes.OrderBy(m => m.FullName))
            {
                Function controller = GetFunction(type);
                if (controller == null || type.HasAttribute<NonFunctionAttribute>())
                {
                    continue;
                }

                if (!HasPickup(functions, controller))
                {
                    functions.Add(controller);
                }

                List<MethodInfo> methods = _methods.ToList();
                // 移除已被重写的方法
                MethodInfo[] overriddenMethodInfos = methods.Where(m => m.IsOverridden()).ToArray();
                foreach (MethodInfo overriddenMethodInfo in overriddenMethodInfos)
                {
                    methods.RemoveAll(m => m.Name == overriddenMethodInfo.Name && m != overriddenMethodInfo);
                }

                foreach (MethodInfo method in methods)
                {
                    Function action = GetFunction(controller, method);
                    if (action == null)
                    {
                        continue;
                    }

                    if (IsIgnoreMethod(action, method, functions))
                    {
                        continue;
                    }

                    if (HasPickup(functions, action))
                    {
                        continue;
                    }

                    functions.Add(action);
                }
            }

            return functions.ToArray();
        }
        /// <summary>
        /// 从功能类型创建功能信息
        /// </summary>
        /// <param name="controllerType">功能类型</param>
        /// <returns></returns>
        protected Function GetFunction(Type controllerType)
        {
            if (!controllerType.IsController())
            {
                throw new Exception($"类型“{controllerType.FullName}”不是MVC控制器类型");
            }
            FunctionAccessType accessType = controllerType.HasAttribute<LoggedInAttribute>()
                ? FunctionAccessType.LoggedIn
                : controllerType.HasAttribute<RoleLimitAttribute>()
                    ? FunctionAccessType.RoleLimit
                    : FunctionAccessType.Anonymous;
            Function function = new Function()
            {
                Name = controllerType.GetDescription(),
                Area = GetArea(controllerType),
                Controller = controllerType.Name.Replace("ControllerBase", string.Empty).Replace("Controller", string.Empty),
                IsController = true,
                AccessType = accessType
            };
            return function;
        }

        /// <summary>
        /// 实现从方法信息中创建功能信息
        /// </summary>
        /// <param name="typeFunction">类功能信息</param>
        /// <param name="method">方法信息</param>
        /// <returns></returns>
        protected Function GetFunction(Function typeFunction, MethodInfo method)
        {
            FunctionAccessType accessType = method.HasAttribute<LoggedInAttribute>()
                ? FunctionAccessType.LoggedIn
                : method.HasAttribute<AllowAnonymousAttribute>()
                    ? FunctionAccessType.Anonymous
                    : method.HasAttribute<RoleLimitAttribute>()
                        ? FunctionAccessType.RoleLimit
                        : typeFunction.AccessType;
            Function function = new Function()
            {
                Name = $"{typeFunction.Name}-{method.GetDescription()}",
                Area = typeFunction.Area,
                Controller = typeFunction.Controller,
                Action = method.Name,
                AccessType = accessType,
                IsController = false,
                IsAjax = true
            };
            return function;
        }

        /// <summary>
        /// 重写以实现是否忽略指定方法的功能信息
        /// </summary>
        /// <param name="action">要判断的功能信息</param>
        /// <param name="method">功能相关的方法信息</param>
        /// <param name="functions">已存在的功能信息集合</param>
        /// <returns></returns>
        protected bool IsIgnoreMethod(Function action, MethodInfo method, IEnumerable<Function> functions)
        {
            if (method.HasAttribute<NonActionAttribute>() || method.HasAttribute<NonFunctionAttribute>())
            {
                return true;
            }

            Function existing = GetFunction(functions, action.Area, action.Controller, action.Action, action.Name);
            return existing != null && method.HasAttribute<HttpPostAttribute>();
        }
        /// <summary>
        /// 重写以实现功能信息查找
        /// </summary>
        /// <param name="functions">功能信息集合</param>
        /// <param name="area">区域名称</param>
        /// <param name="controller">类型名称</param>
        /// <param name="action">方法名称</param>
        /// <param name="name">功能名称</param>
        /// <returns></returns>
        protected Function GetFunction(IEnumerable<Function> functions, string area, string controller, string action, string name)
        {
            return functions.FirstOrDefault(m =>
                string.Equals(m.Area, area, StringComparison.OrdinalIgnoreCase)
                && string.Equals(m.Controller, controller, StringComparison.OrdinalIgnoreCase)
                && string.Equals(m.Action, action, StringComparison.OrdinalIgnoreCase)
                && string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
        }
        /// <summary>
        /// 从类型中获取功能的区域信息
        /// </summary>
        private static string GetArea(MemberInfo type)
        {
            AreaAttribute attribute = type.GetAttribute<AreaAttribute>();
            return attribute?.RouteValue;
        }

        /// <summary>
        /// 重写以判断指定功能信息是否已提取过
        /// </summary>
        /// <param name="functions">已提取功能信息集合</param>
        /// <param name="function">要判断的功能信息</param>
        /// <returns></returns>
        protected bool HasPickup(List<Function> functions, Function function)
        {
            return functions.Any(m =>
                string.Equals(m.Area, function.Area, StringComparison.OrdinalIgnoreCase)
                && string.Equals(m.Controller, function.Controller, StringComparison.OrdinalIgnoreCase)
                && string.Equals(m.Action, function.Action, StringComparison.OrdinalIgnoreCase)
                && string.Equals(m.Name, function.Name, StringComparison.OrdinalIgnoreCase));
        }

        #endregion


        /// <summary>
        /// 从程序集中获取模块信息
        /// </summary>
        public ModuleInfo[] Pickup()
        {
            Type[] moduleTypes = _assemblyFinder.Find(o => o.FullName.Contains("QS.Core.Web")).FirstOrDefault().GetTypes()
                .Where(type => type.HasAttribute<ModuleInfoAttribute>()).ToArray();
            ModuleInfo[] modules = GetModules(moduleTypes);
            return modules;
        }

        public ModuleInfo[] GetModules(Type[] moduleTypes)
        {
            List<ModuleInfo> infos = new List<ModuleInfo>();
            foreach (Type moduleType in moduleTypes)
            {
                string[] existPaths = infos.Select(m => $"{m.Position}.{m.Code}").ToArray();
                ModuleInfo[] typeInfos = GetModules(moduleType, existPaths);
                foreach (ModuleInfo info in typeInfos)
                {
                    if (info.Order == 0)
                    {
                        info.Order = infos.Count(m => m.Position == info.Position) + 1;
                    }

                    infos.AddIfNotExist(info);
                }
                MethodInfo[] methods = _assemblyFinder.Find(o => o.FullName.Contains("QS.Core.Web")).FirstOrDefault()
                    .GetTypes().SelectMany(m => m.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    .Where(type => type.HasAttribute<ModuleInfoAttribute>()).ToArray();
                for (int index = 0; index < methods.Length; index++)
                {
                    ModuleInfo methodInfo = GetModule(methods[index], typeInfos.Last(), index);
                    infos.AddIfNotNull(methodInfo);
                }
            }

            return infos.ToArray();
        }


        /// <summary>
        /// 初始化添加预定义权限值
        /// </summary>
        /// <param name="app"></param>
        public void InitPermission()
        {
            //验证权限值是否重复
            ValidPermissions();
            //反射被标记的Controller和Action

            var permList = new List<MenuActionDto>();
            var actions = _assemblyFinder.Find(o => o.FullName.Contains("QS.Core.Web")).FirstOrDefault().GetTypes()
                .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
                .SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly));

            //遍历集合整理信息
            foreach (var action in actions)
            {
                var permissionAttribute =
                    action.GetCustomAttributes(typeof(PermissionAttribute), false).ToList();
                if (!permissionAttribute.Any())
                    continue;

                var codes = permissionAttribute.Select(a => ((PermissionAttribute)a).Code).ToArray();
                var controllerName = action?.ReflectedType?.Name.Replace("Controller", "").ToLower();
                var actionName = action.Name.ToLower();

                foreach (var item in codes)
                {
                    if (permList.Exists(c => c.Code == item))
                    {
                        var menuAction = permList.FirstOrDefault(a => a.Code == item);
                        menuAction?.Url.Add($"{controllerName}/{actionName}".ToLower());
                    }
                    else
                    {
                        var perm = new MenuActionDto
                        {
                            CreateDateTime = DateTime.Now,
                            Url = new List<string> { $"{controllerName}/{actionName}".ToLower() },
                            Code = item,
                            Name = ((PermCode)item).GetDisplayName() ?? ((PermCode)item).ToString()
                        };
                        permList.Add(perm);
                    }
                }
                //PermissionUrls.TryAdd($"{controllerName}/{actionName}".ToLower(), codes);
            }

            var data = permList;
        }


        /// <summary>
        /// 从类型中提取模块信息
        /// </summary>
        /// <param name="type">类型信息</param>
        /// <param name="existPaths">已存在的路径集合</param>
        /// <returns>提取到的模块信息</returns>
        protected ModuleInfo[] GetModules(Type type, string[] existPaths)
        {
            ModuleInfoAttribute infoAttr = type.GetAttribute<ModuleInfoAttribute>();
            if (infoAttr == null)
            {
                return new ModuleInfo[0];
            }
            ModuleInfo info = new ModuleInfo()
            {
                Name = infoAttr.Name ?? GetName(type),
                Code = infoAttr.Code ?? type.Name.Replace("Controller", ""),
                Order = infoAttr.Order,
                Position = GetPosition(type, infoAttr.Position),
                PositionName = infoAttr.PositionName
            };
            List<ModuleInfo> infos = new List<ModuleInfo>() { info };
            //获取中间分类模块
            if (infoAttr.Position != null)
            {
                info = new ModuleInfo()
                {
                    Name = infoAttr.PositionName ?? infoAttr.Position,
                    Code = infoAttr.Position,
                    Position = GetPosition(type, null)
                };
                if (!existPaths.Contains($"{info.Position}.{info.Code}"))
                {
                    infos.Insert(0, info);
                }
            }
            //获取区域模块
            string area, name;
            AreaInfoAttribute areaInfo = type.GetAttribute<AreaInfoAttribute>();
            if (areaInfo != null)
            {
                area = areaInfo.RouteValue;
                name = areaInfo.Display ?? area;
            }
            else
            {
                AreaAttribute areaAttr = type.GetAttribute<AreaAttribute>();
                area = areaAttr?.RouteValue ?? "Site";
                DisplayNameAttribute display = type.GetAttribute<DisplayNameAttribute>();
                name = display?.DisplayName ?? area;
            }
            info = new ModuleInfo()
            {
                Name = name,
                Code = area,
                Position = "Root",
                PositionName = name
            };
            if (!existPaths.Contains($"{info.Position}.{info.Code}"))
            {
                infos.Insert(0, info);
            }

            return infos.ToArray();
        }

        /// <summary>
        /// 从方法信息中提取模块信息
        /// </summary>
        /// <param name="method">方法信息</param>
        /// <param name="typeInfo">所在类型模块信息</param>
        /// <param name="index">序号</param>
        /// <returns>提取到的模块信息</returns>
        protected ModuleInfo GetModule(MethodInfo method, ModuleInfo typeInfo, int index)
        {
            ModuleInfoAttribute infoAttr = method.GetAttribute<ModuleInfoAttribute>();
            if (infoAttr == null)
            {
                return null;
            }
            ModuleInfo info = new ModuleInfo()
            {
                Name = infoAttr.Name ?? method.GetDescription() ?? method.Name,
                Code = infoAttr.Code ?? method.Name,
                Order = infoAttr.Order > 0 ? infoAttr.Order : index + 1,
            };
            string controller = method.DeclaringType?.Name.Replace("ControllerBase", string.Empty).Replace("Controller", string.Empty);
            info.Position = $"{typeInfo.Position}.{controller}";
            //依赖的功能
            string area = method.DeclaringType.GetAttribute<AreaAttribute>()?.RouteValue;
            //List<IFunction> dependOnFunctions = new List<IFunction>()
            //{
            //    FunctionHandler.GetFunction(area, controller, method.Name)
            //};
            //DependOnFunctionAttribute[] dependOnAttrs = method.GetAttributes<DependOnFunctionAttribute>();
            //foreach (DependOnFunctionAttribute dependOnAttr in dependOnAttrs)
            //{
            //    string dependArea = dependOnAttr.Area == null ? area : dependOnAttr.Area == string.Empty ? null : dependOnAttr.Area;
            //    string dependController = dependOnAttr.Controller ?? controller;
            //    IFunction function = FunctionHandler.GetFunction(dependArea, dependController, dependOnAttr.Action);
            //    if (function == null)
            //    {
            //        throw new Exception($"功能“{area}/{controller}/{method.Name}”的依赖功能“{dependArea}/{dependController}/{dependOnAttr.Action}”无法找到");
            //    }
            //    dependOnFunctions.Add(function);
            //}
            //info.DependOnFunctions = dependOnFunctions.ToArray();

            return info;
        }

        private static string GetName(Type type)
        {
            string name = type.GetDescription();
            if (name == null)
            {
                return type.Name.Replace("Controller", "");
            }
            if (name.Contains("-"))
            {
                name = name.Split('-').Last();
            }
            return name;
        }

        private static string GetPosition(Type type, string attrPosition)
        {
            string area = type.GetAttribute<AreaAttribute>()?.RouteValue;
            if (area == null)
            {
                //无区域，使用Root.Site位置
                return attrPosition == null
                    ? "Root.Site"
                    : $"Root.Site.{attrPosition}";
            }
            return attrPosition == null
                ? $"Root.{area}"
                : $"Root.{area}.{attrPosition}";
        }
    }
}