using DownLoaderZakupki.Configurations;
using FluentFTP;
using FluentScheduler;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DownLoaderZakupki.Core.Jobs
{
    internal class UploadFilesJob : IJob
    {
        private readonly CommonSettings _commonSettings;
        private readonly FZSettings44 _fzSettings44;
        private readonly FZSettings223 _fzSettings223;
        private readonly ILogger _logger;
        private readonly string _path;
        public UploadFilesJob(CommonSettings commonSettings,
            FZSettings44 fzSettings44,
            FZSettings223 fzSettings223,
            ILogger logger
            )
        {
            _commonSettings = commonSettings;
            _fzSettings44 = fzSettings44;
            _fzSettings223 = fzSettings223;
            _logger = logger;
            _path = commonSettings.BasePath;
        }

        void IJob.Execute()
        {
            DateTime StartDateTime = DateTime.Now;
            _logger.LogInformation($"Начало загрузки архивов: {StartDateTime.ToString()}: {_path}");

            try
            {
                Parallel.Invoke(
                    () => { GetListFTP44(); }
                //    () => { GetListFTP223(); }
                    );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }


        }

        private void GetListFTP44()
        {
            DateTime StartDate = DateTime.Now;
            var basedir44 = _fzSettings44.BaseDir;
            _logger.LogInformation($"connect to ftp 44, Начало создания списка в {StartDate}"); ;
            {

                FtpClient client = new FtpClient(_fzSettings44.Url)
                {
                    Credentials = new NetworkCredential(_fzSettings44.Login, _fzSettings44.Password)
                };

                //Список регионов
                //Дата модификации/создания
                DateTime ModDate = DateTime.ParseExact(_commonSettings.StartDate, "yyyy-MM-dd",
                                           System.Globalization.CultureInfo.InvariantCulture);

                client.Connect();

                var ftpBasePath = $"/{basedir44}/";
                var region44List = client.GetListing(ftpBasePath).Where(item => item.Type == FtpFileSystemObjectType.Directory).Select(x => x.Name).ToList();
                client.Disconnect();

                foreach (string region in region44List)
                {
                    foreach (string DirsDoc in _fzSettings44.DocDirList)
                    {
                        try
                        {
                            client.Connect();
                            //_logger.LogInformation("connect to ftp 44, region for download: " + region);
                            var ftpPath = $"/{basedir44}/{region}/{DirsDoc}/";
                            var fileList = client.GetListing(ftpPath, FtpListOption.Recursive);
                            var ftpList = fileList.Where(item => item.Size > _fzSettings44.EmptyZipSize && item.Type == FtpFileSystemObjectType.File && item.Modified > ModDate).ToList();
                            //ToDo Реализовать обработку списка файлов, через кэширование записей. 
                            //1. Получить список файлов. 
                            //2. проверить загружался ли, если нет загружаем. 
                            //3. выдать топ 100 файлов на загрузку 
                            //4. Выдать топ 100 загруженных zip но не обработанных файлов.
                            //5. Обработанные архивы фтопку. 
                            //ToDo Save ListFTP

                            //Загрузка файла по региону
                            DownloadFtpFiles44(ftpList);
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

        private void DownloadFtpFiles44(List<FtpListItem> fileCashes)
        {
            DateTime StartDate = DateTime.Now;
            _logger.LogInformation($"Начало загрузки {fileCashes.Count} архивов FZ44 {StartDate}...");

            var parallelOptions = new ParallelOptions()
            {
                //MaxDegreeOfParallelism = 1,
                MaxDegreeOfParallelism = _fzSettings44.Parallels,
            };

            Parallel.ForEach(fileCashes, parallelOptions, item =>
            {
                FtpClient client = new FtpClient(_fzSettings44.Url)
                {
                    Credentials = new NetworkCredential(_fzSettings44.Login, _fzSettings44.Password),
                    RetryAttempts = 5
                };

                try
                {

                    client.Connect();

                    _logger.LogInformation($"Загрузка архива FZ44 {item.FullName}...");
                    client.DownloadFile(_fzSettings44.WorkPath + item.FullName, item.FullName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Ошибка скачивания архива FZ44 файл перемещён или недоступен: {item.FullName}");
                    _logger.LogError(ex, ex.Message);
                }
                finally
                {
                    client.Disconnect();
                }
            });

            DateTime EndDate = DateTime.Now;
            _logger.LogInformation($"Загружено {fileCashes.Count} архивов FZ44 {EndDate}... Время загрузки {(EndDate - StartDate).TotalMinutes} минут");

        }
    }


}
