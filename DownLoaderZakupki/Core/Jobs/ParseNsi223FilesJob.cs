using DownLoaderZakupki.Configurations;
using DownLoaderZakupki.Core.Interfaces;
using DownLoaderZakupki.Data.DB;
using DownLoaderZakupki.Models.Enum;
using DownLoaderZakupki.Models.Ext.Fz44;
using FluentScheduler;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DownLoaderZakupki.Core.Jobs
{
    internal class ParseNsi223FilesJob: IJob
    {
        private readonly CommonSettings _commonSettings;
        private readonly NsiSettings223 _nsiSettings223;
        private readonly ILogger _logger;
        private readonly IGovDbManager _govDb;
        private readonly string _path;
        private readonly IDataServices _getDataServices;
        public ParseNsi223FilesJob(CommonSettings commonSettings,
            NsiSettings223 nsiSettings223,
            IGovDbManager govDb,
            ILogger logger,
            IDataServices getDataServices
            )
        {
            _commonSettings = commonSettings;
            _nsiSettings223 = nsiSettings223;
            _govDb = govDb;
            _logger = logger;
            _path = commonSettings.BasePath;
            _getDataServices = getDataServices;
        }

        void IJob.Execute()
        {
            try
            {
                var basepath = _nsiSettings223.BaseDir;
                var dirlist = _nsiSettings223.DocDirList;
                var parallels223 = _nsiSettings223.Parallels;
                foreach (var dir in dirlist)
                {
                    switch (dir)
                    {
                        case "nsiOrganization":
                            {
                                var tt223 = _getDataServices.GetDBList1(100, Status.Uploaded, FLType.Fl223, basepath, dir);
                                //ParseNsiOrganization(tt223);
                            }
                            break;
                        case "nsiPlacingWay":
                            {
                                var tt223 = _getDataServices.GetDBList1(100, Status.Uploaded, FLType.Fl223, basepath, dir);
                                //ParsensiPlacingWay(tt);
                            }
                            break;
                        case "nsiETP":
                            {
                                var tt223 = _getDataServices.GetDBList1(100, Status.Uploaded, FLType.Fl223, basepath, dir);
                                //ParsensiETP(_getDataServices.GetDBList1(100, Status.Uploaded, FLType.Fl223, basepath, dir));
                            }
                            break;

                        default: break;
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

    }
}
