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
    internal class ParseNsi44FilesJob: IJob
    {
        private readonly CommonSettings _commonSettings;
        private readonly NsiSettings44 _nsiSettings44;
        private readonly ILogger _logger;
        private readonly IGovDbManager _govDb;
        private readonly string _path;
        private readonly IDataServices _dataServices;
        public ParseNsi44FilesJob(CommonSettings commonSettings,
            NsiSettings44 nsiSettings44,
            IGovDbManager govDb,
            ILogger logger,
            IDataServices getDataServices
            )
        {
            _commonSettings = commonSettings;
            _nsiSettings44 = nsiSettings44;
            _govDb = govDb;
            _logger = logger;
            _path = commonSettings.BasePath;
            _dataServices = getDataServices;
        }

        void IJob.Execute()
        {
            try
            {
                var basepath = _nsiSettings44.BaseDir;
                var dirlist = _nsiSettings44.DocDirList;
                var parallels44 = _nsiSettings44.Parallels;
                foreach (var dir in dirlist)
                {
                    switch (dir)
                    {
                        case "nsiAbandonedReason":
                            {
                                //ParsensiAbandonedReason(GetDBList(100, Status.Uploaded, FLType.Fl44, basepath, dir));
                                ParsensiAbandonedReason(_dataServices.GetNsiDBList(100, Status.Uploaded, FLType.Fl44, basepath, dir));
                            }
                            break;
                        case "nsiOrganization":
                            {
                                
                                ParseNsiOrganization(_dataServices.GetNsiDBList(200, Status.Uploaded, FLType.Fl44, basepath, dir));
                            }
                            break;
                        case "nsiPlacingWay":
                            {
                                ParsensiPlacingWay(_dataServices.GetNsiDBList(100, Status.Uploaded, FLType.Fl44, basepath, dir));
                            }
                            break;
                        case "nsiETP":
                            {
                                ParsensiETP(_dataServices.GetNsiDBList(100, Status.Uploaded, FLType.Fl44, basepath, dir));
                            }
                            break;

                        default: break;
                    }

                }

                _logger.LogInformation("Закончена обработка справочников ФЗ-44");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
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
                                    //Console.WriteLine($"{exportd.ItemsElementName[0].ToString()}");
                                    try
                                    {
                                        exportNsiAbandonedReasonList exportNsiAbandoned = exportd.Items[0] as exportNsiAbandonedReasonList;
                                        SaveAbandonedReason(exportNsiAbandoned.nsiAbandonedReason);

                                        nsiFile.Status = Status.Processed;
                                        _dataServices.UpdateCasheFiles(nsiFile);

                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, ex.Message);
                                    }

                                }

                            }
                        }
                }

                Directory.Delete(extractPath, true);
            }
        }


        void SaveAbandonedReason(zfcs_nsiAbandonedReasonType[] nsiAbandonedReason)
        {

            using (var db = _govDb.GetContext())
            {
                foreach (var ar in nsiAbandonedReason)
                {

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


        void ParsensiETP(List<NsiFileCashes> nsiFileCashes)
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
                    try
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

                                    //Console.WriteLine($"{exportd.ItemsElementName[0].ToString()}");
                                        exportNsiETPs NsiETPs = exportd.Items[0] as exportNsiETPs;
                                        SaveNsiETP(NsiETPs.nsiETP);

                                }

                            }
                        }
                        nsiFile.Status = Status.Processed;
                        _dataServices.UpdateCasheFiles(nsiFile);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);
                        nsiFile.Status = Status.Data_Error;
                        _dataServices.UpdateCasheFiles(nsiFile);
                    }
                }

                Directory.Delete(extractPath, true);
            }
        }


        void SaveNsiETP(zfcs_nsiETPType[] nsiETPs)
        {
            using (var db = _govDb.GetContext())
            {
                foreach (var nsiETp in nsiETPs)
                {
                    NsiEtps etp = new NsiEtps()
                    {
                        Code = nsiETp.code,
                        Name = nsiETp.name,
                        Actual = nsiETp.actual,
                        Address = nsiETp.address,
                        Description = nsiETp.description,
                        Email = nsiETp.email,
                        FullName = nsiETp.fullName,
                        INN = nsiETp.INN,
                        KPP = nsiETp.KPP,
                        Phone = nsiETp.phone
                    };
                    try
                    {
                        var find = db.NsiEtps.Where (x=>x.Code==etp.Code).FirstOrDefault();

                        if (find == default)
                        {
                            db.NsiEtps.Add(etp);
                            db.SaveChanges();
                        }
                        else
                        {
                            find.Actual = etp.Actual;
                            db.NsiEtps.Update(etp);
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

        void ParsensiPlacingWay(List<NsiFileCashes> nsiFileCashes)
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
                    try
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
                                    //Console.WriteLine($"{exportd.ItemsElementName[0].ToString()}");
                                    //nsiPlacingWayList
                                    
                                        exportNsiPlacingWayList NsiPlacingWayList = exportd.Items[0] as exportNsiPlacingWayList;
                                        SavePlacingWay(NsiPlacingWayList.nsiPlacingWay);


                                }

                            }
                        }
                        nsiFile.Status = Status.Processed;
                        _dataServices.UpdateCasheFiles(nsiFile);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);
                        nsiFile.Status = Status.Data_Error;
                        _dataServices.UpdateCasheFiles(nsiFile);
                    }
                }

                Directory.Delete(extractPath, true);
            }
        }


        void SavePlacingWay(zfcs_nsiPlacingWayType[] PlacingWays)
        {

            using (var db = _govDb.GetContext())
            {

                foreach (var pw in PlacingWays)
                {
                    NsiPlacingWays placingWay = new NsiPlacingWays()
                    {
                        Code = pw.code,
                        Actual = pw.actual,
                        Fz_type = FLType.Fl44,
                        IsExclude = pw.isExclude,
                        IsProcedure = pw.isProcedure,
                        Name = pw.name,
                        PlacingWayData = JsonConvert.SerializeObject(pw),
                        PlacingWayId = pw.placingWayId,
                        Type = pw.type
                    };

                    switch (pw.subsystemType)
                    {

                        case zfcs_placingWayTypeEnum.FZ44:
                            placingWay.SSType = 44;
                            break;
                        case zfcs_placingWayTypeEnum.FZ94:
                            placingWay.SSType = 94;
                            break;
                        case zfcs_placingWayTypeEnum.PP615:
                            placingWay.SSType = 615;
                            break;
                        default:
                            placingWay.SSType = 0;
                            break;
                    }

                    placingWay.IsClosing = false;
                    if (pw.name.ToLower().Contains("закрыт"))
                    {
                        placingWay.IsClosing = true;
                    }


                    if (!pw.actual)
                    {
                        placingWay.IsClosing = true;
                    }
                    var find = db.NsiPlacingWays.Where(x => x.PlacingWayId == placingWay.PlacingWayId && x.Fz_type == placingWay.Fz_type).SingleOrDefault();

                    if (find == null)
                    {
                        db.NsiPlacingWays.Add(placingWay);
                        db.SaveChanges();
                    }
                    else
                    {
                        find.IsClosing = placingWay.IsClosing;
                        db.NsiPlacingWays.Update(find);
                        db.SaveChanges();
                    }

                }
            }
        }

        void ParseNsiOrganization(List<NsiFileCashes> nsiFileCashes)
        {
            //foreach (var nsiFile in nsiFileCashes)
            Parallel.ForEach(nsiFileCashes,
                new ParallelOptions { MaxDegreeOfParallelism = _nsiSettings44.Parallels },
                (nsiFile) =>
                {

                    string zipPath = (_nsiSettings44.WorkPath + nsiFile.Full_path);
                    string extractPath = (_nsiSettings44.WorkPath + "/extract" + nsiFile.Full_path);
                    //zipPath= @"C:\Work2\Fz44\fcs_nsi\nsiOrganization\nsiOrganizationList_all_20201227000000_174.xml.zip";
                    if (Directory.Exists(extractPath))
                    {
                        Directory.Delete(extractPath, true);
                    }
                    //и создаём её заново
                    Directory.CreateDirectory(extractPath);
                    if (File.Exists(zipPath))
                    {
                        try
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

                                            exportNsiOrganizationList nsiOrganizationList = exportd.Items[0] as exportNsiOrganizationList;
                                            ParseNsiOrganizationList(nsiOrganizationList.nsiOrganization);

                                        }

                                    }
                                }
                            nsiFile.Status = Status.Processed;
                            _dataServices.UpdateCasheFiles(nsiFile);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, ex.Message);
                            //nsiFile.Status = Status.Data_Error;
                            //_dataServices.UpdateCasheFiles(nsiFile);

                            //string[] paths = new string[] { @"C:\Work2\Fz44\", "error", nsiFile.BaseDir, nsiFile.Dirtype};
                            //string fullPath = Path.Combine(paths);
                            //Console.WriteLine(fullPath);

                            string[] paths = { _nsiSettings44.WorkPath, @"error", nsiFile.BaseDir, nsiFile.Dirtype };
                            string fullPath = Path.Combine(paths);
                            Console.WriteLine(fullPath);


                            string errorPath = (_nsiSettings44.WorkPath + "error" + nsiFile.BaseDir + nsiFile.Dirtype);
                            if (!Directory.Exists(fullPath))
                            {
                                Directory.CreateDirectory(fullPath);
                            }

                            errorPath = (errorPath + nsiFile.Zip_file);

                            File.Copy(zipPath, errorPath);
                            
                        }
                    }

                    Directory.Delete(extractPath, true);

                });
        }


        void ParseNsiOrganizationList(zfcs_nsiOrganizationType[] OrganizationList)
        {
            List<NsiOrganizations> nsiOrganizations = new List<NsiOrganizations>();
            foreach (var org in OrganizationList)
            {
                //ToDo Save Org
                try
                {
                    NsiOrganizations nsiOrganization = new NsiOrganizations();
                    if ((org.INN.Trim().Length == 10)|| (org.INN.Trim().Length == 12))
                    {

                        if (org.accounts != null) nsiOrganization.Accounts = JsonConvert.SerializeObject(org.accounts);
                        nsiOrganization.FullName = org.fullName ?? string.Empty;
                        nsiOrganization.Inn = org.INN.Trim();
                        nsiOrganization.Kpp = org.KPP ?? string.Empty;
                        nsiOrganization.Ogrn = org.OGRN ?? string.Empty;
                        nsiOrganization.IsActual = org.actual;                      
                        nsiOrganization.NsiData = JsonConvert.SerializeObject(org);
                        nsiOrganization.RegistrationDate = org.registrationDate;
                        nsiOrganization.RegNumber = org.regNumber;
                        nsiOrganization.Fz_type = FLType.Fl44;

                        nsiOrganizations.Add(nsiOrganization);
#if true && DEBUG
                        var json = JsonConvert.SerializeObject(org);
#endif

                    }
                }

                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    //44
                }

            }

            _dataServices.SaveNsiOrganizationList(nsiOrganizations);
        }


    }
}
