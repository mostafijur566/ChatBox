using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dto.Account;
using api.Models;

namespace api.Mappers
{
    public static class AccountMapper
    {
        public static UserDto ToUserDto(this User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Phonenumber = user.Phonenumber,
                ProfilePicture = user.ProfilePicture,
                StatusMessage = user.StatusMessage,
                LastSeen = user.LastSeen,
                IsOnline = user.IsOnline
            };
        }
    }
}