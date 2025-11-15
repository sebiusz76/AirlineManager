using AirlineManager.Application.Common.Models;
using AirlineManager.Application.DTOs.Auth;
using MediatR;

namespace AirlineManager.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName) : IRequest<Result<AuthResponse>>;