using Microsoft.AspNetCore.Identity;
using NexusITSM.Models.Entities;

namespace NexusITSM.Services;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/login", async (
            HttpContext context,
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager) =>
        {
            var form = await context.Request.ReadFormAsync();
            var email = form["Email"].ToString();
            var password = form["Password"].ToString();
            var rememberMe = form["RememberMe"].ToString() == "on";
            var returnUrl = form["ReturnUrl"].ToString();
            if (string.IsNullOrEmpty(returnUrl)) returnUrl = "/";

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                context.Response.Redirect("/login?error=invalid");
                return;
            }

            var result = await signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                context.Response.Redirect(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                context.Response.Redirect("/login?error=locked");
            }
            else
            {
                context.Response.Redirect("/login?error=invalid");
            }
        }).DisableAntiforgery();

        app.MapPost("/api/auth/register", async (
            HttpContext context,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager) =>
        {
            var form = await context.Request.ReadFormAsync();
            var fullName = form["FullName"].ToString();
            var email = form["Email"].ToString();
            var password = form["Password"].ToString();
            var department = form["Department"].ToString();

            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                context.Response.Redirect("/register?error=exists");
                return;
            }

            var initials = string.Join("", fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Take(2).Select(w => w[0])).ToUpper();

            var user = new AppUser
            {
                UserName = email, Email = email, FullName = fullName,
                Initials = initials, Department = department,
                Color = "#4B9EFF", IsActive = true, EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "User");
                await signInManager.SignInAsync(user, isPersistent: false);
                context.Response.Redirect("/");
            }
            else
            {
                context.Response.Redirect("/register?error=failed");
            }
        }).DisableAntiforgery();

        app.MapGet("/api/auth/logout", async (
            SignInManager<AppUser> signInManager,
            HttpContext context) =>
        {
            await signInManager.SignOutAsync();
            context.Response.Redirect("/login");
        });
    }
}
