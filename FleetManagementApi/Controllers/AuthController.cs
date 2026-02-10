using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using FleetManagementApi.Data;
using FleetManagementApi.Domain.Entities;
using FleetManagementApi.Exceptions;
using FleetManagementApi.Models;

namespace FleetManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly FleetDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuthController(FleetDbContext context, IConfiguration configuration)
    {
        _dbContext = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterRequest register)
    {
        //Validate ModelState
        if (!ModelState.IsValid)
        {
            throw new BadRequestException("Invalid registration data");
        }
        // Normalize email so "Test@Mail.com" and "test@mail.com" are treated as the same
        var email = register.Email.Trim().ToLowerInvariant();

        // Check if user already exists
        var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existingUser != null)
        {
            throw new ConflictException("User already exists");
        }
        // Hash password (trim to match login behavior and avoid client-added spaces)
        var passwordToHash = register.Password.Trim();
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(passwordToHash);

        // Create new user
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            UserName = register.Username,
            Email = email,
            PasswordHash = hashedPassword,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        //Add User to database
        _dbContext.Users.Add(newUser);
        await _dbContext.SaveChangesAsync();

        return StatusCode(201, new { id = newUser.Id, email = newUser.Email });
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        //Validate ModelState
        if (!ModelState.IsValid)
        {
            throw new BadRequestException("Invalid login data");
        }
        // Normalize email for lookup
        var email = loginRequest.Email.Trim().ToLowerInvariant();

        // Check if user exists
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            throw new NotFoundException("User with this email is not found");
        }

        // Verify password (trim so spaces from client/Swagger don't cause mismatch)
        var passwordToVerify = loginRequest.Password.Trim();
        if (!BCrypt.Net.BCrypt.Verify(passwordToVerify, user.PasswordHash))
        {
            throw new BadRequestException("Password is incorrect. Please try again.");
        }

        // Generate JWT token using JsonWebTokenHandler (Microsoft.IdentityModel.JsonWebTokens)
        var token = GenerateJwtToken(user);
        var expiresMinutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes", 60);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiresMinutes);

        return Ok(new AuthResponse { Token = token, ExpiresAt = expiresAt });
    }

    private string GenerateJwtToken(User user)
    {
        var secretKey = _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email)
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:ExpirationMinutes", 60)),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = credentials
        };

        var handler = new JsonWebTokenHandler();
        // CreateToken(SecurityTokenDescriptor) returns the encoded JWT string directly
        return handler.CreateToken(descriptor);
    }
}