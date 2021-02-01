using DownLoaderZakupki.Configurations;
using DownLoaderZakupki.Core.Interfaces;
using DownLoaderZakupki.Core.Jobs;
using FluentScheduler;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;



namespace DownLoaderZakupki
{
    internal class JobsRegister : Registry
    {
        private readonly ILogger _logger;
        public JobsRegister(
            IOptions<CommonSettings> commonSettings,
            IGovDbManager govDb,
            IOptions<FZSettings44> fzSettings44,
            IOptions<FZSettings223> fzSettings223,
            IOptions<NsiSettings44> nsiSettings44,
            IOptions<NsiSettings223> nsiSettings223,
            ILogger<JobsRegister> logger,
            IDataServices dataServices
            )
        {
            _logger = logger;
            _logger.LogInformation("Start Init Job");

            var partUsed = commonSettings.Value.partUsed;
            if (!Directory.Exists(commonSettings.Value.BasePath))
            {
                Directory.CreateDirectory(commonSettings.Value.BasePath);
                Directory.CreateDirectory(fzSettings44.Value.WorkPath);
                Directory.CreateDirectory(fzSettings223.Value.WorkPath);
            }
            else
            {
                if (!Directory.Exists(fzSettings44.Value.WorkPath))
                {
                    Directory.CreateDirectory(fzSettings44.Value.WorkPath);
                }
                if (!Directory.Exists(fzSettings223.Value.WorkPath))
                {
                    Directory.CreateDirectory(fzSettings223.Value.WorkPath);
                }
            }
            
            //Загрузка архивов ФЗ 44 и 223 - данные аукционов, контрактов...справочников - всё что может понадобиться.
            if (partUsed.UseUpload)
            {
                // данные аукционов, контрактов
                Schedule(() => new UploadFilesJob(commonSettings.Value, 
                    fzSettings44.Value, 
                    fzSettings223.Value, 
                    govDb, 
                    logger,
                    dataServices))
                    .NonReentrant()
                    .ToRunNow()
                    .AndEvery(4).Hours();

                //Данные справочников
                Schedule(() => new UploadNsiFilesJob(commonSettings.Value,
                    nsiSettings44.Value,
                    nsiSettings223.Value,
                    govDb, logger))
                    .NonReentrant()
                    .ToRunNow()
                    .AndEvery(24).Hours();
            }
            
            // Обработка справочников ФЗ-44
            if (partUsed.UseNsiSettings44)
            {
                Schedule(() => new Parse44NsiFilesJob(commonSettings.Value,
                        nsiSettings44.Value,
                        govDb, logger, 
                        dataServices))
                        .NonReentrant()
                        .ToRunNow();
            }
            //Обработка справочников ФЗ-223
            if (partUsed.UseNsiSettings223)
            {
                Schedule(() => new Parse223NsiFilesJob(commonSettings.Value,
                        nsiSettings223.Value,
                        govDb, logger,
                        dataServices))
                        .NonReentrant()
                        .ToRunNow();
            }

            if (partUsed.UseFz44Settings)
            {
                Schedule(() => new Parse44FilesJob(commonSettings.Value,
                            fzSettings44.Value,
                            govDb, logger,
                            dataServices))
                            .NonReentrant()
                            .ToRunNow();
            }
            _logger.LogInformation("End Init Job");
        }

    }
}
