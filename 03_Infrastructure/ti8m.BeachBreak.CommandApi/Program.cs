using ti8m.BeachBreak.CommandApi.Services;

namespace ti8m.BeachBreak.CommandApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.AddServiceDefaults();

            builder.Services.AddCors(
                options => options.AddDefaultPolicy(
                    policy => policy.WithOrigins([builder.Configuration["BackendUrl"] ?? "https://localhost:5001",
                                builder.Configuration["FrontendUrl"] ?? "https://localhost:5002"])
                        .AllowAnyMethod()
                        .AllowAnyHeader()));

            // Add services to the container.
            builder.Services.AddControllers();
            
            // Add custom services
            builder.Services.AddSingleton<IQuestionnaireService, QuestionnaireService>();
            
            // Configure CORS for frontend connection
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins("https://localhost:5001", "http://localhost:5001", "http://localhost:5000", "https://localhost:7000")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            // Add Swagger for API documentation
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.MapDefaultEndpoints();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
