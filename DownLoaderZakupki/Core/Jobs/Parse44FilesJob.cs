using DownLoaderZakupki.Configurations;
using DownLoaderZakupki.Core.Interfaces;
using DownLoaderZakupki.Data.DB;
using DownLoaderZakupki.Models.Enum;
using DownLoaderZakupki.Models.Ext.Fz44;
using FluentScheduler;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace DownLoaderZakupki.Core.Jobs
{
    partial class Parse44FilesJob: IJob
    {
        private readonly CommonSettings _commonSettings;
        private readonly FZSettings44 _fzSettings44;
        private readonly ILogger _logger;
        private readonly IGovDbManager _govDb;
        private readonly string _path;
        private readonly IDataServices _dataServices;
        public Parse44FilesJob(CommonSettings commonSettings,
            FZSettings44 fzSettings44,
            IGovDbManager govDb,
            ILogger logger,
            IDataServices getDataServices
            )
        {
            _commonSettings = commonSettings;
            _fzSettings44 = fzSettings44;
            _govDb = govDb;
            _logger = logger;
            _path = commonSettings.BasePath;
            _dataServices = getDataServices;
        }

        void IJob.Execute()
        {
            try
            {
                _logger.LogInformation("Начата обработка данных закупок ФЗ-44");

                var basepath = _fzSettings44.BaseDir;
                var dirlist = _fzSettings44.DocDirList;
                var parallels44 = _fzSettings44.Parallels;
                foreach (var dir in dirlist)
                {
                    switch (dir)
                    {
                        case "notifications":
                            {
                                //ParseNnotifications(_dataServices.GetFileCashesList(1000, Status.Uploaded, FLType.Fl44, basepath, dir));
                                //var check = _dataServices.GetFileCashesList(1000, Status.Uploaded, FLType.Fl44, basepath, dir);
                                //while (check.Count > 0)
                                //{
                                   // ParseNnotifications(_dataServices.GetFileCashesList(1000, Status.Uploaded, FLType.Fl44, basepath, dir));
                                //    check = _dataServices.GetFileCashesList(1000, Status.Uploaded, FLType.Fl44, basepath, dir);
                                //}
                            }
                            break;
                        case "contracts":
                            {
                                //ParseContracts(_dataServices.GetFileCashesList(1000, Status.Uploaded, FLType.Fl44, basepath, dir));
                            }
                            break;
                        case "protocols":
                            {
                                var check = _dataServices.GetFileCashesList(1000, Status.Uploaded, FLType.Fl44, basepath, dir);
                                while (check.Count > 0)
                                {

                                    ParseProtocols(_dataServices.GetFileCashesList(1000, Status.Uploaded, FLType.Fl44, basepath, dir));
                                    check = _dataServices.GetFileCashesList(1000, Status.Uploaded, FLType.Fl44, basepath, dir);
                                }
                            }
                            break;
                        case "contractprojects":
                            {
                                var tt4 = _dataServices.GetFileCashesList(1000, Status.Uploaded, FLType.Fl44, basepath, dir);
                                //ParseContractProjects(_dataServices.GetFileCashesList(100, Status.Uploaded, FLType.Fl44, basepath, dir));
                            }
                            break;
                        case "notificationExceptions":
                            {
                                var tt5 = _dataServices.GetFileCashesList(1000, Status.Uploaded, FLType.Fl44, basepath, dir);
                                //ParseNotificationExceptions(_dataServices.GetFileCashesList(100, Status.Uploaded, FLType.Fl44, basepath, dir));
                            }
                            break;

                        default:
                            var tt = _dataServices.GetFileCashesList(1000, Status.Uploaded, FLType.Fl44, basepath, dir);
                            _logger.LogWarning($"Ошибка обработки файла из списка DirsDocs: {dir}, проверьте параметры ФЗ-44, не обработано {tt.Count} файлов");
                            break;
                    }

                }

                _logger.LogInformation("Закончена обработка данных закупок ФЗ-44");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

      

            


    }
}
