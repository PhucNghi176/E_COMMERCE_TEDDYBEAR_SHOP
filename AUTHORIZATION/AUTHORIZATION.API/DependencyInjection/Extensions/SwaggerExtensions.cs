﻿using CONTRACT.CONTRACT.API.DependencyInjection.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace AUTHORIZATION.API.DependencyInjection.Extensions;
public static class SwaggerExtensions
{
    public static void AddSwaggerAPI1(this IServiceCollection services)
    {
        _ = services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Description = @"JWT Authorization header using the Bearer scheme. 

Enter your token in the text input below.

Example: 'Bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        },
                        Scheme = "oauth2",
                        Name = JwtBearerDefaults.AuthenticationScheme,
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });

            // c.OperationFilter<SwaggerFormDataOperationFilter>();

            // c.OperationFilter<FileUploadOperationFilter>();

            c.EnableAnnotations();

            // c.UseOneOfForPolymorphism();
        });
        _ = services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
    }


    public static void UseSwaggerAPI1(this WebApplication app)
    {
        _ = app.UseSwagger();
        _ = app.UseSwaggerUI(options =>
        {
            foreach (var version in app.DescribeApiVersions().Select(version => version.GroupName))
                options.SwaggerEndpoint($"/swagger/{version}/swagger.json", version);

            options.EnableFilter();
            options.DisplayRequestDuration();
            options.EnableTryItOutByDefault();
            options.DocExpansion(DocExpansion.None);
        });

        _ = app.MapGet("/", () => Results.Redirect("/swagger/index.html"))
            .WithTags(string.Empty);
    }
}