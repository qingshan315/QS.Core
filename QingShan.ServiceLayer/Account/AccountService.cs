﻿using Microsoft.Extensions.Configuration;
using QingShan.Data;
using QingShan.DatabaseAccessor;
using QingShan.DependencyInjection;
using QingShan.Encryption;
using QingShan.DataLayer.Entities;
using QingShan.Services.Account.Dto;
using QingShan.Services.Account.Dto.OutputDto;
using QingShan.Services.User;
using System.Threading.Tasks;
using System;

namespace QingShan.Services.Account
{
    public class AccountService : IAccountService, IScopeDependency
    {
        private readonly IConfiguration _config;
        private readonly IUserService _userService;

        private readonly IRepository<UserEntity, int> _userRepository;

        public AccountService(IConfiguration config, IUserService userService, IRepository<UserEntity, int> userRepository)
        {
            _config = config;
            _userService = userService;
            _userRepository = userRepository;
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<StatusResult<AuthLoginOutputDto>> LoginAsync(LoginInputDto dto)
        {
            var user = await _userRepository
                .Select
                .Where(o => o.UserName == dto.Account).FirstAsync();
            if (user == null)
            {
                return new StatusResult<AuthLoginOutputDto>("账号或密码错误");
            }

            if (user.Status != EAdministratorStatus.Normal)
            {
                return new StatusResult<AuthLoginOutputDto>($"当前账号状态为：{user.Status.ToDescription()}");
            }

            var password = MD5Encrypt.Encrypt32(dto.Password);
            if (user.Password != password)
            {
                return new StatusResult<AuthLoginOutputDto>("账号或密码错误");
            }

            var model = new AuthLoginOutputDto()
            {
                Id = user.Id,
                NickName = user.NickName,
                UserName = user.UserName
            };
            return new StatusResult<AuthLoginOutputDto>(model);
        }
    }
}
