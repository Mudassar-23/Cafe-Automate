using System.Text;
using CafeAutomate.Api.Data;
using CafeAutomate.Api.Hubs;
using CafeAutomate.Api.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// Load .env when present (local dev). In Docker, env vars are injected
// directly by docker-compose so the file won't exist — that's fine.
try { Env.TraversePath().Load(); } catch { /* no .env file — using Docker env vars */ }

var builder = WebApplication.CreateBuilder(args);

// ── Database (Postgres preferred, SQLite fallback) ───────────────────────────
builder.AddAppDatabase();

// ── JWT Auth ─────────────────────────────────────────────────────────────────
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt__Secret is not set. Add it to your .env file.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        // Keep claim names exactly as issued (e.g. "role") instead of letting
        // the handler remap them to long ClaimTypes.* URIs, which broke
        // RoleGuard's User.FindFirst("role") lookups.
        opt.MapInboundClaims = false;

        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "CafeAutomate",
            ValidAudience = "CafeAutomateApp",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };

        // Support SignalR token from query string
        opt.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"];
                var path = ctx.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    ctx.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ── Services ─────────────────────────────────────────────────────────────────
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<SeederService>();
builder.Services.AddSignalR();
builder.Services.AddControllers();

// ── CORS ─────────────────────────────────────────────────────────────────────
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("FrontendPolicy", policy =>
    {
        var origins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                      ?? new[] { "http://localhost:5500", "http://127.0.0.1:5500", "http://localhost:3000" };

        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ── Migrate + Seed ────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    var seeder = scope.ServiceProvider.GetRequiredService<SeederService>();
    await seeder.SeedAsync();
}

// ── Middleware pipeline ────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<OrderHub>("/hubs/orders");

app.Run();
