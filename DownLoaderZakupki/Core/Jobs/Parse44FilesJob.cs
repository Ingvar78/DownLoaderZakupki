using DownLoaderZakupki.Configurations;
using DownLoaderZakupki.Core.Interfaces;
using FluentScheduler;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DownLoaderZakupki.Core.Jobs
{
    internal class Parse44FilesJob: IJob
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

                            }
                            break;
                        case "contracts":
                            {

                            }
                            break;
                        case "protocols":
                            {
                            }
                            break;
                        case "contractprojects":
                            {

                            }
                            break;
                        case "notificationExceptions":
                            { 
                            }
                            break;

                        default: break;
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
