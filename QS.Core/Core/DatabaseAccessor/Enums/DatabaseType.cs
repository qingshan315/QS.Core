﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QS.Core.DatabaseAccessor
{
    /// <summary>
    /// 数据库类型
    /// </summary>
    public enum DatabaseType
    {
        /// <summary>
        /// SqlServer数据库类型
        /// </summary>
        SqlServer,
        /// <summary>
        /// Sqlite数据库类型
        /// </summary>
        Sqlite,
        /// <summary>
        /// MySql数据库类型
        /// </summary>
        MySql,
        /// <summary>
        /// PostgreSql数据库类型
        /// </summary>
        PostgreSql,
        /// <summary>
        /// Oracle数据库类型
        /// </summary>
        Oracle
    }
}