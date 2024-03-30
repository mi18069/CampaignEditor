using Database.DTOs.DayPartDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class DayPartExtensions
    {
        public static void AddDayPartExtensions(this IServiceCollection services)
        {
            services.AddScoped<IDayPartRepository, DayPartRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<DayPartDTO, DayPart>().ReverseMap();
            });
        }
    }
}
