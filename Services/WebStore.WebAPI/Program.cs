using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebStore.DAL.Context;
using WebStore.Domain.Entities.Identity;
using WebStore.Interfaces.Services;
using WebStore.Logging;
using WebStore.Services.Services;
using WebStore.Services.Services.InSQL;
using WebStore.WebAPI.Infrastructure.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddLog4Net();

// Add services to the container.
var services = builder.Services;
services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(opt =>
{
    const string webstore_api_xml = "WebStore.WebAPI.xml";
    const string webstore_domain_xml = "WebStore.Domain.xml";
    const string debug_path = "bin/Debug/net6.0";

    if (File.Exists(webstore_api_xml))
        opt.IncludeXmlComments(webstore_api_xml);
    else if (File.Exists(Path.Combine(debug_path, webstore_api_xml)))
        opt.IncludeXmlComments(Path.Combine(debug_path, webstore_api_xml));

    if (File.Exists(webstore_domain_xml))
        opt.IncludeXmlComments(webstore_domain_xml);
    else if (File.Exists(Path.Combine(debug_path, webstore_domain_xml)))
        opt.IncludeXmlComments(Path.Combine(debug_path, webstore_domain_xml));
});

var database_type = builder.Configuration["Database"];
switch (database_type)
{
    default: throw new InvalidOperationException($"??? ?? {database_type} ?? ??????????????");

    case "SqlServer":
        services.AddDbContext<WebStoreDB>(opt =>
            opt.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));
        break;

    case "Sqlite":
        services.AddDbContext<WebStoreDB>(opt =>
            opt.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"),
                o => o.MigrationsAssembly("WebStore.DAL.Sqlite")));
        break;
}

services.AddTransient<IDbInitializer, DbInitializer>();

services.AddIdentity<User, Role>()
   .AddEntityFrameworkStores<WebStoreDB>()
   .AddDefaultTokenProviders();

services.Configure<IdentityOptions>(opt =>
{
#if DEBUG
    opt.Password.RequireDigit = false;
    opt.Password.RequireLowercase = false;
    opt.Password.RequireUppercase = false;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequiredLength = 3;
    opt.Password.RequiredUniqueChars = 3;
#endif

    opt.User.RequireUniqueEmail = false;
    opt.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIGKLMNOPQRSTUVWXYZ1234567890";

    opt.Lockout.AllowedForNewUsers = false;
    opt.Lockout.MaxFailedAccessAttempts = 10;
    opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
});

services.AddScoped<IEmployeesData, SqlEmployeesData>();
services.AddScoped<IProductData, SqlProductData>();
services.AddScoped<IOrderService, SqlOrderService>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db_initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await db_initializer.InitializeAsync(RemoveBefore: false).ConfigureAwait(true);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();
