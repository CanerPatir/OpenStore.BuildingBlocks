using System.Globalization;
using System.Reflection;
using OpenStore.Infrastructure.Localization;


var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);
builder.Services.AddControllersWithViews();

var mvcBuilder = builder.Services.AddControllersWithViews();

builder.Services.AddOpenStoreResxLocalization(mvcBuilder, options =>
{
    options.Assembly = Assembly.GetEntryAssembly();
    options.DefaultUiCulture = new CultureInfo("tr-TR");
});

var app = builder.Build();

app.UseDeveloperExceptionPage();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseOpenStoreLocalization();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();