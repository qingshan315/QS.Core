﻿using QS.Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace QS.DataLayer.Entities
{
    /// <summary>
    /// 用户表
    /// </summary>
    public class User:EntityBase<int>
    {
        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 部门Id
        /// </summary>
        public int DepartmentId { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 账号状态
        /// </summary>
        public EAdministratorStatus Status { get; set; }

        /// <summary>
        /// 是否本系统超级管理员
        /// </summary>
        public bool IsSuper { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateDateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateDateTime { get; set; }
    }
    /// <summary>
    /// 用户状态
    /// </summary>
    public enum EAdministratorStatus
    {
        [Description("已停用")]
        Stop = -1,
        [Description("已删除")]
        Deleted = -1,
        [Description("未激活")]
        NotActive = 0,
        [Description("正常")]
        Normal = 1,
    }
}