using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using SpendSmart.DatabaseContext;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var environment = builder.Environment;
var configuration = builder.Configuration;

services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    });

services.AddRazorPages();

if (environment.IsDevelopment()) configuration.AddUserSecrets<Program>();

services.AddSingleton<SpendSmartDynamoDbContext>();
services.AddAWSService<IAmazonDynamoDB>();
services.AddScoped<IDynamoDBContext, DynamoDBContext>();
services.AddScoped<DynamoDBContext, DynamoDBContext>();

services.AddHttpClient();

var app = builder.Build();
var serviceProvider = app.Services;

var dynamoDbContext = serviceProvider.GetRequiredService<SpendSmartDynamoDbContext>();
await dynamoDbContext.CreateTableIfNotExistsAsync("Users");
await dynamoDbContext.CreateTableIfNotExistsAsync("Transactions");
await dynamoDbContext.CreateTableIfNotExistsAsync("Accounts");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapGet("/", () => Results.Redirect("/dashboard"));

app.Run();
