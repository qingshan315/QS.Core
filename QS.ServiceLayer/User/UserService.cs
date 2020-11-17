﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using QS.Core.Data;
using QS.Core.Dependency;
using QS.Core.Encryption;
using QS.Core.Permission;
using QS.DataLayer.Entities;
using QS.ServiceLayer.User.Dtos.InputDto;
using QS.ServiceLayer.User.Dtos.OutputDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace QS.ServiceLayer.User
{
    /// <summary>
    /// 用户管理-
    /// </summary>
    public class UserService : IUserService, IScopeDependency
    {
        private readonly IUserInfo _user;
        private readonly IMapper _mapper;
        private readonly EFContext _context;
        private readonly IConfigurationProvider _configurationProvider;
        /// <summary>
        /// 用户信息
        /// </summary>
        private IQueryable<UserEntity> Users => _context.Users.GetTrackEntities();

        public UserService(IUserInfo user,
            IMapper mapper,
            EFContext context,
            IConfigurationProvider configurationProvider
            )
        {
            _user = user;
            _mapper = mapper;
            _context = context;
            _configurationProvider = configurationProvider;
        }
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<StatusResult<UserGetOutputDto>> GetAsync(int id)
        {

            //var entity = await _userRepository.Select
            //.WhereDynamic(id)
            //.IncludeMany(a => a.Roles.Select(b => new RoleEntity { Id = b.Id }))
            //.ToOneAsync();

            var entityDto = await _context.Users.ProjectTo<UserGetOutputDto>(_configurationProvider).FirstOrDefaultAsync(o => o.Id == id);
            //var entityDto = _mapper.Map<UserGetOutputDto>(entity);
            return new StatusResult<UserGetOutputDto>(entityDto);
        }
        /// <summary>
        /// 获取用户基本信息
        /// </summary>
        /// <returns></returns>
        public async Task<StatusResult<UserGetOutputDto>> GetBasicAsync()
        {
            if (!(_user?.Id > 0))
            {
                return new StatusResult<UserGetOutputDto>("未登录！");
            }

            var data = await Users.ProjectTo<UserGetOutputDto>(_configurationProvider).FirstOrDefaultAsync(o => o.Id == _user.Id);

            return new StatusResult<UserGetOutputDto>(data);
        }

        /// <summary>
        /// 获取权限信息
        /// </summary>
        /// <returns></returns>
        public async Task<IList<string>> GetPermissionsAsync()
        {
            //var key = string.Format(CacheKey.UserPermissions, _user.Id);
            //if (await _cache.ExistsAsync(key))
            //{
            //    return await _cache.GetAsync<IList<string>>(key);
            //}
            //else
            //{

            //}
            //这里加缓存

            var userPermissoins = await (from rp in _context.RoleModules
                                         join ur in _context.UserRole on rp.RoleId equals ur.RoleId
                                         join p in _context.Modules on rp.ModuleId equals p.Id
                                         select p.Id.ToString()).ToListAsync();
            return userPermissoins;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<PageOutputDto<UserListOutputDto>> PageAsync(PageInputDto dto)
        {

            var data = await Users.LoadPageListAsync(dto, u => new UserListOutputDto
            {
                CreatedTime = u.CreateTime,
                Id = u.Id,
                Name = u.RealName,
                NickName = u.NickName,
                UserName = u.UserName,
                Status = u.Status
            }, o => dto.Search.IsNull() || o.UserName.Contains(dto.Search) || o.NickName.Contains(dto.Search));
            return data;
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<StatusResult> AddAsync(UserAddInputDto input)
        {
            if (input.Password.IsNull())
            {
                input.Password = "123456"; //初始密码 123456
            }

            if (Users.Any(o => o.UserName == input.UserName))
            {
                return new StatusResult("账号已存在");
            }

            input.Password = MD5Encrypt.Encrypt32(input.Password);
            var entity = _mapper.Map<UserEntity>(input);
            var res = await _context.InsertEntityAsync<UserEntity, int>(entity);
            return new StatusResult(res > 0, "添加失败");
        }

        /// <summary>
        /// 修改用户
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<StatusResult> UpdateAsync(UserUpdateInputDto input)
        {
            if (!(input?.Id > 0))
            {
                return new StatusResult("未获取到用户信息");
            }

            var user = await Users.FirstOrDefaultAsync(o => o.Id == input.Id);
            if (!(user?.Id > 0))
            {
                return new StatusResult("用户不存在！");
            }

            var users = _mapper.Map(input, user);

            Expression<Func<UserEntity, object>>[] updatedProperties = {
                    p => p.Avatar,
                    p => p.NickName,
                    p => p.Phone,
                    p => p.RealName
                };
            int res = await _context.UpdateEntity<UserEntity, int>(users, updatedProperties);
            return new StatusResult(res > 0, "修改失败");
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<StatusResult> DeleteAsync(int id)
        {
            var res = await _context.Set<UserEntity>().DeleteByIdAsync(id);

            return new StatusResult(res > 0, "删除失败");
        }
    }
}
