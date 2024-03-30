using Database.DTOs.DPTimeDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class DPTimeExtensions
    {
        public static void AddDPTimeExtensions(this IServiceCollection services)
        {
            services.AddScoped<IDayPartRepository, DayPartRepository>();
            services.AddAutoMapper(configuration => {
                configuration.CreateMap<DPTimeDTO, DPTime>().ReverseMap();
            });
        }
    }
}
