using Tortis.Iam.Oidc;

var builder = WebApplication.CreateBuilder(args);

builder.AddTortisIamOidc();

var app = builder.Build();

app.UseRouting();
app.MapGet("/", () => "Hello World!");

app.Run();