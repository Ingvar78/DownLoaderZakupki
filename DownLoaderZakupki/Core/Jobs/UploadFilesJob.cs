using FluentFTP;
using FluentScheduler;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DownLoaderZakupki.Configurations;
using DownLoaderZakupki.Core.Interfaces;
using DownLoaderZakupki.Data.DB;
using System.IO;
using Newtonsoft.Json;
using DownLoaderZakupki.Models.Enum;

namespace DownLoaderZakupki.Core.Jobs
{
    internal class UploadFilesJob : IJob
    {
        private readonly CommonSettings _commonSettings;
        private readonly FZSettings44 _fzSettings44;
        private readonly FZSettings223 _fzSettings223;
        private readonly IGovDbManager _govDb;
        private readonly ILogger _logger;
        private readonly string _path;
        private readonly IDataServices _dataServices;
        public UploadFilesJob(CommonSettings commonSettings,
            FZSettings44 fzSettings44,
            FZSettings223 fzSettings223,
            IGovDbManager govDb,
            ILogger logger,
            IDataServices getDataServices
            )
        {
            _commonSettings = commonSettings;
            _fzSettings44 = fzSettings44;
            _fzSettings223 = fzSettings223;
            _govDb = govDb;
            _logger = logger;
            _path = commonSettings.BasePath;
            _dataServices = getDataServices;
        }

        void IJob.Execute()
        {
            _logger.LogDebug($"Начало загрузки архивов");
            DateTime StartDateTime = DateTime.Now;
            _logger.LogInformation($"Начало загрузки архивов: {StartDateTime.ToString()}: {_path}");

            //DownloadFtpFiles44(_dataServices.GetDwList(100, Status.Exist, FLType.Fl44));

            
            var cnt44 = _dataServices.GetDwList(100, Status.Exist, FLType.Fl44).Count;
            var cnt223 = _dataServices.GetDwList(100, Status.Exist, FLType.Fl223).Count;
            
            //Грузить пока не устанет
            while (cnt44 > 0 || cnt223 > 0)
            {
                //2. Загрузка файлов
                //44ФЗ/223ФЗ
                Parallel.Invoke(
                    () => { DownloadFtpFiles44(_dataServices.GetDwList(100, Status.Exist, FLType.Fl44)); },
                    () => { DownloadFtpFiles223(_dataServices.GetDwList(500, Status.Exist, FLType.Fl223)); }
                    );

                cnt44 = _dataServices.GetDwList(1000, Status.Exist, FLType.Fl44).Count;
                cnt223 = _dataServices.GetDwList(1000, Status.Exist, FLType.Fl223).Count;
            }

            //Создание списка и загрузка первой 1000 файлов, при первом вызове.
            Parallel.Invoke(

                // 1. получение списка файлов + сохранение списка для последующей загрузки
                () => { GetListFTP44(); },
                //2. загрузка 1000 файлов через получение списка файлов.
                () => { DownloadFtpFiles44(_dataServices.GetDwList(1000, Status.Exist, FLType.Fl44)); },
                ////ToDo
                // 1. получение списка файлов + сохранение списка для последующей загрузки
                () => { GetListFTP223(); },
                //2. загрузка 1000 файлов через получение списка файлов.
                () => { DownloadFtpFiles223(_dataServices.GetDwList(1000, Status.Exist, FLType.Fl223)); }
                );

        }

        private void GetListFTP44()
        {
            DateTime StartDate = DateTime.Now;
            _logger.LogInformation($"connect to ftp 44, Начало создания списка в {StartDate}"); ;
            var basedir44 = _fzSettings44.BaseDir;
            {

                FtpClient client = new FtpClient(_commonSettings.FtpCredential.FZ44.Url)
                {
                    Credentials = new NetworkCredential(_commonSettings.FtpCredential.FZ44.Login, _commonSettings.FtpCredential.FZ44.Password)
                };

                //Список регионов
                //Дата модификации/создания
                DateTime ModDate = DateTime.ParseExact(_commonSettings.StartDate, "yyyy-MM-dd",
                                           System.Globalization.CultureInfo.InvariantCulture);

                client.Connect();

                var ftpBasePath = $"/{basedir44}/";
                var region44List = client.GetListing(ftpBasePath).Where(item => item.Type == FtpFileSystemObjectType.Directory).Select(x => x.Name).ToList();
                client.Disconnect();
                // Сохранение списка регионов
                var dayyear = DateTime.Now.ToShortDateString();
                var saveregions = Path.Combine(_fzSettings44.WorkPath, $"RegionsList_{dayyear}.txt");
                var saveregionscsv = Path.Combine(_fzSettings44.WorkPath, $"RegionsList_{dayyear}.csv");
                using (StreamWriter file = File.CreateText(saveregions))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, region44List);
                }

