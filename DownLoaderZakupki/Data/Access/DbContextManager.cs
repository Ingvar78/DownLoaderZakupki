using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DownLoaderZakupki.Data.Access
{
    public class DbContextManager
    {
        public static IGovDbContext CreateGovContext(string connectionString, ILoggerFactory loggerFactory)
        {
            return new GovDbContext(connectionString, loggerFactory);
        }

	
        public static void InitGovDb(string connectionString, ILoggerFactory loggerFactory)
        {
            try
            {
            }
            catch (Exception ex)
            {
                loggerFactory.CreateLogger<DbContextManager>().LogCritical(ex, ex.Message);
                throw ex;
            }
        }
    }
}
