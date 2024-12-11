using GD.Api;
using GD.Api.DB;
using GD.Api.DB.Models;
using GD.Api.Hubs;
using GD.Shared.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddLogging();
builder.Logging.AddConsole();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("allow", policyBuilder => policyBuilder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
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

//main data seeding
{
    using var scope = app.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<GDUser>>();
    var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    async Task<Guid?> CreateUserAsync(string email, string password, double balance, string role)
    {
        if (!userManager.Users.Any(u => u.Email == email))
        {
            var user = new GDUser { Email = email, UserName = email, Balance = balance};
            await userManager.CreateAsync(user, password);
            await userManager.AddToRoleAsync(user, role);
            return user.Id;
        }

        return null;
    }
    
    async Task<Guid?> AddProductAsync(string name, string description, double price, string tag = "суши", string image = Images.filadelfia)
    {
        if (!appDbContext.Products.Any(p => p.Name == name))
        {
            var product = new Product
            {
                Name = name,
                Amount = 199,
                Description = description,
                Price = price,
                ImageValue = image,
                Feedbacks = [],
                IsDeleted = false,
                Tags = tag
            };
            
            await appDbContext.AddAsync(product);
            await appDbContext.SaveChangesAsync();
            return product.Id;
        }

        return null;
    }
    

    async Task<Guid> CreateOrderAsync(Guid? UserId, List<Guid?> products)
    {
        if (UserId is null || products.Any(p => p is null)) 
            return Guid.Empty;
        
        var order = new Order
        {
            CreatedAt = DateTime.UtcNow,
            Status = "Waiting",
            ClientId = UserId.Value,
            PayMethod = "online",
            ToAddress = "asdasdad",
        };
        
        await appDbContext.Orders.AddAsync(order);
        await appDbContext.SaveChangesAsync();

        foreach (var product in products)
        {
            var orderItem = new OrderItem
            {
                Amount = 5,
                OrderId = order.Id,
                ProductId = product!.Value,
            };
            
            await appDbContext.OrderItems.AddAsync(orderItem);
        }
        
        await appDbContext.SaveChangesAsync();
        return order.Id;
    }

    var bananaId = await AddProductAsync("Филадельфия с зеленым луком", "Охлажденный лосось, сливочный сыр, зеленый лучок, рис, нори", 100, "роллы");
    var buterbrodId = await AddProductAsync("Честер ролл", "Копченая курочка, свежие томаты, сыр Чеддер, хрустящий лук фри(внутри)", 200, "фастфуд", Images.chester);
    var teaId = await AddProductAsync("Филадельфия с авокадо", "Соевый соус, васаби и имбирь уже в комплекте. Может содержать косточки", 300);
    
    var client1 = await CreateUserAsync("client1@c", "qwerty", Random.Shared.Next(1000, 2001), GDUserRoles.Client);
    var client2 = await CreateUserAsync("client2@c", "qwerty", Random.Shared.Next(1000, 2001), GDUserRoles.Client);
    var client3 = await CreateUserAsync("client3@c", "qwerty", Random.Shared.Next(1000, 2001), GDUserRoles.Client);
    var client4 = await CreateUserAsync("client4@c", "qwerty", Random.Shared.Next(1000, 2001), GDUserRoles.Client);
    var client5 = await CreateUserAsync("client5@c", "qwerty", Random.Shared.Next(1000, 2001), GDUserRoles.Client);
    
    await CreateUserAsync("courier1@c", "qwerty", 0, GDUserRoles.Courier);
    await CreateUserAsync("courier2@c", "qwerty", 0, GDUserRoles.Courier);
    await CreateUserAsync("courier3@c", "qwerty", 0, GDUserRoles.Courier);

/*    await CreateOrderAsync(client1, [bananaId, buterbrodId]);
    await CreateOrderAsync(client2, [teaId, buterbrodId]);
    await CreateOrderAsync(client3, [buterbrodId]);
    await CreateOrderAsync(client4, [teaId, buterbrodId, bananaId]);
    await CreateOrderAsync(client5, [bananaId]);*/
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
