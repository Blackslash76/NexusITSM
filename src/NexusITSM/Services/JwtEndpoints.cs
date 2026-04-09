using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NexusITSM.Models.Entities;

namespace NexusITSM.Services;

public static class JwtEndpoints
{
    public static void MapJwtEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/token", async (
            [FromBody] TokenRequest req,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            JwtService jwt) =>
        {
            var user = await userManager.FindByEmailAsync(req.Email);
            if (user == null)
                return Results.Unauthorized();

            var valid = await signInManager.CheckPasswordSignInAsync(user, req.Password, false);
            if (!valid.Succeeded)
                return Results.Unauthorized();

            var token = await jwt.GenerateTokenAsync(user);
            var roles = await userManager.GetRolesAsync(user);

            return Results.Ok(new
            {
                token,
                expiresIn = 86400,
                user = new { user.Id, user.Email, user.FullName, user.Initials, Roles = roles }
            });
        });

        app.MapPost("/api/auth/password-reset-request", async (
            [FromBody] PasswordResetRequest req,
            UserManager<AppUser> userManager) =>
        {
            var user = await userManager.FindByEmailAsync(req.Email);
            if (user == null)
                return Results.Ok(new { message = "Se l'email esiste, riceverai un link per il reset." });

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            // In production: send email with token
            // For now: return token directly (dev only)
            return Results.Ok(new { message = "Token generato.", resetToken = token });
        });

        app.MapPost("/api/auth/password-reset", async (
            [FromBody] PasswordResetConfirm req,
            UserManager<AppUser> userManager) =>
        {
            var user = await userManager.FindByEmailAsync(req.Email);
            if (user == null)
                return Results.BadRequest(new { error = "Utente non trovato." });

            var result = await userManager.ResetPasswordAsync(user, req.Token, req.NewPassword);
            if (!result.Succeeded)
                return Results.BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            return Results.Ok(new { message = "Password aggiornata con successo." });
        });
    }

    public record TokenRequest(string Email, string Password);
    public record PasswordResetRequest(string Email);
    public record PasswordResetConfirm(string Email, string Token, string NewPassword);
}
