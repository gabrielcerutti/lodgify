using Figgle;
using Showtime.Api.Application.Behaviors;
using Showtime.Api.Application.Exceptions;
using Showtime.Api.Database;
using Showtime.Api.Database.Repositories;
using Showtime.Api.Database.Repositories.Abstractions;
using Showtime.Api.Infrastructure;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Serilog;
using FluentValidation;
using Showtime.Api.Application.Validations;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Console.WriteLine(FiggleFonts.Standard.Render("Showtime API"));
    var builder = WebApplication.CreateBuilder(args);
    Log.Information("Starting web application");    
    
    // Setup configuration sources.
    builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
    //builder.Host.UseSerilog();
    builder.Host.UseSerilog((context, services, configuration) => configuration.ReadFrom.Configuration(context.Configuration));
    //var logger = new LoggerConfiguration()
    //    .ReadFrom.Configuration(builder.Configuration)
    //    .CreateLogger();
    //builder.Logging.AddSerilog(logger);

    // Add services to the container
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
    builder.Services.AddControllers(options =>
    {
        options.Filters.Add(typeof(HttpGlobalExceptionFilter)); //This can also be achieved by using a middleware
    });
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddHttpClient();
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IShowtimesRepository>());
    builder.Services.AddValidatorsFromAssemblyContaining<CreateShowtimeCommandValidator>();
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TrackingBehavior<,>));
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidatorBehavior<,>));

    //Application middlewares pipeline setup
    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(options => 
        {
            options.SwaggerEndpoint("v1/swagger.json", "Showtime.API v1");
        });
    }
    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    SampleData.Initialize(app);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

