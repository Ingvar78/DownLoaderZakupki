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
using System.Text;
using System.Xml;
using System.Xml.Serialization;

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
                                //var tt1 = _dataServices.GetFileCashesList(100, Status.Uploaded, FLType.Fl44, basepath, dir);
                                ParseNnotifications(_dataServices.GetFileCashesList(100, Status.Uploaded, FLType.Fl44, basepath, dir));
                            }
                            break;
                        case "contracts":
                            {
                                var tt2 = _dataServices.GetFileCashesList(100, Status.Uploaded, FLType.Fl44, basepath, dir);
                                //ParseContracts(_dataServices.GetFileCashesList(100, Status.Uploaded, FLType.Fl44, basepath, dir));
                            }
                            break;
                        case "protocols":
                            {
                                var tt3 = _dataServices.GetFileCashesList(100, Status.Uploaded, FLType.Fl44, basepath, dir);
                                //ParseProtocols(_dataServices.GetFileCashesList(100, Status.Uploaded, FLType.Fl44, basepath, dir));
                            }
                            break;
                        case "contractprojects":
                            {
                                var tt4 = _dataServices.GetFileCashesList(100, Status.Uploaded, FLType.Fl44, basepath, dir);
                                //ParseContractProjects(_dataServices.GetFileCashesList(100, Status.Uploaded, FLType.Fl44, basepath, dir));
                            }
                            break;
                        case "notificationExceptions":
                            {
                                var tt5 = _dataServices.GetFileCashesList(100, Status.Uploaded, FLType.Fl44, basepath, dir);
                                //ParseNotificationExceptions(_dataServices.GetFileCashesList(100, Status.Uploaded, FLType.Fl44, basepath, dir));
                            }
                            break;

                        default:
                            var tt = _dataServices.GetFileCashesList(100, Status.Uploaded, FLType.Fl44, basepath, dir);
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

        void ParseNnotifications(List<FileCashes> FileCashes)
        {
            //Обрабатываем данный тип;

            foreach (var nFile in FileCashes)
            {
                string zipPath = (_fzSettings44.WorkPath + nFile.Full_path);
                string extractPath = (_fzSettings44.WorkPath + "/extract" + nFile.Full_path);

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

                                FileInfo infoCheck = new FileInfo(xmlin);
                                if (infoCheck.Length != 0)
                                {
                                    XmlDocument xml = new XmlDocument();
                                    try
                                    { xml.Load(xmlin); }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, ex.Message + xmlin);
                                    }

                                    XmlNode node = xml.DocumentElement;

                                    var exportNode_xml = node.ChildNodes[0];
                                    string xmlnodename = exportNode_xml.LocalName;

                                    var tttt = exportNode_xml.Attributes[0].InnerText;

                                    var valuesAsArray = Enum.GetValues(typeof(schemeVersionType));
                                    var valuesAsName = Enum.GetNames(typeof(schemeVersionType));
                                    //var zzz = Enum.Parse(typeof(schemeVersionType), tttt);

                                    var ett = Enum.IsDefined(typeof(schemeVersionType), exportNode_xml.SchemaInfo);

                                    if (ett)
                                    {
                                        Console.WriteLine("Пщщв");
                                    }
                                    

                                    var json1 = JsonConvert.SerializeObject(exportNode_xml);

                                    using (StreamReader reader = new StreamReader(xmlin, Encoding.UTF8, false))
                                    {
                                        XmlSerializer serializer = new XmlSerializer(typeof(export));

                                        XmlSerializer xmlser = new XmlSerializer(typeof(export));
                                        export exportd = xmlser.Deserialize(reader) as export;
                                        Console.WriteLine($"{exportd.ItemsElementName[0].ToString()}");
                                        try
                                        {
                                            exportNsiAbandonedReasonList exportNsiAbandoned = exportd.Items[0] as exportNsiAbandonedReasonList;

#if true && DEBUG
                                            var json = JsonConvert.SerializeObject(exportNsiAbandoned.nsiAbandonedReason);
#endif
                                            //SaveAbandonedReason(exportNsiAbandoned.nsiAbandonedReason);

                                            nFile.Status = Status.Processed;
                                            // Обновляем статус обработанного файла.
                                            //???? _dataServices.UpdateCasheFiles(nFile);

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

                Directory.Delete(extractPath, true);
            }

        }

            


    }
}
