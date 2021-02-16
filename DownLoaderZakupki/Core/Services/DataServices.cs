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
    internal class DataServices : IDataServices
    {
        private readonly IGovDbManager _govDb;
        private readonly ILogger _logger;


        public DataServices(IGovDbManager govDb, ILogger<DataServices> logger)
        {
            _govDb = govDb;
            _logger = logger;
        }

        #region NSI File Cash

        /// <summary>
        /// Получение списка файлов из кэша.
        /// </summary>
        /// <param name="lim"></param>
        /// <param name="status"></param>
        /// <param name="fz_type"></param>
        /// <param name="basepath"></param>
        /// <param name="dirtype"></param>
        /// <returns></returns>
        public List<NsiFileCashes> GetNsiDBList(int lim, Status status, FLType fz_type, string basepath, string dirtype)
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

        /// <summary>
        /// Обновление статусов кэша файлов 
        /// </summary>
        /// <param name="fileCashes"></param>
        public void UpdateNsiCasheFiles(NsiFileCashes fileCashes)
        {
            using (var db = _govDb.GetContext())
            {
                db.NsiFileCashes.Update(fileCashes);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Сохранение данных организации из справочника.
        /// </summary>
        /// <param name="nsiOrganizations"></param>
        public void SaveNsiOrgList(List<NsiOrganizations> nsiOrganizations)
        {

            foreach (var organization in nsiOrganizations)
            {
                using (var db = _govDb.GetContext())
                {
                    try
                    {
                        var find = db.NsiOrganizations
                        .AsNoTracking()
                        .Where(x => x.RegNumber == organization.RegNumber
                        && x.Fz_type == organization.Fz_type)
                        .SingleOrDefault();

                        if (find == null)
                        {
                            db.NsiOrganizations.Add(organization);
                            db.SaveChanges();
                        }
                        else
                        {
                            find.NsiData = organization.NsiData;
                            find.FullName = organization.FullName;
                            find.IsActual = organization.IsActual;
                            find.Inn = organization.Inn ?? string.Empty;
                            find.Kpp = organization.Kpp ?? string.Empty;
                            find.Ogrn = organization.Ogrn ?? string.Empty;
                            find.RegistrationDate = organization.RegistrationDate;
                            find.Accounts = organization.Accounts;
                            db.NsiOrganizations.Update(find);
                            db.SaveChanges();
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);
                    }
                }
            }
        }

        #endregion NSI File Cash

        #region Data File Cash
        /// <summary>
        /// Получение списка файлов на загрузку
        /// </summary>
        /// <param name="lim"></param>
        /// <param name="status"></param>
        /// <param name="fz_type"></param>
        /// <returns></returns>
        public List<FileCashes> GetDwList(int lim, Status status, FLType fz_type)
        {
            List<FileCashes> data = new List<FileCashes>();

            using (var db = _govDb.GetContext())
            {
                data = db.FileCashes
                    .AsNoTracking()
                    .Where(x => x.Status == status && x.Fz_type == fz_type)
                    .OrderByDescending(x => x.Date)
                    .Take(lim)
                    .ToList();
            }
            return data;
        }

        /// <summary>
        /// Проверка на наличие имеющейся записи о файле
        /// </summary>
        /// <param name="FullPath"></param>
        /// <returns></returns>
        public bool CheckCasheFiles(string FullPath)
        {
            FileCashes find = null;

            using (var db = _govDb.GetContext())
            {
                find = db.FileCashes
                    .AsNoTracking()
                    .Where(x => x.Full_path == FullPath)
                    .OrderByDescending(x => x.Date)
                    .FirstOrDefault();
            }
            if (find == null) return false;
            else return true;
        }

        /// <summary>
        /// Обновление данных по загрузке в кэше
        /// </summary>
        /// <param name="fileCashes"></param>
        public void UpdateCasheFiles(FileCashes fileCashes)
        {
            using (var db = _govDb.GetContext())
            {
                db.FileCashes.Update(fileCashes);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Удаление из кэша несуществующего/недоступного на ftp файла 
        /// </summary>
        /// <param name="fileCashes"></param>
        public void DeleteCasheFiles(FileCashes fileCashes)
        {
            using (var db = _govDb.GetContext())
            {
                db.FileCashes.Remove(fileCashes);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Получение списка файлов из кэша по извещениям и протоколам.
        /// </summary>
        /// <param name="lim"></param>
        /// <param name="status"></param>
        /// <param name="fz_type"></param>
        /// <param name="basepath"></param>
        /// <param name="dirtype"></param>
        /// <returns></returns>
        public List<FileCashes> GetFileCashesList(int lim, Status status, FLType fz_type, string basepath, string dirtype)
        {
            //throw new NotImplementedException();

            List<FileCashes> data = new List<FileCashes>();

            using (var db = _govDb.GetContext())
            {
                data = db.FileCashes
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

        /// <summary>
        /// Сохранение данных извещений.
        /// </summary>
        /// <param name="notifications"></param>
        public void SaveNotification(List<Notifications> notifications)
        {

            foreach (var notif in notifications)
            {
                using (var db = _govDb.GetContext())
                {
                    try
                    {
                        var find = db.Notifications
                        .AsNoTracking()
                        .Where(x => x.Purchase_num == notif.Purchase_num
                        && x.Fz_type == notif.Fz_type
                        && x.PublishDate == notif.PublishDate
                        && x.Hash == notif.Hash)
                        .SingleOrDefault();

                        if (find == null)
                        {
                            db.Notifications.Add(notif);
                            db.SaveChanges();
                        }
                        //else
                        //{
                        //    find.R_body = notif.R_body;
                        //    find.ContractConclusion = notif.ContractConclusion;
                        //    find.Wname = notif.Wname;
                        //    find.Inn = notif.Inn ?? string.Empty;
                        //    find.Zip_file = notif.Zip_file;
                        //    find.File_name = notif.File_name;
                        //    db.Notifications.Update(find);
                        //    db.SaveChanges();
                        //}

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);
                    }
                }
            }
        }
        /// <summary>
        /// Сохранение данных контрактов.
        /// </summary>
        /// <param name="contracts"></param>
        public void SaveContracts(List<Contracts> contracts)
        {
            foreach (var c in contracts)
            {
                using (var db = _govDb.GetContext())
                {
                    try
                    {
                        var find = db.Contracts
                        .AsNoTracking()
                        .Where(x => x.Contract_num == c.Contract_num
                        && x.Fz_type == c.Fz_type
                        && x.PublishDate == c.PublishDate
                        && x.Hash == c.Hash)
                        .SingleOrDefault();

                        if (find == null)
                        {
                            db.Contracts.Add(c);
                            db.SaveChanges();
                        }
                        

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);
                    }
                }
            }
        }
        #endregion Data File Cash

    }
}
