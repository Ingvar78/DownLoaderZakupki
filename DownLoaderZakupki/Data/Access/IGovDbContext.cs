using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DownLoaderZakupki.Data.DB;

namespace DownLoaderZakupki.Data.Access
{
    public interface IGovDbContext : IDisposable
    {
        /// <summary>
        /// Кэш файлов справочников
        /// </summary>
        DbSet<FileCash> FileCashes { get; set; }
     
        int SaveChanges();
    }
}
