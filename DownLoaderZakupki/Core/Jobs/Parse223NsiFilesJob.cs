using DownLoaderZakupki.Configurations;
using DownLoaderZakupki.Core.Interfaces;
using DownLoaderZakupki.Data.DB;
using DownLoaderZakupki.Models.Enum;
using DownLoaderZakupki.Models.Ext.Fz223;
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
    internal class Parse223NsiFilesJob: IJob
    {
        private readonly CommonSettings _commonSettings;
        private readonly NsiSettings223 _nsiSettings223;
        private readonly ILogger _logger;
        private readonly IGovDbManager _govDb;
        private readonly string _path;
        private readonly IDataServices _dataServices;
        public Parse223NsiFilesJob(CommonSettings commonSettings,
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
            _dataServices = getDataServices;
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
                                var tt223 = _dataServices.GetNsiDBList(100, Status.Uploaded, FLType.Fl223, basepath, dir);
                                ParseNsiOrganization(tt223);
                            }
                            break;
                        case "nsiPlacingWay":
                            {
                                var tt223 = _dataServices.GetNsiDBList(100, Status.Uploaded, FLType.Fl223, basepath, dir);
                                //ParsensiPlacingWay(tt);
                            }
                            break;
                        case "nsiETP":
                            {
                                var tt223 = _dataServices.GetNsiDBList(100, Status.Uploaded, FLType.Fl223, basepath, dir);
                                //ParsensiETP(_getDataServices.GetDBList1(100, Status.Uploaded, FLType.Fl223, basepath, dir));
                            }
                            break;

                        default: break;
                    }

                }
                _logger.LogInformation("Закончена обработка справочников ФЗ-223");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }


        void ParseNsiOrganization(List<NsiFileCashes> nsiFileCashes)
        {
            //foreach (var nsiFile in nsiFileCashes)
            Parallel.ForEach(nsiFileCashes,
                new ParallelOptions { MaxDegreeOfParallelism = _nsiSettings223.Parallels },
                (nsiFile) =>
                {

                    string zipPath = (_nsiSettings223.WorkPath + nsiFile.Full_path);
                    string extractPath = (_nsiSettings223.WorkPath + "/extract" + nsiFile.Full_path);

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
                                    //xmlin = @"C:\Work2\Fz223\out\nsi\111\nsiOrganization_all_20201220_010000_001.xml";

                                    FileInfo infoCheck = new FileInfo(xmlin);
                                    if (infoCheck.Length != 0)
                                    {
                                        using (StreamReader reader = new StreamReader(xmlin, Encoding.UTF8, false))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(nsiOrganization));

                                            XmlSerializer xmlser = new XmlSerializer(typeof(nsiOrganization));
                                            nsiOrganization exportd = xmlser.Deserialize(reader) as nsiOrganization;

                                            //Console.WriteLine($"{exportd.ItemsElementName[0].ToString()}");


                                            try
                                            {

                                                nsiOrganizationItemType[] nsiOrganizationList = exportd.body as nsiOrganizationItemType[];
                                                _logger.LogInformation($"Поступило в обработку {nsiOrganizationList.Length} организаций");
                                                //Console.WriteLine(nsiOrganizationList.Length);
                                                ParseNsiOrganizationList(exportd.body);
                                                nsiFile.Status = Status.Processed;
                                                _dataServices.UpdateNsiCasheFiles(nsiFile);
                                            }
                                            catch (Exception ex)
                                            {
                                                _logger.LogError(ex, ex.Message);
                                            }

                                        }
                                    }
                                    else
                                    {
                                        nsiFile.Status = Status.Data_Error;
                                        _dataServices.UpdateNsiCasheFiles(nsiFile);
                                    }
                                }
                            }
                    }

                    Directory.Delete(extractPath, true);

                });
        }


        void ParseNsiOrganizationList(nsiOrganizationItemType[] OrganizationList)
        {
            List<NsiOrganizations> nsiOrganizations = new List<NsiOrganizations>();
            foreach (var org in OrganizationList)
            {
                //ToDo Save Org
                try
                {
                    NsiOrganizations nsiOrganization = new NsiOrganizations();

                    if (org.nsiOrganizationData.mainInfo.inn != null)
                    {
                        if ((org.nsiOrganizationData.mainInfo.inn.Trim().Length == 10) || (org.nsiOrganizationData.mainInfo.inn.Trim().Length == 12))
                        {

#if true && DEBUG
                            var json = JsonConvert.SerializeObject(org);
#endif
                            nsiOrganization.FullName = org.nsiOrganizationData.mainInfo.fullName ?? string.Empty;
                            nsiOrganization.Inn = org.nsiOrganizationData.mainInfo.inn.Trim() ?? string.Empty;
                            nsiOrganization.Kpp = org.nsiOrganizationData.mainInfo.kpp ?? string.Empty;
                            nsiOrganization.Ogrn = org.nsiOrganizationData.mainInfo.ogrn ?? string.Empty;
                            nsiOrganization.NsiData = JsonConvert.SerializeObject(org);
                            nsiOrganization.RegistrationDate = org.nsiOrganizationData.codeAssignDateTime;
                            nsiOrganization.RegNumber = org.nsiOrganizationData.code;
                            nsiOrganization.Accounts = "{}";
                            //Нет данных - поискать
                            nsiOrganization.Fz_type = FLType.Fl223;
                            nsiOrganizations.Add(nsiOrganization);
                        }
                    }
                    
                }

                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }

            }

            _logger.LogInformation($"Обработано {nsiOrganizations.Count} организаций");
            _dataServices.SaveNsiOrgList(nsiOrganizations);
            //SaveNsiOrganizationList(nsiOrganizations);
        }
    }
}
