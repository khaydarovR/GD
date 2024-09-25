using GD.Api;
using GD.Api.DB;
using GD.Api.DB.Models;
using GD.Api.Hubs;
using GD.Shared.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddLogging();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("allow", policyBuilder => policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddDbContext<AppDbContext>(options =>options.UseSqlite("Data Source=GDdb0.db"));
builder.Services
    .AddIdentity<GDUser, IdentityRole<Guid>>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 1;
        options.Password.RequireUppercase = false;
        options.Password.RequiredUniqueChars = 1;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    })
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("admin", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(GDUserClaimTypes.Roles, GDUserRoles.Admin);
    });
    options.AddPolicy("client", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(GDUserClaimTypes.Roles, GDUserRoles.Client);
    });
    options.AddPolicy("courier", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(GDUserClaimTypes.Roles, GDUserRoles.Courier);
    });
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // ���������, ����� �� �������������� �������� ��� ��������� ������
            ValidateIssuer = true,
            // ������, �������������� ��������
            ValidIssuer = Options.AuthOptions.ISSUER,
            // ����� �� �������������� ����������� ������
            ValidateAudience = true,
            // ��������� ����������� ������
            ValidAudience = Options.AuthOptions.AUDIENCE,
            // ����� �� �������������� ����� �������������
            ValidateLifetime = true,
            // ��������� ����� ������������
            IssuerSigningKey = Options.AuthOptions.GetSymmetricSecurityKey(),
            // ��������� ����� ������������
            ValidateIssuerSigningKey = true,
        };
    });

var app = builder.Build();

// role seeding
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    async Task CreateRoleAsync(string name)
    {
        if (!await roleManager.RoleExistsAsync(name)) await roleManager.CreateAsync(new(name) { Id = Guid.NewGuid() });
    }

    await CreateRoleAsync(GDUserRoles.Client);
    await CreateRoleAsync(GDUserRoles.Admin);
    await CreateRoleAsync(GDUserRoles.Courier);
}

// admin seeding
{
    using var scope = app.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<GDUser>>();
    const string email = "razil_khayka@mail.ru";
    const string password = "a123";
    if (!userManager.Users.Any(u => u.Email == email))
    {
        var user = new GDUser { Email = email, UserName = email };
        await userManager.CreateAsync(user, password);
        await userManager.AddToRoleAsync(user, GDUserRoles.Admin);
    }
}

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("allow");

app.MapControllers();

app.MapHub<PosHub>("/poshub");

app.Run();
