﻿using Microsoft.OpenApi.Models;
using QingShan.DependencyInjection;

namespace QingShan.Core.SpecificationDocument
{
    /// <summary>
    /// 规范化文档开放接口信息
    /// </summary>
    [SkipScan]
    public sealed class SpecificationOpenApiInfo : OpenApiInfo
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public SpecificationOpenApiInfo()
        {
        }

        /// <summary>
        /// 分组私有字段
        /// </summary>
        private string _group;

        /// <summary>
        /// 所属组
        /// </summary>
        public string Group
        {
            get => _group;
            set
            {
                _group = value;
                Title ??= _group;
            }
        }

        /// <summary>
        /// 排序
        /// </summary>
        public int? Order { get; set; }
    }
}