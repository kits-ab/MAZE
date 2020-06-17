using System.Text;
using MAZE.Api.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;

namespace MAZE.Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter(new CamelCaseNamingStrategy()));
                });

            services.AddCors();

            services.AddSignalR()
                .AddAzureSignalR();

            var tokenSecret = _configuration.GetValue<string>("TokenSecret");
            var key = Encoding.ASCII.GetBytes(tokenSecret);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = false,
                    };
                });

            services.AddTransient<TokenFactory>();

            services.AddTransient<WorldSerializer>();
            services.AddSingleton<EventRepository>();
            services.AddSingleton<GameRepository>();

            services.AddTransient<GameService>();
            services.AddTransient<LocationService>();
            services.AddTransient<PathService>();
            services.AddTransient<ObstacleService>();
            services.AddTransient<GameEventService>();
            services.AddTransient<CharacterService>();
            services.AddTransient<AvailableMovementsFactory>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(options =>
            {
                if (env.IsDevelopment())
                {
                    options.WithOrigins("https://localhost:44320");
                }
                else
                {
                    options.WithOrigins("https://maze-client.azurewebsites.net");
                }

                options.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi", "MAZE API");
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseAzureSignalR(routes =>
            {
                routes.MapHub<GameHub>("/gameEvents");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
