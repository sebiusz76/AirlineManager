using AirlineManager.Application.Common.Models;
using AirlineManager.Application.DTOs.Auth;
using MediatR;

namespace AirlineManager.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password) : IRequest<Result<AuthResponse>>;