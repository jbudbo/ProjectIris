var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddRedis();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseServerSentEvents()!
    .Run();
