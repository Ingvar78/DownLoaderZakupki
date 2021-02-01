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

        /// <summary>
        /// Получение списка файлов на обработку.
        /// </summary>
        /// <param name="lim"></param>
        /// <param name="status"></param>
        /// <param name="fz_type"></param>
        /// <returns></returns>
        List<FileCashes> GetDwList(int lim, Status status, FLType fz_type);
        /// <summary>
        /// Проверка на наличие имеющейся записи о файле
        /// </summary>
        /// <param name="FullPath"></param>
        /// <returns></returns>
        bool CheckCasheFiles(string FullPath);
        /// <summary>
        /// Обновление данных по загрузке в кэше
        /// </summary>
        /// <param name="fileCashes"></param>
        void UpdateCasheFiles(FileCashes fileCashes);
        /// <summary>
        /// Удаление из кэша несуществующего/недоступного на ftp файла 
        /// </summary>
        /// <param name="fileCashes"></param>
        void DeleteCasheFiles(FileCashes fileCashes);

        /// <summary>
        /// Получение списка файлов из кэша по извещениям и протоколам.
        /// </summary>
        /// <param name="lim"></param>
        /// <param name="status"></param>
        /// <param name="fz_type"></param>
        /// <param name="basepath"></param>
        /// <param name="dirtype"></param>
        /// <returns></returns>
        List<FileCashes> GetFileCashesList(int lim, Status status, FLType fz_type, string basepath, string dirtype);

    }
}
