using DownLoaderZakupki.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DownLoaderZakupki.Data.DB;
using DownLoaderZakupki.Models.Enum;
using Microsoft.EntityFrameworkCore;

namespace DownLoaderZakupki.Core.Services
{
    internal class DataServices: IDataServices
    {
        private readonly IGovDbManager _govDb;
        private readonly ILogger _logger;


        public DataServices(IGovDbManager govDb, ILogger<DataServices> logger)
        {
            _govDb = govDb;
            _logger = logger;
        }
        public List<NsiFileCashes> GetDBList1(int lim, Status status, FLType fz_type, string basepath, string dirtype)
        {
            //throw new NotImplementedException();

            List<NsiFileCashes> data = new List<NsiFileCashes>();

            using (var db = _govDb.GetContext())
            {
                data = db.NsiFileCashes
                    .AsNoTracking()
                    .Where(x => x.Status == status
                    && x.Fz_type == fz_type
                    && x.BaseDir == basepath
                    && x.Dirtype == dirtype)
                    .OrderBy(x => x.Date)
                    //.OrderByDescending(x => x.Date)
                    .Take(lim)
                    .ToList();
            }
            return data;
        }

        public void UpdateCasheFiles(NsiFileCashes fileCashes)
        {
            using (var db = _govDb.GetContext())
            {
                db.NsiFileCashes.Update(fileCashes);
                db.SaveChanges();
            }
        }
    }
}
