using QuizTaker.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using QuizTaker.Data;
using QuizTaker.Services;

namespace QuizTaker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys")));
            builder.Services.AddDbContext<QuizTakerDbContext>(options =>
                options.UseSqlite($"Data Source={Path.Combine(builder.Environment.ContentRootPath, "quiztaker.db")}"));
            builder.Services.AddSingleton<QuizCatalogService>();
            builder.Services.AddSingleton<QuizSessionService>();
            builder.Services.AddScoped<ThemeState>();

            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<QuizTakerDbContext>();
                db.Database.EnsureCreated();
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
