using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Extensions
{
    public static class ClientRealizedCoefsExtensions
    {
        public static void AddClientRealizedCoefsExtensions(this IServiceCollection services)
        {
            services.AddScoped<IClientRealizedCoefsRepository, ClientRealizedCoefsRepository>();

        }
    }
}
