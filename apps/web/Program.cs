using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Web.DermaImage.Components;
using Web.DermaImage.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Auth state provider
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AuthenticatedHttpClientHandler>();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/account/login";
        options.AccessDeniedPath = "/account/login";
    });
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

// Register HttpClient with the auth handler
builder.Services.AddScoped<HttpClient>(sp =>
{
    var handler = sp.GetRequiredService<AuthenticatedHttpClientHandler>();
    handler.InnerHandler = new HttpClientHandler();
    return new HttpClient(handler)
    {
        BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5131")
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
