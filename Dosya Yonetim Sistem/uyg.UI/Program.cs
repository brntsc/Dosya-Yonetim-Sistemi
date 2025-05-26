var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.Secure = CookieSecurePolicy.Always;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseCookiePolicy();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "files",
    pattern: "Files/{catId?}",
    defaults: new { controller = "Admin", action = "Files" });

app.MapControllerRoute(
    name: "categories",
    pattern: "Categories",
    defaults: new { controller = "Admin", action = "Categories" });

app.MapControllerRoute(
    name: "filetag",
    pattern: "FileTag",
    defaults: new { controller = "Admin", action = "FileTag" });

app.MapControllerRoute(
    name: "profile",
    pattern: "Profile",
    defaults: new { controller = "Admin", action = "Profile" });

app.MapControllerRoute(
    name: "login",
    pattern: "Login",
    defaults: new { controller = "Admin", action = "Login" });

app.MapControllerRoute(
    name: "privacy",
    pattern: "Privacy",
    defaults: new { controller = "Admin", action = "Privacy" });

app.Run();
