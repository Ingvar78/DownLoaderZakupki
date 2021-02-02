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
                                    try
                                    { 
                                    using (StreamReader reader = new StreamReader(xmlin, Encoding.UTF8, false))
                                    {
                                        XmlSerializer serializer = new XmlSerializer(typeof(export));

                                        XmlSerializer xmlser = new XmlSerializer(typeof(export));
                                        export exportd = xmlser.Deserialize(reader) as export;
                                        Console.WriteLine($"{exportd.ItemsElementName[0].ToString()}");
                                            //Console.WriteLine();

                                            //string errfile = (_commonSettings.DebugPath + nFile.Full_path);
                                            //if (!Directory.Exists(errfile)) Directory.CreateDirectory(errfile);

                                            //System.IO.File.Copy(xmlin, _commonSettings.DebugPath + nFile.Full_path +'/'+ entry.FullName, true);
                                            switch (exportd.ItemsElementName[0].ToString())
                                        {

                                            case "fcsNotificationEP":
                                                {
                                                    Console.WriteLine($"{exportd.ItemsElementName[0].ToString()}");

                                                    string exp_json = JsonConvert.SerializeObject(exportd);
                                                    //var EData = JsonConvert.DeserializeObject<export>(exp_json);

                                                    zfcs_notificationEPType fcsNotificationEP = exportd.Items[0] as zfcs_notificationEPType;
                                                    string unf_json = JsonConvert.SerializeObject(fcsNotificationEP);

                                                    try
                                                    {
                                                        var AData = JsonConvert.DeserializeObject<notificationEOKOUType>(unf_json);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Console.WriteLine(ex);
                                                    }
                                                    var pnum = fcsNotificationEP.purchaseNumber;
                                                    var etype = exportd.Items[0].GetType().Name;
                                                    var pdate = fcsNotificationEP.docPublishDate;
                                                    //SaveNotification(pnum, exp_json, etype, zipPath, xml_f_name, 44, pdate);
                                                    Console.WriteLine($"{exportd.ItemsElementName[0].ToString()}");
                                                    break;
                                                }
                                            case "fcsClarification":
                                                {
                                                    string exp_json = JsonConvert.SerializeObject(exportd);
                                                    var EData = JsonConvert.DeserializeObject<export>(exp_json);
                                                    zfcs_clarificationType fcsClarification = exportd.Items[0] as zfcs_clarificationType;
                                                    string unf_json = JsonConvert.SerializeObject(fcsClarification);
                                                    var pnum = fcsClarification.purchaseNumber;
                                                    var etype = exportd.Items[0].GetType().Name;
                                                    var pdate = fcsClarification.docPublishDate;
                                                    //SaveNotification(pnum, exp_json, etype, zipPath, xml_f_name, 44, pdate);

                                                    Console.WriteLine($"{exportd.ItemsElementName[0].ToString()}");
                                                    break;
                                                }
                                            case "fcsNotificationEF":
                                                {
                                                    string exp_json = JsonConvert.SerializeObject(exportd);
                                                    zfcs_notificationEFType fcsNotificationEF = exportd.Items[0] as zfcs_notificationEFType;
                                                    string unf_json = JsonConvert.SerializeObject(fcsNotificationEF);
                                                    var pnum = fcsNotificationEF.purchaseNumber;
                                                    var etype = exportd.Items[0].GetType().Name;
                                                    var pdate = fcsNotificationEF.docPublishDate;
                                                    //SaveNotification(pnum, exp_json, etype, zipPath, xml_f_name, 44, pdate);

                                                    Console.WriteLine($"{exportd.ItemsElementName[0].ToString()}");
                                                    break;
                                                }

                                            case "contractProcedure":
                                                {
                                                    string exp_json = JsonConvert.SerializeObject(exportd);
                                                    zfcs_contractProcedure2015Type contractProcedure = exportd.Items[0] as zfcs_contractProcedure2015Type;

                                                    string unf_json = JsonConvert.SerializeObject(contractProcedure);
                                                    var cnum = contractProcedure.regNum;
                                                    var etype = exportd.Items[0].GetType().Name;
                                                    var pdate = contractProcedure.publishDate;
                                                    //contractProcedure(_connectionDB.ConnectionDB1, cnum, exp_json, etype, zipPath, xml_f_name, 44, pdate);
                                                    Console.WriteLine($"{exportd.ItemsElementName[0].ToString()}");
                                                    break;
                                                }
                                            case "contract":
                                                {
                                                    string exp_json = JsonConvert.SerializeObject(exportd);
                                                    zfcs_contract2015Type contract = exportd.Items[0] as zfcs_contract2015Type;

                                                    string unf_json = JsonConvert.SerializeObject(contract);
                                                    var cnum = contract.regNum;
                                                    var etype = exportd.Items[0].GetType().Name;
                                                    var pdate = contract.publishDate;
                                                    //contractProcedure(cnum, exp_json, etype, zipPath, xml_f_name, 44, pdate);
                                                    Console.WriteLine($"{exportd.ItemsElementName[0].ToString()}");
                                                    break;
                                                }

                                            default:
                                                {

                                                    if (exportd.Items.Length > 1)
                                                    {
                                                        Console.WriteLine("More one");
                                                        _logger.LogWarning($"More then one Items in file: {infoCheck.FullName} ");
                                                    }
                                                    string exp_json = JsonConvert.SerializeObject(exportd);
                                                    var EData = JsonConvert.DeserializeObject<export>(exp_json);
                                                    string eltype = $"{exportd.ItemsElementName[0].ToString()};{exportd.Items[0].GetType().Name}";
                                                    string fnel = $"{exportd.Items[0].GetType().Name}";

                                                    using (StreamWriter sw1 = new StreamWriter(@$"D:\\FZ\\Types44\\{fnel}", true, System.Text.Encoding.Default))
                                                    {

                                                        sw1.WriteLine(eltype);

                                                    };


                                                    break;
                                                }
                                        }

                                   
                                        //#if true && DEBUG
                                        //                                            var json = JsonConvert.SerializeObject(exportd.item);
                                        //#endif
                                    }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "Error parse");
                                        _logger.LogError(ex, ex.Message);
                                        string errfile = (_commonSettings.DebugPath + nFile.Full_path);
                                        if (!Directory.Exists(errfile)) Directory.CreateDirectory(errfile);
                                        System.IO.File.Copy(xmlin, _commonSettings.DebugPath + nFile.Full_path + '/' + entry.FullName, true);
                                        

                                    }
                                }
                            }
                        }
                }

                nFile.Status = Status.Processed;
                _dataServices.UpdateCasheFiles(nFile);

                Directory.Delete(extractPath, true);
            }

        }

            


    }
}
