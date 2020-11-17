﻿using QS.Core.Data;
using QS.ServiceLayer.ProductService.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QS.ServiceLayer.ProductService
{
    public interface IProductService
    {
        Task<List<ProductOutputDto>> Get();

        /// <summary>
        /// 删除指定数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<StatusResult> Delete(int id);

        Task<StatusResult> Add(ProductInputDto dto);
    }
}
