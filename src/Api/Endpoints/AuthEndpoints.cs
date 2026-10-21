using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StockAnalysis.Api.Auth;
using StockAnalysis.Domain.Entities;

namespace StockAnalysis.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth").WithTags("Auth");

        group.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .AllowAnonymous();

        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .AllowAnonymous();

        group.MapPost("/refresh", RefreshAsync)
            .WithName("Refresh")
            .AllowAnonymous();

        group.MapPost("/logout", LogoutAsync)
            .WithName("Logout")
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        UserManager<User> userManager,
        ITokenService tokenService,
        CancellationToken ct)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return Results.Conflict(new { error = "Email already registered" });

        var passwordHash = new PasswordHasher<User>().HashPassword(null!, request.Password);
        var user = User.Create(request.Email, passwordHash);

        var result = await userManager.CreateAsync(user);
        if (!result.Succeeded)
            return Results.BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();
                await tokenService.CreateRefreshTokenAsync(user, ct: ct);

        var response = new AuthResponse(accessToken, refreshToken, DateTimeOffset.UtcNow.AddMinutes(15), user.Tier);
        return Results.Ok(response);
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        UserManager<User> userManager,
        ITokenService tokenService,
        CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Results.Unauthorized();

                var result = await userManager.CheckPasswordAsync(user, request.Password);
                if (!result)
            return Results.Unauthorized();

        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();
                await tokenService.CreateRefreshTokenAsync(user, ct: ct);

        var response = new AuthResponse(accessToken, refreshToken, DateTimeOffset.UtcNow.AddMinutes(15), user.Tier);
        return Results.Ok(response);
    }

    private static async Task<IResult> RefreshAsync(
        RefreshRequest request,
        UserManager<User> userManager,
        ITokenService tokenService,
        CancellationToken ct)
    {
        var tokenHash = BCrypt.Net.BCrypt.HashPassword(request.RefreshToken);
        var storedToken = await tokenService.GetRefreshTokenAsync(tokenHash, ct);

        if (storedToken == null || storedToken.RevokedAt != null || storedToken.ExpiresAt < DateTimeOffset.UtcNow)
            return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(storedToken.UserId.ToString());
        if (user == null)
            return Results.Unauthorized();

        // Rotate refresh token
        await tokenService.RevokeRefreshTokenAsync(tokenHash, ct);
        var newRefreshToken = tokenService.GenerateRefreshToken();
        await tokenService.CreateRefreshTokenAsync(user, tokenHash, ct);

        var accessToken = tokenService.GenerateAccessToken(user);
        var response = new AuthResponse(accessToken, newRefreshToken, DateTimeOffset.UtcNow.AddMinutes(15), user.Tier);
        return Results.Ok(response);
    }

    private static async Task<IResult> LogoutAsync(
        RefreshRequest request,
        ITokenService tokenService,
        CancellationToken ct)
    {
        var tokenHash = BCrypt.Net.BCrypt.HashPassword(request.RefreshToken);
        await tokenService.RevokeRefreshTokenAsync(tokenHash, ct);
        return Results.Ok(new { message = "Logged out successfully" });
    }
}