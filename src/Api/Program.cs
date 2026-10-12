using Api;
using Api.Middleware;
using Application.Interfaces;
using Application.Services;
using Application.Settings;
using Core.Entities;
using Infrastructure.Auth;
using Application.Validators;
using FluentValidation;
using Infrastructure.Data;
using Infrastructure.Repositories;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("default"))
);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();

//automatically included w above thing
//builder.Services.AddValidatorsFromAssemblyContaining<CreateCustomerValidator>();
//...

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
var jwt =
    builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt settings are missing.");
if (jwt.Key.Length < 32)
    throw new InvalidOperationException("Jwt:Key must be at least 32 characters.");

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// dev convenience: apply migrations and add sample data on first run
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    if (!await db.Products.AnyAsync())
    {
        db.Products.AddRange(
            new Product { Name = "Mechanical Keyboard", Price = 89.99m },
            new Product { Name = "Wireless Mouse", Price = 34.50m },
            new Product { Name = "27\" Monitor", Price = 249.00m },
            new Product { Name = "USB-C Hub", Price = 42.90m },
            new Product { Name = "Laptop Stand", Price = 29.99m },
            new Product { Name = "Webcam 1080p", Price = 59.00m }
        );
        db.Customers.AddRange(
            new Customer { Name = "Alice Smith", Email = "alice@example.com" },
            new Customer { Name = "Bob Jones", Email = "bob@example.com" }
        );
        await db.SaveChangesAsync();
    }
}

// bootstrap the first admin from config (Seed:AdminEmail / Seed:AdminPassword), only if absent
var seedEmail = app.Configuration["Seed:AdminEmail"];
var seedPassword = app.Configuration["Seed:AdminPassword"];
if (!string.IsNullOrWhiteSpace(seedEmail) && !string.IsNullOrWhiteSpace(seedPassword))
{
    using var scope = app.Services.CreateScope();
    var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    var passwords = scope.ServiceProvider.GetRequiredService<IPasswordService>();
    var email = seedEmail.Trim().ToLowerInvariant();
    if (await users.GetByEmailAsync(email) == null)
    {
        var admin = new User { Email = email, Role = UserRole.Admin };
        admin.PasswordHash = passwords.Hash(admin, seedPassword);
        await users.AddAsync(admin);
    }
}

app.UseSerilogRequestLogging();

app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
