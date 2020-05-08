using DownLoaderZakupki.Configurations;
using FluentScheduler;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
                //Parallel.Invoke(
                //    () => { GetListFTP44(); },
                //    () => { GetListFTP223(); }
                //    );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }


        }
    }
}
