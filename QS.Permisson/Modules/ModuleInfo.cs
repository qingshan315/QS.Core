﻿using System;
using System.Collections.Generic;
using System.Text;

namespace QS.Permission.Modules
{
    /// <summary>
    /// 从程序集中提取的模块信息载体，包含模块基本信息和模块依赖的功能信息集合
    /// </summary>
    public class ModuleInfo
    {
        /// <summary>
        /// 获取或设置 模块名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置 模块代码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 获取或设置 层次序号
        /// </summary>
        public double Order { get; set; }

        /// <summary>
        /// 获取或设置 模块位置，父模块Code以点号 . 相连的字符串
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// 获取或设置 父级模块名称，需要创建父级模块的时候设置值
        /// </summary>
        public string PositionName { get; set; }

        ///// <summary>
        ///// 获取或设置 依赖功能
        ///// </summary>
        //public IFunction[] DependOnFunctions { get; set; } = new IFunction[0];
        //private string ToDebugDisplay()
        //{
        //    return $"{Name}[{Code}]({Position}),FunctionCount:{DependOnFunctions.Length}";
        //}
    }
}