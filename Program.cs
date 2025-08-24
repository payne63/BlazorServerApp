using BlazorServerApp.Data;
using BlazorServerApp.Db;
using BlazorServerApp.Services;
using Radzen;

namespace BlazorServerApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
        builder.Services.AddSingleton<WeatherForecastService>();
        builder.Services.AddRadzenComponents();

        builder.Services.AddDbContext<DataBaseContext>();
        builder.Services.AddDbContext<PeopleContext>();
        builder.Services.AddScoped<DataBaseService>();
        builder.Services.AddScoped<Services.ThemeService>();
        builder.Services.AddScoped<EventConsoleService>();
        builder.Services.AddScoped<AccessDatabaseService>();
        builder.Services.AddScoped<LiteDbPeopleService>();
        builder.Services.AddScoped<LiteDbService>();
        // builder.Services.AddSingleton<EventConsoleService>();

        var app = builder.Build();

        // Ensure the People database is created on startup
        using (var scope = app.Services.CreateScope())
        {
            var peopleContext = scope.ServiceProvider.GetRequiredService<PeopleContext>();
            peopleContext.Database.EnsureCreated();
        }

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //app.UseHsts();
        }

        //app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");
        app.Run();
    }
}