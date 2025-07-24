using CusotmFileUploader.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusotmFileUploader.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomeFileUploader(this IServiceCollection services)
        {
            services.AddScoped<IFileUploadService, FileUploadService>();
            return services;
        }

        public static IServiceCollection AddCustomeFileUploader<TContext>(this IServiceCollection services)

            where TContext : DbContext
        {
            services.AddScoped<DbContext, TContext>();
            services.AddScoped<IFileUploadService, FileUploadService>();
            return services;
        }
    }
}
