using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class CompletedValidationExtensions
    {
        public static void AddCompletedValidationExtensions(this IServiceCollection services)
        {
            services.AddScoped<ICompletedValidationRepository, CompletedValidationRepository>();

        }
    }
}
