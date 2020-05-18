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
        private readonly IGovDbManager _govDb;
        public JobsRegister(
            IOptions<CommonSettings> commonSettings,
            IGovDbManager govDb,
            IOptions<FZSettings44> fzSettings44,
            IOptions<FZSettings223> fzSettings223,
            ILogger<JobsRegister> logger
            )
        {
            _logger = logger;
            _logger.LogInformation("Start Job");

            var partUsed = commonSettings.Value.partUsed;
            if (!Directory.Exists(commonSettings.Value.BasePath))
            {
                Directory.CreateDirectory(commonSettings.Value.BasePath);
                Directory.CreateDirectory(fzSettings44.Value.WorkPath);
                Directory.CreateDirectory(fzSettings223.Value.WorkPath);
            }

            if (partUsed.UseUpload)
            {
                Schedule(() => new UploadFilesJob(commonSettings.Value, fzSettings44.Value, fzSettings223.Value,govDb, logger))
                    .NonReentrant()
                    .ToRunNow();
            }

            _logger.LogInformation("End Job");
        }

    }
}
