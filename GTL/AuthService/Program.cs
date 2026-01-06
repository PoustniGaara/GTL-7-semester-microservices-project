using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Swagger (optional but nice for demo)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// POST /auth/login
app.MapPost("/auth/login", (LoginRequest request, IConfiguration config) =>
{
    // Basic input guard
    if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        return Results.Unauthorized();

    // Demo credentials rule
    if (request.Username != "admin" || request.Password != "admin")
        return Results.Unauthorized();

    var jwtSection = config.GetSection("Jwt");
    var issuer = jwtSection["Issuer"];
    var audience = jwtSection["Audience"];
    var secret = jwtSection["SigningKey"];
    var minutesStr = jwtSection["ExpiresMinutes"];


    if (string.IsNullOrWhiteSpace(issuer) ||
        string.IsNullOrWhiteSpace(audience) ||
        string.IsNullOrWhiteSpace(secret))
    {
        // Misconfiguration => fail fast (you can return 500 or throw)
        return Results.Problem("JWT configuration missing (Jwt:Issuer/Audience/Secret).");
    }

    var accessTokenMinutes = 45;
    if (int.TryParse(minutesStr, out var m) && m > 0)
        accessTokenMinutes = m;

    var now = DateTime.UtcNow;
    var expires = now.AddMinutes(accessTokenMinutes);

    // Minimal claims requested
    var claims = new[]
    {
        // "sub" or name -> we’ll set both for convenience in demos
        new Claim(JwtRegisteredClaimNames.Sub, "admin"),
        new Claim(ClaimTypes.Name, "admin"),
        new Claim(ClaimTypes.Role, "Admin")
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        notBefore: now,
        expires: expires,
        signingCredentials: creds
    );

    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

    var response = new TokenResponse(
        AccessToken: tokenString,
        TokenType: "Bearer",
        ExpiresIn: (int)(expires - now).TotalSeconds
    );

    return Results.Ok(response);
})
.WithName("Login")
.Produces<TokenResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized);

app.Run();

public sealed record LoginRequest(string Username, string Password);

public sealed record TokenResponse(string AccessToken, string TokenType, int ExpiresIn);
