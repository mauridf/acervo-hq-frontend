using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using acervo_hq_frontend.Data;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Register HttpClient
builder.Services.AddHttpClient<EditoraService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiUrl"]);
});

builder.Services.AddHttpClient<PersonagemService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiUrl"]);
});

builder.Services.AddHttpClient<HqService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiUrl"]);
});

builder.Services.AddHttpClient<EdicaoService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiUrl"]);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();