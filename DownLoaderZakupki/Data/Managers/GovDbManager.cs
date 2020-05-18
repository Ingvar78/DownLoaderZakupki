using DownLoaderZakupki.Configurations;
using DownLoaderZakupki.Core.Interfaces;
using DownLoaderZakupki.Data.Access;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace DownLoaderZakupki.Data.Managers
{
    internal class GovDbManager :IGovDbManager
    {
        private readonly string _govDbConnectionString;
        private readonly ILoggerFactory _loggerFactory;
        public GovDbManager(IOptions<ConnectionDB> settings, ILoggerFactory loggerFactory)
        {
            _govDbConnectionString = settings.Value.ConnectionGDB;
            _loggerFactory = loggerFactory;
        }

        void IDbManager.InitDb()
        {
            DbContextManager.InitGovDb(_govDbConnectionString, _loggerFactory);
        }
        public IGovDbContext GetContext()
        {
            return DbContextManager.CreateGovContext(_govDbConnectionString, _loggerFactory);
        }

    }
}
