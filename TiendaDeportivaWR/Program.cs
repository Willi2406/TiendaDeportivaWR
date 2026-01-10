using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TiendaDeportivaWR.Components;
using TiendaDeportivaWR.DAL;
using TiendaDeportivaWR.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


var connectionString = builder.Configuration.GetConnectionString("ConStr");

builder.Services.AddDbContextFactory<Contexto>(options =>
    options.UseSqlite(connectionString));


builder.Services.AddIdentityCore<IdentityUser>()
    .AddEntityFrameworkStores<Contexto>();


builder.Services.AddScoped<EntradaServices>();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var contextFactory = services.GetRequiredService<IDbContextFactory<Contexto>>();
    using var context = contextFactory.CreateDbContext();
    context.Database.EnsureCreated();
}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();