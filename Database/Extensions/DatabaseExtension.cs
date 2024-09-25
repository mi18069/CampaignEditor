using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Extensions
{
    public static class DatabaseExtension
    {
        public static int SaveChanges(this IQueryable queryable)
        {
            int numberOfTries = 3;
            int.TryParse(Environment.GetEnvironmentVariable("DATABASE_RETRY"), out numberOfTries);
            int numberOfRows = 0;
            try
            {
                for(int i = 0; i < numberOfTries; i++)
                {

                }
                numberOfRows = queryable.SaveChanges();
            } catch (Exception ex)
            {

            }
            return numberOfRows;
        }
    }
}
