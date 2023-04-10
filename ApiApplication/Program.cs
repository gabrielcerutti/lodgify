using Lodgify.Api.Database;
using Lodgify.Api.Database.Repositories;
using Lodgify.Api.Database.Repositories.Abstractions;
using Lodgify.Api.Infrastructure;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Setup configuration sources.
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());

builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddTransient<IShowtimesRepository, ShowtimesRepository>();
builder.Services.AddTransient<ITicketsRepository, TicketsRepository>();
builder.Services.AddTransient<IAuditoriumsRepository, AuditoriumsRepository>();
builder.Services.AddDbContext<CinemaContext>(options =>
{
    options.UseInMemoryDatabase("CinemaDb")
        .EnableSensitiveDataLogging()
        .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning));
});
builder.Services.AddSingleton<ICache, RedisCache>();
builder.Services.AddSingleton<IMoviesApi, MoviesApi>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IShowtimesRepository>());

//Application middlewares pipeline setup
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

SampleData.Initialize(app);

app.Run();
