﻿namespace OutOfSchool.AuthCommon.Services.Interfaces;
public interface IUserService
{
    Task<ResponseDto> LogOutUserById(string userId);
}
