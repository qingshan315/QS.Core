﻿using QS.Core.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace QS.DataLayer.Entities
{
    /// <summary>
    /// 角色模块
    /// </summary>
    public class RoleModuleEntity:EntityBaseById<int>
    {
        /// <summary>
        /// 角色Id
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// 模块id
        /// </summary>
        public Guid ModuleId { get; set; }
    }
}