using Npgsql;

namespace CampaignEditor
{
    public class PgHelper
    {
        NpgsqlConnection cn;
        public PgHelper(string connectionString)
        {
            cn = new NpgsqlConnection(connectionString);
        }

        public bool isConnection
        {
            get
            {
                if (cn.State == System.Data.ConnectionState.Closed)
                    cn.Open();
                return true;
            }
        }
    }
}
