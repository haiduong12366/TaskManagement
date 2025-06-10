using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.OpenApi.Models;
using System.Globalization;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ManagementSystem.WSAPI.MiddleWare;
using Microsoft.Extensions.Options;

namespace ManagementSystem.WSAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                services.AddCors(c =>
                {
                    c.AddPolicy("AllowOrigin",
                        policy =>
                        {
                            var allowedOrigins = Configuration["Cors:AllowedOrigins"];
                            if (string.IsNullOrEmpty(allowedOrigins) || allowedOrigins == "*")
                            {
                                policy.AllowAnyHeader()
                                .AllowAnyMethod()//.WithMethods("GET", "POST") 
                                .SetIsOriginAllowed(origin => true) // Cho phép tất cả các domain
                                .AllowCredentials();
                            }
                            else
                            {
                                policy.WithOrigins(allowedOrigins?.Split(",") ?? Array.Empty<string>()) // Cho phép các domain cụ thể
                                      .AllowAnyHeader()
                                      .AllowAnyMethod()
                                      .AllowCredentials();
                            }
                        });


                });
                services.AddControllers().AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
                });
                services.AddSignalR(hubOptions =>
                {
                    hubOptions.EnableDetailedErrors = true;
                    //hubOptions.MaximumReceiveMessageSize = 10240; // default 32Kb
                }).AddJsonProtocol();

                services.Configure<GzipCompressionProviderOptions>(options =>
                {
                    options.Level = CompressionLevel.Fastest;
                });
                services.AddResponseCompression(options =>
                {
                    options.EnableForHttps = true;
                    options.Providers.Add<GzipCompressionProvider>();
                });
                services.Configure<IISServerOptions>(options =>
                {
                    options.AutomaticAuthentication = false;
                });
                //services.AddSingleton<API.Middleware.ExceptionHandlingMiddleware>();
                //services.AddAuthorizationServices(Configuration);
                //services.AddAuthorizationFunctionRequirementServices(Configuration);
                services.AddHttpContextAccessor();

                #region Swagger
                // Register the Swagger generator, defining 1 or more Swagger documents
                services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo()
                    {
                        Title = "Haiduong",
                        Version = "v1",
                    });
                    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = JwtBearerDefaults.AuthenticationScheme //"Bearer"
                    });
                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme,
                                        //Id = JwtBearerDefaults.AuthenticationScheme //"Bearer"
                                        Id = "Bearer"
                                    }
                            },
                        Array.Empty<string>()
                        }
                    });
                    options.EnableAnnotations();

                });
                #endregion

                #region Add DI
                services.AddDependencyService();
                #endregion

            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
                throw;
            }
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            try
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                    app.UseExceptionHandler("/Error");
                }
                else
                {
                    app.UseExceptionHandler("/Error");
                    app.UseHsts();
                }

                app.UseRouting();

                //swapger
                app.UseSwagger();

                // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
                // specifying the Swagger JSON endpoint.
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HaiDuong API");
                    //c.RoutePrefix = string.Empty; // Để Swagger chạy ở đường dẫn gốc
                });

                //middleware này để chuyển url /api/tat/ về /api/. Khỏi mắc công các controller thêm /tat/
                //app.Use(async (context, next) =>
                //{
                //    var path = context.Request.Path.Value;

                //    if (!string.IsNullOrEmpty(path) && path.StartsWith("/api/crm/"))
                //    {
                //        // Chuyển hướng request về URL đúng
                //        var newPath = path.Replace("/api/crm/", "/api/");
                //        context.Request.Path = newPath;
                //    }

                //    await next(context);
                //});

                app.UseCors("AllowOrigin");
                //app.UseCors(x => x
                //    .AllowAnyOrigin()
                //    .AllowAnyMethod()
                //    .AllowAnyHeader());
                app.UseAuthentication();
                app.UseAuthorization();
                //app.UseMiddleware<ApiRequestMiddleware>();
                app.UseHttpsRedirection();
                app.UseStaticFiles();

                //mapping API enpoint
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    //endpoints.MapHub<Watcher>("/notify");

                });
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
                throw;
            }
        }

        public sealed class DateTimeJsonConverter : JsonConverter<DateTime>
        {
            private static readonly string[] AcceptedFormats = new[]
            {
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd HH:mm",
            "yyyy-MM-dd",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ssZ",
            "yyyy-MM-ddTHH:mm:ss.fffZ",
            "yyyy-MM-ddTHH:mm:ss.fff", // ISO 8601 with milliseconds
        };

            private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                string? str = reader.GetString();
                if (string.IsNullOrWhiteSpace(str))
                    throw new JsonException("Date string is null or empty.");

                if (DateTime.TryParseExact(str, AcceptedFormats, Invariant, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime result))
                    return result;

                // fallback to TryParse if formats above fail
                if (DateTime.TryParse(str, Invariant, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out result))
                    return result;

                throw new JsonException($"Cannot parse \"{str}\" to DateTime.");
            }

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                // Xuất DateTime theo định dạng chuẩn nhất
                writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss", Invariant));
            }
        }
    }
}
