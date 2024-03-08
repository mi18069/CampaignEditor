using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Text;

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
            var encryptedString = AppSettings.ConnectionString;
            var connection = new NpgsqlConnection();
            var connectionString = EncryptionUtility.DecryptString(encryptedString);
            connection.ConnectionString = connectionString;
            return connection;
        }
    }
}
