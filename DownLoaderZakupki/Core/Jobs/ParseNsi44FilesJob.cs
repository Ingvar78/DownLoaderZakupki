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
                        case "nsiOrganization":
                            {
                                var tt = GetDBList(100, Status.Uploaded, FLType.Fl44, basepath, dir);
                                ParseNsiOrganization(tt);
                            }
                            break;
                        case "nsiPlacingWay":
                            {
                                var tt = GetDBList(100, Status.Uploaded, FLType.Fl44, basepath, dir);
                                ParsensiPlacingWay(tt);
                            }
                            break;
                        case "nsiETP":
                            {
                                ParsensiETP(GetDBList(100, Status.Uploaded, FLType.Fl44, basepath, dir));
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
                    .OrderBy(x => x.Date)
                    //.OrderByDescending(x => x.Date)
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
                                    //Console.WriteLine($"{exportd.ItemsElementName[0].ToString()}");
                                    try
                                    {
                                        exportNsiAbandonedReasonList exportNsiAbandoned = exportd.Items[0] as exportNsiAbandonedReasonList;
                                        SaveAbandonedReason(exportNsiAbandoned.nsiAbandonedReason);

                                        nsiFile.Status = Status.Processed;
                                        UpdateCasheFiles(nsiFile);

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
                                        exportNsiETPs NsiETPs = exportd.Items[0] as exportNsiETPs;
                                        SaveNsiETP(NsiETPs.nsiETP);

                                        nsiFile.Status = Status.Processed;
                                        UpdateCasheFiles(nsiFile);
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
                                    try
                                    {
                                        exportNsiPlacingWayList NsiPlacingWayList = exportd.Items[0] as exportNsiPlacingWayList;
                                        SavePlacingWay(NsiPlacingWayList.nsiPlacingWay);

                                        nsiFile.Status = Status.Processed;
                                        UpdateCasheFiles(nsiFile);
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
                                //xmlin = @"C:\FZ\000\nsiOrganizationList_all_20200315000006_287.xml";
                                using (StreamReader reader = new StreamReader(xmlin, Encoding.UTF8, false))
                                {
                                    XmlSerializer serializer = new XmlSerializer(typeof(export));

                                    XmlSerializer xmlser = new XmlSerializer(typeof(export));
                                    export exportd = xmlser.Deserialize(reader) as export;

                                    //Console.WriteLine($"{exportd.ItemsElementName[0].ToString()}");


                                    try
                                    {
                                        exportNsiOrganizationList nsiOrganizationList = exportd.Items[0] as exportNsiOrganizationList;
                                        ParseNsiOrganizationList(nsiOrganizationList.nsiOrganization);

                                        nsiFile.Status = Status.Processed;
                                        UpdateCasheFiles(nsiFile);
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

                        if (JsonConvert.SerializeObject(org.contactPerson) != null) nsiOrganization.ContactPerson = JsonConvert.SerializeObject(org.contactPerson);
                        nsiOrganization.Email = org.email ?? string.Empty;
                        if (JsonConvert.SerializeObject(org.factualAddress) != null) nsiOrganization.FactualAddress = JsonConvert.SerializeObject(org.factualAddress);
                        nsiOrganization.Fax = org.fax ?? string.Empty;
                        nsiOrganization.FullName = org.fullName ?? string.Empty;
                        nsiOrganization.Inn = org.INN.Trim();
                        nsiOrganization.IsActual = org.actual;
                        nsiOrganization.Kpp = org.KPP ?? string.Empty;
                        nsiOrganization.NsiData = JsonConvert.SerializeObject(org);
                        nsiOrganization.Ogrn = org.OGRN ?? string.Empty;
                        if (org.OKOPF != null) nsiOrganization.Okopf = org.OKOPF.code;
                        //else nsiOrganization.Okopf = string.Empty;

                        nsiOrganization.Okpo = org.OKPO ?? string.Empty;
                        if (org.OKTMO != null) nsiOrganization.Oktmo = org.OKTMO.code;
                        //else nsiOrganization.Oktmo = string.Empty;
                        nsiOrganization.Okved = org.OKVED ?? string.Empty;
                        nsiOrganization.Phone = org.phone ?? string.Empty;
                        nsiOrganization.PostalAddress = org.postalAddress ?? string.Empty;
                        nsiOrganization.RegistrationDate = org.registrationDate;
                        nsiOrganization.RegNumber = org.regNumber;
                        nsiOrganization.ShortName = org.shortName ?? string.Empty;
                        nsiOrganization.TimeZone = org.timeZone;
                        nsiOrganization.Url = org.url;
                        if (org.accounts != null) nsiOrganization.Accounts = JsonConvert.SerializeObject(org.accounts);
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
                }

            }

            SaveNsiOrganizationList(nsiOrganizations);
        }


        void SaveNsiOrganizationList(List<NsiOrganizations> nsiOrganizations)
        {

            foreach (var organization in nsiOrganizations)
            {
                using (var db = _govDb.GetContext())
                {
                    try
                    {
                        var find = db.NsiOrganizations
                        .AsNoTracking()
                        .Where(x => x.Inn == organization.Inn)
                        .SingleOrDefault();

                        if (find == null)
                        {
                            db.NsiOrganizations.Add(organization);
                            db.SaveChanges();
                        }
                        else
                        {
                            find.NsiData = organization.NsiData;
                            find.ContactPerson = organization.ContactPerson;
                            find.Email = organization.Email;
                            find.FactualAddress = organization.FactualAddress;
                            find.Fax = organization.Fax;
                            find.FullName = organization.FullName;
                            find.IsActual = organization.IsActual;
                            find.Kpp = organization.Kpp;
                            find.NsiData = organization.NsiData;
                            find.Ogrn = organization.Ogrn;
                            find.Okopf = organization.Okopf;
                            find.Okpo = organization.Okpo;
                            find.Oktmo = organization.Oktmo;
                            find.Okved = organization.Okved;
                            find.Phone = organization.Phone;
                            find.PostalAddress = organization.PostalAddress;
                            find.RegistrationDate = organization.RegistrationDate;
                            find.RegNumber = organization.RegNumber;
                            find.ShortName = organization.ShortName;
                            find.TimeZone = organization.TimeZone;
                            find.Url = organization.Url;
                            find.Accounts = organization.Accounts;
                            db.NsiOrganizations.Update(find);
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


        private void UpdateCasheFiles(NsiFileCashes fileCashes)
        {
            using (var db = _govDb.GetContext())
            {
                db.NsiFileCashes.Update(fileCashes);
                db.SaveChanges();
            }
        }
    }
}
