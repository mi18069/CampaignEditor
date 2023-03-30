using Database.DTOs.SchemaDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class SchemaExtensions
    {
        public static void AddSchemaExtensions(this IServiceCollection services)
        {
            services.AddScoped<ISchemaRepository, SchemaRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<SchemaDTO, Schema>().ReverseMap();
            });

        }
    }
}
