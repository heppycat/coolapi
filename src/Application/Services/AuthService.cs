using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Core.Entities;
using FluentValidation;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repository;
    private readonly IPasswordService _passwords;
    private readonly IJwtTokenGenerator _tokens;
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly IValidator<LoginDto> _loginValidator;

    public AuthService(
        IUserRepository repository,
        IPasswordService passwords,
        IJwtTokenGenerator tokens,
        IValidator<RegisterDto> registerValidator,
        IValidator<LoginDto> loginValidator
    )
    {
        _repository = repository;
        _passwords = passwords;
        _tokens = tokens;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        await _registerValidator.ValidateAndThrowAsync(dto);

        var email = dto.Email.Trim().ToLowerInvariant();
        if (await _repository.GetByEmailAsync(email) != null)
            throw new ConflictException("Email is already registered.");

        // self-registration can only ever create a plain User; admins are promoted separately
        var user = new User { Email = email, Role = UserRole.User };
        user.PasswordHash = _passwords.Hash(user, dto.Password);

        await _repository.AddAsync(user);

        return BuildResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        await _loginValidator.ValidateAndThrowAsync(dto);

        var user = await _repository.GetByEmailAsync(dto.Email.Trim().ToLowerInvariant());

        // same message for "no such user" and "wrong password" so emails can't be enumerated
        if (user == null || !_passwords.Verify(user, dto.Password))
            throw new AuthenticationFailedException("Invalid email or password.");

        return BuildResponse(user);
    }

    private AuthResponseDto BuildResponse(User user)
    {
        var (token, expiresAt) = _tokens.Generate(user);
        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role,
            },
        };
    }
}
