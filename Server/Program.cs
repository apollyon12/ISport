using iSportsRecruiting.Server.Services;
using iSportsRecruiting.Shared.Services;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(cors =>
    cors.AddDefaultPolicy(policyBuilder => policyBuilder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddControllersWithViews(o => o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
builder.Services.AddRazorPages();
builder.Services.AddResponseCompression();
builder.Services.AddResponseCaching();
builder.Services.AddScoped<IDatabaseContext, DatabaseContext>();
builder.Services.AddHostedService<PaymentsHostedService>();
builder.Services.AddHostedService<NotificationsHostedService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseResponseCompression();

app.UseResponseCaching();
app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();