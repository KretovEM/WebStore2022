using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebStore.DAL.Context;
using WebStore.Domain.Entities.Identity;
using WebStore.Infrastructure.Conventions;
using WebStore.Infrastructure.Middleware;
using WebStore.Interfaces.Services;
using WebStore.Interfaces.TestAPI;
using WebStore.Services.Services;
using WebStore.Services.Services.InCookies;
using WebStore.WebAPI.Clients.Orders;
using WebStore.WebAPI.Clients.Employees;
using WebStore.WebAPI.Clients.Products;
using WebStore.WebAPI.Clients.Values;
using WebStore.WebAPI.Clients.Identity;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

#region Настройка построителя приложения - определение содержимого

var services = builder.Services;
services.AddControllersWithViews(opt =>
{
    opt.Conventions.Add(new TestConvention());
});
var configuration = builder.Configuration;
var database_type = builder.Configuration["Database"];
switch (database_type)
{
    default: throw new InvalidOperationException($"Тип БД {database_type} не поддерживается");

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
   //.AddEntityFrameworkStores<WebStoreDB>()
   .AddDefaultTokenProviders();

services.AddHttpClient("WebStoreAPIIdentity", client => client.BaseAddress = new(configuration["WebAPI"]))
   .AddTypedClient<IUserStore<User>, UsersClient>()
   .AddTypedClient<IUserRoleStore<User>, UsersClient>()
   .AddTypedClient<IUserPasswordStore<User>, UsersClient>()
   .AddTypedClient<IUserEmailStore<User>, UsersClient>()
   .AddTypedClient<IUserPhoneNumberStore<User>, UsersClient>()
   .AddTypedClient<IUserTwoFactorStore<User>, UsersClient>()
   .AddTypedClient<IUserClaimStore<User>, UsersClient>()
   .AddTypedClient<IUserLoginStore<User>, UsersClient>()
   .AddTypedClient<IRoleStore<Role>, RolesClient>();

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

services.ConfigureApplicationCookie(opt =>
{
    opt.Cookie.Name = "WebStore.GB";
    opt.Cookie.HttpOnly = true;

    //opt.Cookie.Expiration = TimeSpan.FromDays(10); // устарело
    opt.ExpireTimeSpan = TimeSpan.FromDays(10);

    opt.LoginPath = "/Account/Login";
    opt.LogoutPath = "/Account/Logout";
    opt.AccessDeniedPath = "/Account/AccessDenied";

    opt.SlidingExpiration = true;
});

//services.AddSingleton<IEmployeesData, InMemoryEmployeesData>(); // Singleton - потому что InMemory!
//services.AddSingleton<IProductData, InMemoryProductData>();     // Singleton - потому что InMemory!

//services.AddScoped<IEmployeesData, SqlEmployeesData>();
//services.AddScoped<IProductData, SqlProductData>(); // !!! AddScoped !!!
//services.AddScoped<IOrderService, SqlOrderService>();
services.AddScoped<ICartService, InCookiesCartService>();


//services.AddHttpClient<IValuesService, ValuesClient>(client => client.BaseAddress = new(configuration["WebAPI"]));
//services.AddHttpClient<IEmployeesData, EmployessClient>(client => client.BaseAddress = new(configuration["WebAPI"]));
//services.AddHttpClient<IProductData, ProductsClient>(client => client.BaseAddress = new(configuration["WebAPI"]));
//services.AddHttpClient<IOrderService, OrdersClient>(client => client.BaseAddress = new(configuration["WebAPI"]));


services.AddHttpClient("WebStoreApi", client => client.BaseAddress = new(configuration["WebAPI"]))
    .AddTypedClient<IValuesService, ValuesClient>()
    .AddTypedClient<IEmployeesData, EmployessClient>()
    .AddTypedClient<IProductData, ProductsClient>()
    .AddTypedClient<IOrderService, OrdersClient>();

services.AddAutoMapper(Assembly.GetEntryAssembly());

#endregion

var app = builder.Build(); // Сборка приложения

await using (var scope = app.Services.CreateAsyncScope())
{
    var db_initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await db_initializer.InitializeAsync(RemoveBefore: false).ConfigureAwait(true);
}

//app.Urls.Add("http://+:80"); // - если хочется обеспечить видимость приложения в локальной сети

#region Конфигурирование конвейера обработки входящих соединения

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

//app.Map("/testpath", async context => await context.Response.WriteAsync("Test middleware"));

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<TestMiddleware>();

app.UseWelcomePage("/welcome");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

#endregion

app.Run();
