using AirlineManager.Application.Common.Interfaces;
using AirlineManager.Application.Common.Models;
using AirlineManager.Application.DTOs.Auth;
using AirlineManager.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AirlineManager.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return Result<AuthResponse>.Fail("User with this email already exists");
        }

        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<AuthResponse>.Fail(errors);
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