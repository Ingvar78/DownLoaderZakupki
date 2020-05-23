using DownLoaderZakupki.Configurations;
using DownLoaderZakupki.Core.Interfaces;
using DownLoaderZakupki.Data.DB;
using FluentFTP;
using FluentScheduler;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DownLoaderZakupki.Core.Jobs
{
    internal class UploadNsiFilesJob : IJob
    {
        private readonly CommonSettings _commonSettings;
        private readonly NsiSettings44 _nsiSettings44;
        private readonly NsiSettings223 _nsiSettings223;
        private readonly ILogger _logger;
        private readonly IGovDbManager _govDb;
        private readonly string _path;

        public UploadNsiFilesJob(CommonSettings commonSettings,
            NsiSettings44 nsiSettings44,
            NsiSettings223 nsiSettings223,
            IGovDbManager govDb,
            ILogger logger
            )
        {
            _commonSettings = commonSettings;
            _nsiSettings44 = nsiSettings44;
            _nsiSettings223 = nsiSettings223;
            _govDb = govDb;
            _logger = logger;
            _path = commonSettings.BasePath;
        }


        void IJob.Execute()
        {
            try
            {
                Parallel.Invoke(
                    () => { GetNSIListFTP44(); },
                    () => { DownloadFtpFiles44(GetDBList(1000, 1, 44)); },                    
                    () => { GetNSIListFTP223(); },
                    () => { DownloadFtpFiles223(GetDBList(1000, 1, 223)); }
                    );

                var cnt44 = GetDBList(1000, 1, 44).Count;
                var cnt223 = GetDBList(1000, 1, 223).Count;

                //Грузить пока не устанет
                while (cnt44 > 0 || cnt223 > 0)
                {
                    //2. Загрузка справочников 
                    //44ФЗ/223ФЗ
                    Parallel.Invoke(
                        () => { DownloadFtpFiles44(GetDBList(1000, 1, 44)); },
                        () => { DownloadFtpFiles223(GetDBList(1000, 1, 223)); }
                        );

                    cnt44 = GetDBList(1000, 1, 44).Count;
                    cnt223 = GetDBList(1000, 1, 223).Count;
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }


        private void GetNSIListFTP44()
        {
            DateTime StartDate = DateTime.Now;
            var basedir44 = _nsiSettings44.BaseDir;
            _logger.LogInformation($"connect to ftp 44, Начало создания списка справочников NSI в {StartDate}"); ;
            try
            {
                //TODO
                //FtpClient client = new FtpClient("ftp.zakupki.gov.ru");
                FtpClient client = new FtpClient(_commonSettings.FtpCredential.FZ44.Url)
                {
                    Credentials = new NetworkCredential(_commonSettings.FtpCredential.FZ44.Login, _commonSettings.FtpCredential.FZ44.Password)
                };
                //Дата модификации/создания
                DateTime ModDate = DateTime.ParseExact(_commonSettings.StartDate, "yyyy-MM-dd",
                                           System.Globalization.CultureInfo.InvariantCulture);
                var ftpBasePath = $"/{basedir44}/";
                var dayyear = DateTime.Now.ToShortDateString();
                foreach (string DirsDoc in _nsiSettings44.DocDirList)
                {
                    try
                    {
                        client.Connect();
                        //_logger.LogInformation("connect to ftp 44, region for download: " + region);
                        var ftpPath = $"/{basedir44}/{DirsDoc}/";
                        var fileList = client.GetListing(ftpPath, FtpListOption.Recursive);
                        var ftpList = fileList.Where(item => item.Size > _nsiSettings44.EmptyZipSize && item.Type == FtpFileSystemObjectType.File && item.Modified > ModDate).ToList();
                        //ToDo Реализовать обработку списка файлов, через кэширование записей. 
                        //1. Загрузить список файлов. 
                        //2. проверить загружался ли, если нет загружаем. 
                        //3. выдать топ 100 файлов на загрузку 
                        //4. Выдать топ 100 загруженных zip но не обработанных файлов.
                        //5. Обработанные архивы фтопку. 
                        //ToDo Save ListFTP
                        SaveFTPPath(ftpList, DirsDoc, basedir44, 1, 44);
                        //DownloadFTPRegion(GetDBList(100, 1, 44));
                        _logger.LogInformation($"Создан список файлов справочников для загрузки: {basedir44} { DirsDoc} 44ФЗ");
                        client.Disconnect();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);
                        _logger.LogInformation($"Ошибка создания списка файлов справочников для загрузки: { basedir44} /{ DirsDoc} 44ФЗ");
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            DateTime EndDate = DateTime.Now;
            _logger.LogInformation($"connect to ftp 44, Список файлов справочников NSI создан в {EndDate}, время на создание списка {(EndDate - StartDate).TotalSeconds} секунд/ {(EndDate - StartDate).TotalMinutes} минут");
        }

        private void GetNSIListFTP223()
        {
            DateTime StartDate = DateTime.Now;
            var basedir223 = _nsiSettings223.BaseDir;
            _logger.LogInformation($"connect to ftp 223, Начало создания списка в {StartDate}");
            try
            {
                //TODO
                //FtpClient client = new FtpClient("ftp.zakupki.gov.ru");
                FtpClient client = new FtpClient(_commonSettings.FtpCredential.FZ223.Url)
                {
                    Credentials = new NetworkCredential(_commonSettings.FtpCredential.FZ223.Login, _commonSettings.FtpCredential.FZ223.Password)
                };

                //Список регионов
                //Дата модификации/создания
                DateTime ModDate = DateTime.ParseExact(_commonSettings.StartDate, "yyyy-MM-dd",
                                           System.Globalization.CultureInfo.InvariantCulture);

                var ftpBasePath = $"{basedir223}";
                var dayyear = DateTime.Now.ToShortDateString();
                foreach (string DirsDoc in _nsiSettings223.DocDirList)
                {
                    try
                    {
                        client.Connect();
                        var ftpPath = $"{basedir223}/{DirsDoc}/";
                        var fileList = client.GetListing(ftpPath, FtpListOption.Recursive);
                        var ftpList = fileList.Where(item => item.Size > _nsiSettings223.EmptyZipSize && item.Type == FtpFileSystemObjectType.File && item.Modified > ModDate).ToList();
                        //ToDo Save ListFTP
                        SaveFTPPath(ftpList, DirsDoc, basedir223, 1, 223);
                        _logger.LogInformation($"Создан список файлов справочников NSI для загрузки: {basedir223} /{DirsDoc} 223ФЗ");
                        client.Disconnect();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation($"Ошибка создания списка файлов справочников NSI  для загрузки: {basedir223} /{ DirsDoc} 223ФЗ");
                        _logger.LogError(ex, ex.Message);
                    }
                }


                basedir223 = _nsiSettings223.NsiVSRZ;
                ftpBasePath = $"{basedir223}";
                try
                {
                    client.Connect();
                    var ftpPath = $"{basedir223}";
                    var fileList = client.GetListing(ftpPath, FtpListOption.Recursive);
                    var ftpList = fileList.Where(item => item.Size > _nsiSettings223.EmptyZipSize && item.Type == FtpFileSystemObjectType.File && item.Modified > ModDate).ToList();
                    //ToDo Save ListFTP
                    SaveFTPPath(ftpList, "nsiVSRZ_CSV", basedir223, 1, 223);
                    _logger.LogInformation($"Создан список файлов справочников площадок для загрузки: {basedir223} / nsiVSRZ_CSV 223ФЗ");
                    client.Disconnect();
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Ошибка создания списка файлов справочников площадок для загрузки: {basedir223} / nsiVSRZ_CSV 223ФЗ");
                    _logger.LogError(ex, ex.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            DateTime EndDate = DateTime.Now;
            _logger.LogInformation($"connect to ftp 223, Список файлов создан в {EndDate}, время на создание списка {(EndDate - StartDate).TotalSeconds} секунд/ {(EndDate - StartDate).TotalMinutes} минут");
        }
        private void SaveFTPPath(List<FtpListItem> ListFile, string ftpDir, string baseDir, int status, int fz)
        {
            foreach (FtpListItem item in ListFile)
            {
                if (!GetDBfile(item.FullName))
                {
                    var filesave = new NsiFileCashes();
                    filesave.Date = item.Modified;
                    filesave.Size = item.Size;
                    filesave.Full_path = item.FullName;
                    filesave.Zip_file = item.Name;
                    filesave.BaseDir = baseDir;
                    filesave.Dirtype = ftpDir;
                    filesave.Fz_type = fz;
                    filesave.Status = status;
                    filesave.Modifid_date = DateTime.Now;

                    SavePath(filesave);
                }
            }
        }

        bool GetDBfile(string FullPath)
        {
            NsiFileCashes find = null;

            using (var db = _govDb.GetContext())
            {
                find = db.NsiFileCashes
                    .AsNoTracking()
                    .Where(x => x.Full_path == FullPath)
                    .OrderByDescending(x => x.Date)
                    .FirstOrDefault();
            }
            if (find == null) return false;
            else return true;
        }

        private void SavePath(NsiFileCashes item)
        {
            try
            {
                using (var db = _govDb.GetContext())
                {
                    db.NsiFileCashes.Add(item);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }


        }

        List<NsiFileCashes> GetDBList(int lim, int status, int fz_type)
        {
            List<NsiFileCashes> data = new List<NsiFileCashes>();

            using (var db = _govDb.GetContext())
            {
                data = db.NsiFileCashes
                    .AsNoTracking()
                    .Where(x => x.Status == status && x.Fz_type == fz_type)
                    .OrderByDescending(x => x.Date)
                    .Take(lim)
                    .ToList();
            }
            return data;
        }


        private void DownloadFtpFiles44(List<NsiFileCashes> fileCashes)
        {
            DateTime StartDate = DateTime.Now;
            _logger.LogInformation($"Начало загрузки {fileCashes.Count} архивов NSI FZ44 {StartDate}...");

            var parallelOptions = new ParallelOptions()
            {
                //MaxDegreeOfParallelism = 1,
                MaxDegreeOfParallelism = _nsiSettings44.Parallels,
            };

            Parallel.ForEach(fileCashes, parallelOptions, item =>
            {
                FtpClient client = new FtpClient(_commonSettings.FtpCredential.FZ44.Url)
                {
                    Credentials = new NetworkCredential(_commonSettings.FtpCredential.FZ44.Login, _commonSettings.FtpCredential.FZ44.Password),
                    RetryAttempts = 5
                };

                try
                {

                    client.Connect();

                    _logger.LogInformation($"Загрузка архива NSI FZ44 {item.Full_path}...");
                    client.DownloadFile(_nsiSettings44.WorkPath + item.Full_path, item.Full_path);
                    item.Modifid_date = DateTime.Now;
                    item.Status = 2;
                    UpdateCasheFiles(item);
                }
                catch (Exception ex)
                {
                    //Удаляем т.к. файл мог быть перемещён в рамках WorkFlow закупок, при следующей попытке файл станет в очередь. 
                    DeleteDBfile(item);
                    _logger.LogError(ex, $"Ошибка скачивания архива NSI FZ44 файл перемещён или недоступен: {item.Full_path}");
                    _logger.LogError(ex, ex.Message);
                }
                finally
                {
                    client.Disconnect();
                }
            });

            DateTime EndDate = DateTime.Now;
            _logger.LogInformation($"Загружено {fileCashes.Count} архивов NSI FZ44 {EndDate}... Время загрузки {(EndDate - StartDate).TotalMinutes} минут");
        }

        private void DownloadFtpFiles223(List<NsiFileCashes> fileCashes)
        {
            DateTime StartDate = DateTime.Now;
            _logger.LogInformation($"Начало загрузки {fileCashes.Count} архивов FZ223 {StartDate}...");

            var parallelOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = _nsiSettings223.Parallels,
            };

            Parallel.ForEach(fileCashes, parallelOptions, item =>
            {
                FtpClient client = new FtpClient(_commonSettings.FtpCredential.FZ223.Url)
                {
                    Credentials = new NetworkCredential(_commonSettings.FtpCredential.FZ223.Login, _commonSettings.FtpCredential.FZ223.Password),
                    RetryAttempts = 5
                };

                try
                {

                    client.Connect();

                    _logger.LogInformation($"Загрузка архива NSI FZ223 {item.Full_path}...");
                    client.DownloadFile(_nsiSettings223.WorkPath + item.Full_path, item.Full_path);
                    item.Modifid_date = DateTime.Now;
                    item.Status = 2;
                    UpdateCasheFiles(item);
                }
                catch (Exception ex)
                {
                    //Удаляем т.к. файл мог быть перемещён в рамках WorkFlow закупок, при следующей попытке файл станет в очередь. 
                    DeleteDBfile(item);
                    _logger.LogError(ex, $"Ошибка скачивания архива NSI FZ223 файл перемещён или недоступен: {item.Full_path}");
                    _logger.LogError(ex, ex.Message);
                }
                finally
                {
                    client.Disconnect();
                }
            });

            DateTime EndDate = DateTime.Now;
            _logger.LogInformation($"Загружено {fileCashes.Count} архивов FZ223 {EndDate}... Время загрузки {(EndDate - StartDate).TotalMinutes} минут");

        }
        private void UpdateCasheFiles(NsiFileCashes fileCashes)
        {
            using (var db = _govDb.GetContext())
            {
                db.NsiFileCashes.Update(fileCashes);
                db.SaveChanges();
            }
        }

        private void DeleteDBfile(NsiFileCashes fileCashes)
        {
            using (var db = _govDb.GetContext())
            {
                db.NsiFileCashes.Remove(fileCashes);
                db.SaveChanges();
            }
        }
    }
}
