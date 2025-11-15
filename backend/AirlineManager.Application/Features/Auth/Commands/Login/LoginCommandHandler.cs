using AirlineManager.Application.Common.Interfaces;
using AirlineManager.Application.Common.Models;
using AirlineManager.Application.DTOs.Auth;
using AirlineManager.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AirlineManager.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Result<AuthResponse>.Fail("Invalid email or password");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            return Result<AuthResponse>.Fail("Invalid email or password");
        }

        var token = _jwtTokenGenerator.GenerateToken(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName);

        var response = new AuthResponse
        {
            Token = token,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName
        };

        return Result<AuthResponse>.Ok(response);
    }
}