var builder = WebApplication.CreateBuilder(args);

var application = builder.Build();

await application.RunAsync();