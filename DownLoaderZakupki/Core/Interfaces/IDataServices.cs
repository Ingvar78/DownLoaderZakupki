using DownLoaderZakupki.Data.DB;
using DownLoaderZakupki.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownLoaderZakupki.Core.Interfaces
{
    public interface IDataServices
    {
        List<NsiFileCashes> GetNsiDBList(int lim, Status status, FLType fz_type, string basepath, string dirtype);
        public void UpdateNsiCasheFiles(NsiFileCashes fileCashes);
        public void SaveNsiOrgList(List<NsiOrganizations> nsiOrganizations);

    }
}
