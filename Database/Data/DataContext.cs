using Microsoft.Extensions.Configuration;
using Npgsql;
using System;

namespace Database.Data
{
    public class DataContext : IDataContext
    {
        private readonly IConfiguration _configuration;

        public DataContext(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public NpgsqlConnection GetConnection()
        {
            string connectionString = "Server=localhost;port=5433;user id=postgres;password=jovan1999;database=mplaner;";
            //string connectionString = "Server=localhost;port=5432;user id=postgres;password=Peca;database=mplaner;";
            //return new NpgsqlConnection(_configuration.GetConnectionString(connectionString)); // Check this
            var connection = new NpgsqlConnection();
            connection.ConnectionString = connectionString;
            return connection;
        }
    }
}