                foreach (string region in region44List)
                {
                    _logger.LogInformation($" Get {region} "); ;
                    foreach (string DirsDoc in _fzSettings44.DocDirList)
                    {
                        try
                        {
                            client.Connect();
                            //_logger.LogInformation("connect to ftp 44, region for download: " + region);
                            var ftpPath = $"/{basedir44}/{region}/{DirsDoc}/";

                            string[] paths = { basedir44, region, DirsDoc};
                            string fullPath = Path.Combine(paths);
                            Console.WriteLine(fullPath);


                            var fileList = client.GetListing(ftpPath, FtpListOption.Recursive);
                            var ftpList = fileList.Where(item => item.Size > _fzSettings44.EmptyZipSize && item.Type == FtpFileSystemObjectType.File && item.Modified > ModDate).ToList();
                            //ToDo Реализовать обработку списка файлов, через кэширование записей. 
                            //1. Получить список файлов. 
                            //2. проверить загружался ли, если нет загружаем. 
                            //3. выдать топ 100 файлов на загрузку 
                            //4. Выдать топ 100 загруженных zip но не обработанных файлов.
                            //5. Обработанные архивы фтопку. 
                            //ToDo Save ListFTP
                            SaveFTPPath(ftpList, DirsDoc, basedir44, Status.Exist, FLType.Fl44);
                            //Загрузка файла по региону переделать на загрузку с проверкой
                            //DownloadFtpFiles44(ftpList);
                            _logger.LogInformation($"Создан список файлов для загрузки: { region} /{ DirsDoc} 44ФЗ");
                            client.Disconnect();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, ex.Message);
                            _logger.LogInformation($"Ошибка создания списка файлов для загрузки: { region} /{ DirsDoc} 44ФЗ");
                        }
                    }
                }

            }
        }


        private void GetListFTP223()
        {
            DateTime StartDate = DateTime.Now;
            var basedir223 = _fzSettings223.BaseDir;
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

                client.Connect();
                var ftpBasePath = $"{basedir223}";
                var region223List = client.GetListing(ftpBasePath).Where(item => item.Type == FtpFileSystemObjectType.Directory).Select(x => x.Name).ToList();
                client.Disconnect();
                var dayyear = DateTime.Now.ToShortDateString();
                var saveregions = Path.Combine(_fzSettings223.WorkPath, $"RegionsList_{dayyear}.txt");
                var saveregionscsv = Path.Combine(_fzSettings223.WorkPath, $"RegionsList_{dayyear}.csv");
                using (StreamWriter file = File.CreateText(saveregions))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, region223List);
                }

                //foreach (string region in _fz223Settings.RegionsList)
                foreach (string region in region223List)
                {
                    foreach (string DirsDoc in _fzSettings223.DocDirList)
                    {
                        try
                        {
                            client.Connect();
                            //_logger.LogInformation("connect to ftp 223, region for download: " + region);
                            var ftpPath = $"{basedir223}/{region}/{DirsDoc}/";
                            var fileList = client.GetListing(ftpPath, FtpListOption.Recursive);
                            var ftpList = fileList.Where(item => item.Size > _fzSettings223.EmptyZipSize && item.Type == FtpFileSystemObjectType.File && item.Modified > ModDate).ToList();
                            //ToDo Реализовать обработку списка файлов, через кэширование записей. 
                            //1. Загрузить список файлов. 
                            //2. проверить загружался ли, если нет загружаем. 
                            //3. выдать топ 100 файлов на загрузку 
                            //4. Выдать топ 100 загруженных zip но не обработанных файлов.
                            //5. Обработанные архивы фтопку. 
                            //ToDo Save ListFTP
                            SaveFTPPath(ftpList, DirsDoc, basedir223, Status.Exist, FLType.Fl223);
                            _logger.LogInformation($"Создан список файлов для загрузки: {region} /{DirsDoc} 223ФЗ");
                            client.Disconnect();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogInformation($"Ошибка создания списка файлов для загрузки: { region} /{ DirsDoc} 223ФЗ");
                            _logger.LogError(ex, ex.Message);
                        }
                    }
                } //end foreach regions
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            DateTime EndDate = DateTime.Now;
            _logger.LogInformation($"connect to ftp 223, Список файлов создан в {EndDate}, время на создание списка {(EndDate - StartDate).TotalSeconds} секунд/ {(EndDate - StartDate).TotalMinutes} минут");
        }

        private void DownloadFtpFiles44(List<FileCashes> fileCashes)
        {
            DateTime StartDate = DateTime.Now;
            _logger.LogInformation($"Начало загрузки {fileCashes.Count} архивов FZ44 {StartDate}...");
            _logger.LogDebug($"Начало загрузки {fileCashes.Count} архивов FZ44 {StartDate}...");

            var parallelOptions = new ParallelOptions()
            {
                //MaxDegreeOfParallelism = 1,
                MaxDegreeOfParallelism = _fzSettings44.Parallels,
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

                    _logger.LogInformation($"Загрузка архива FZ44 {item.Full_path}...");
                    client.DownloadFile(_fzSettings44.WorkPath + item.Full_path, item.Full_path);
                    item.Modifid_date = DateTime.Now;
                    item.Status = Status.Uploaded;
                    _dataServices.UpdateCasheFiles(item);
                }
                catch (Exception ex)
                {
                    _dataServices.DeleteCasheFiles(item);
                    _logger.LogError(ex, $"Ошибка скачивания архива FZ44 файл перемещён или недоступен: {item.Full_path}");
                    _logger.LogError(ex, ex.Message);
                }
                finally
                {
                    client.Disconnect();
                }
            });

            DateTime EndDate = DateTime.Now;
            _logger.LogInformation($"Загружено {fileCashes.Count} архивов FZ44 {EndDate}... Время загрузки {(EndDate - StartDate).TotalMinutes} минут");
            _logger.LogDebug($"Загружено  {fileCashes.Count} архивов FZ44 {EndDate}... Время загрузки {(EndDate - StartDate).TotalMinutes} минут");
        }

        private void DownloadFtpFiles223(List<FileCashes> fileCashes)
        {
            DateTime StartDate = DateTime.Now;
            _logger.LogInformation($"Начало загрузки {fileCashes.Count} архивов FZ223 {StartDate}...");
            _logger.LogDebug($"Начало загрузки {fileCashes.Count} архивов FZ223 {StartDate}...");

            var parallelOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = _fzSettings223.Parallels,
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

                    _logger.LogInformation($"Загрузка архива FZ223 {item.Full_path}...");
                    client.DownloadFile(_fzSettings223.WorkPath + item.Full_path, item.Full_path);
                    item.Modifid_date = DateTime.Now;
                    item.Status = Status.Uploaded;
                    _dataServices.UpdateCasheFiles(item);
                }
                catch (Exception ex)
                {
                    _dataServices.DeleteCasheFiles(item);
                    _logger.LogError(ex, $"Ошибка скачивания архива FZ223 файл перемещён или недоступен: {item.Full_path}");
                    _logger.LogError(ex, ex.Message);
                }
                finally
                {
                    client.Disconnect();
                }
            });

            DateTime EndDate = DateTime.Now;
            _logger.LogInformation($"Загружено {fileCashes.Count} архивов FZ223 {EndDate}... Время загрузки {(EndDate - StartDate).TotalMinutes} минут");
            _logger.LogDebug($"Загружено {fileCashes.Count} архивов FZ223 {EndDate}... Время загрузки {(EndDate - StartDate).TotalMinutes} минут");

        }

        private void SaveFTPPath(List<FtpListItem> ListFile, string ftpDir, string baseDir, Status status, FLType fz)
        {
            foreach (FtpListItem item in ListFile)
            {

                if (!_dataServices.CheckCasheFiles(item.FullName))
                {
                    var filesave = new FileCashes();
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


        private void SavePath(FileCashes item)
        {
            try
            {
                using (var db = _govDb.GetContext())
                {
                    db.FileCashes.Add(item);
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
