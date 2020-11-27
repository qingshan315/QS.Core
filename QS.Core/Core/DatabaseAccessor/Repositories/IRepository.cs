﻿using FreeSql;

namespace QS.Core.DatabaseAccessor
{
    /// <summary>
    /// 仓储
    /// </summary>
    public interface IRepository<TEntity, TKey> : IBaseRepository<TEntity, TKey> 
        where TEntity : class,new()
    {

    }
}
