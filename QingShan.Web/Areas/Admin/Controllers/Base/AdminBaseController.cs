﻿using Microsoft.AspNetCore.Mvc;
using QingShan.Attributes;
using QingShan.Attributes.Permission;

namespace QingShan.Core.Web.Areas.Admin.Controllers
{
    /// <summary>
    /// admin控制器基类
    /// </summary>
    [AreaInfo("Admin", Display = "后台管理")]
    [ApiController]
    [Route("/Api/[controller]/[action]")]
    [ApiDescriptionSettings("Default")]
    [Permission]
    public class AdminBaseController : ControllerBase
    {
    }
}
