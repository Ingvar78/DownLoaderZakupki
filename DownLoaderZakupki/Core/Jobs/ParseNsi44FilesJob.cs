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
using System.Xml.Serialization;

namespace DownLoaderZakupki.Core.Jobs
{
    internal class ParseNsi44FilesJob: IJob
    {
        private readonly CommonSettings _commonSettings;
        private readonly NsiSettings44 _nsiSettings44;
        private readonly ILogger _logger;
        private readonly IGovDbManager _govDb;
        private readonly string _path;
        public ParseNsi44FilesJob(CommonSettings commonSettings,
            NsiSettings44 nsiSettings44,
            IGovDbManager govDb,
            ILogger logger
            )
        {
            _commonSettings = commonSettings;
            _nsiSettings44 = nsiSettings44;
            _govDb = govDb;
            _logger = logger;
            _path = commonSettings.BasePath;
        }

        void IJob.Execute()
        {
            try
            {
                var tx = (double.MaxValue).ToString();

                var basepath = _nsiSettings44.BaseDir;
                var dirlist = _nsiSettings44.DocDirList;
                foreach (var dir in dirlist)
                {
                    switch (dir)
                    {
                        case "nsiAbandonedReason":
                            { 
                                ParsensiAbandonedReason(GetDBList(100, Status.Uploaded, FLType.Fl44, basepath, dir));
                            }
                            break;
                        //ToDo
                        case "nsiOrganization":
                            {
                                var tt = GetDBList(100, Status.Uploaded, FLType.Fl44, basepath, dir);
                            }
                            break;
                        case "nsiPlacingWay":
                            {
                                var tt = GetDBList(100, Status.Uploaded, FLType.Fl44, basepath, dir);
                            }
                            break;
                        case "nsiETP":
                            {
                                var tt = GetDBList(100, Status.Uploaded, FLType.Fl44, basepath, dir);
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


        List<NsiFileCashes> GetDBList(int lim, Status status, FLType fz_type, string basepath, string dirtype)
        {
            List<NsiFileCashes> data = new List<NsiFileCashes>();

            using (var db = _govDb.GetContext())
            {
                data = db.NsiFileCashes
                    .AsNoTracking()
                    .Where(x => x.Status == status
                    && x.Fz_type == fz_type
                    && x.BaseDir == basepath
                    && x.Dirtype == dirtype)
                    .OrderByDescending(x => x.Date)
                    .Take(lim)
                    .ToList();
            }
            return data;
        }

        void ParsensiAbandonedReason(List<NsiFileCashes> nsiFileCashes)
        {
            foreach (var nsiFile in nsiFileCashes)
            {
                string zipPath = (_nsiSettings44.WorkPath + nsiFile.Full_path);
                string extractPath = (_nsiSettings44.WorkPath + "/extract" + nsiFile.Full_path);

                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true);
                }
                //и создаём её заново
                Directory.CreateDirectory(extractPath);

                if (File.Exists(zipPath))
                {
                    using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                            {
                                entry.ExtractToFile(Path.Combine(extractPath, entry.FullName));
                                string xml_f_name = entry.FullName;
                                string xmlin = (extractPath + "/" + entry.FullName);
                                _logger.LogInformation("xmlin parse: " + xmlin);

                                using (StreamReader reader = new StreamReader(xmlin, Encoding.UTF8, false))
                                {
                                    XmlSerializer serializer = new XmlSerializer(typeof(export));

                                    XmlSerializer xmlser = new XmlSerializer(typeof(export));
                                    export exportd = xmlser.Deserialize(reader) as export;

                                    Console.WriteLine($"{exportd.ItemsElementName[0].ToString()}");
                                    //exportNsiPlacingWayList

                                    try
                                    {
                                        exportNsiAbandonedReasonList exportNsiAbandoned = exportd.Items[0] as exportNsiAbandonedReasonList;
                                        //SaveAbandonedReasons(exportNsiAbandonedReasonList);
                                        SaveAbandonedReason(exportNsiAbandoned.nsiAbandonedReason);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, ex.Message);
                                    }

                                }

                            }
                        }
                }
            }
        }


        void SaveAbandonedReason(zfcs_nsiAbandonedReasonType[] nsiAbandonedReason)
        {

            using (var db = _govDb.GetContext())
            {
                foreach (var ar in nsiAbandonedReason)
                {
                    var tt = JsonConvert.SerializeObject(ar.docType);
                    var dd = JsonConvert.SerializeObject(ar.placingWay);

                    NsiAbandonedReason NsiAReason = new NsiAbandonedReason()
                    {
                        Code = ar.code,
                        Name = ar.name,
                        docType = JsonConvert.SerializeObject( ar.docType),
                        objectName = ar.objectName,
                        PlacingWay = JsonConvert.SerializeObject(ar.placingWay),
                        Type = ar.type.ToString(),
                        Fz_type=FLType.Fl44,
                        Actual = ar.actual,
                        OosId = ar.id
                    };
                    try {

                        var find = db.NsiAReasons.Where(x => x.Code == NsiAReason.Code 
                        && x.OosId==ar.id
                        && x.Fz_type == FLType.Fl44).FirstOrDefault();
                    if (find == null)
                    {
                        db.NsiAReasons.Add(NsiAReason);
                        db.SaveChanges();
                    }
                    else
                    {
                            //find.objectName = NsiAReason.objectName;
                            find.Actual= NsiAReason.Actual;
                            //find.docType= NsiAReason.docType;
                            //find.objectName = NsiAReason.objectName;
                            db.NsiAReasons.Update(find);
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
    }
}
