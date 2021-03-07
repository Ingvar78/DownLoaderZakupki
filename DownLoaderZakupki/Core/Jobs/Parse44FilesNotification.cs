using DownLoaderZakupki.Data.DB;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using DownLoaderZakupki.Models.Enum;
using DownLoaderZakupki.Models.Ext.Fz44;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Xml.Serialization;
using System.Threading.Tasks;

namespace DownLoaderZakupki.Core.Jobs
{
    partial class Parse44FilesJob
    {

        void ParseNnotifications(List<FileCashes> FileCashes)
        {
            //Обрабатываем данный тип;

            Parallel.ForEach(FileCashes,
                new ParallelOptions { MaxDegreeOfParallelism = _fzSettings44.Parallels },
                (nFile) =>

                //foreach (var nFile in FileCashes)
                {
                string zipPath = (_fzSettings44.WorkPath + nFile.Full_path);
                string extractPath = (_fzSettings44.WorkPath + "/extract" + nFile.Full_path);
                var notifications = new List<Notifications>();

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
                                        string read_xml_text;
                                        using (var streamReader = new StreamReader(xmlin, Encoding.UTF8, false))
                                        {
                                            read_xml_text = streamReader.ReadToEnd();
                                        }

                                        var strBuilder = new StringBuilder();
                                        using (var hash = SHA256.Create())
                                        {
                                            //Getting hashed byte array
                                            var result = hash.ComputeHash(Encoding.UTF8.GetBytes(read_xml_text));
                                            foreach (var b in result)
                                                strBuilder.Append(b.ToString("x2")); //Byte as hexadecimal format
                                        }

                                        var hashstr = strBuilder.ToString();

                                        //Console.WriteLine($"{hashstr}");

                                        using (StreamReader reader = new StreamReader(xmlin, Encoding.UTF8, false))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(export));

                                            XmlSerializer xmlser = new XmlSerializer(typeof(export));
                                            export exportd = xmlser.Deserialize(reader) as export;
                                            //Console.WriteLine($"{exportd.ItemsElementName[0].ToString()}");


                                            var settings = new JsonSerializerSettings()
                                            {
                                                Formatting = Newtonsoft.Json.Formatting.Indented,
                                                TypeNameHandling = TypeNameHandling.Auto
                                            };

                                            switch (exportd.ItemsElementName[0].ToString())
                                            {
                                                case "epClarificationDoc": //epClarificationDoc; clarificationDocType - Разъяснение положений документации;
                                                    {
                                                        clarificationDocType clarificationDoc = exportd.Items[0] as clarificationDocType;
                                                        string unf_json = JsonConvert.SerializeObject(clarificationDoc);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = clarificationDoc.commonInfo.purchaseNumber;
                                                        fscn.PublishDate = clarificationDoc.commonInfo.docPublishDTInEIS;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        //SaveNotification(pnum, exp_json, etype, zipPath, xml_f_name, 44, pdate);
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }

                                                case "epClarificationResult": //epClarificationResult; clarificationResultType - Запрос о даче разъяснений результатов;
                                                    {
                                                        clarificationResultType clarificationResult = exportd.Items[0] as clarificationResultType;
                                                        string unf_json = JsonConvert.SerializeObject(clarificationResult);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = clarificationResult.commonInfo.purchaseNumber;
                                                        fscn.PublishDate = clarificationResult.commonInfo.docPublishDTInEIS;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        //SaveNotification(pnum, exp_json, etype, zipPath, xml_f_name, 44, pdate);
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }

                                                case "epNotificationCancel": //epNotificationCancel;notificationCancelType1 - Извещение об отмене определения поставщика (подрядчика, исполнителя) в электронной форме;
                                                    {
                                                        notificationCancelType1 notificationCancel = exportd.Items[0] as notificationCancelType1;
                                                        string unf_json = JsonConvert.SerializeObject(notificationCancel);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = notificationCancel.commonInfo.purchaseNumber;
                                                        fscn.PublishDate = notificationCancel.commonInfo.docPublishDTInEIS;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        //SaveNotification(pnum, exp_json, etype, zipPath, xml_f_name, 44, pdate);
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }

                                                case "epNotificationCancelFailure": //epNotificationCancelFailure;notificationCancelFailureType - Отмена извещения об отмене определения поставщика (подрядчика, исполнителя) в электронной форме;
                                                    {
                                                        notificationCancelFailureType notificationCancelF = exportd.Items[0] as notificationCancelFailureType;
                                                        string unf_json = JsonConvert.SerializeObject(notificationCancelF);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = notificationCancelF.commonInfo.purchaseNumber;
                                                        fscn.PublishDate = notificationCancelF.commonInfo.docPublishDTInEIS;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        //SaveNotification(pnum, exp_json, etype, zipPath, xml_f_name, 44, pdate);
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }

                                                case "epNotificationEOK": //epNotificationEOK;notificationEOKType - Извещение о проведении ЭOK;
                                                    {
                                                        notificationEOKType notificationEOK = exportd.Items[0] as notificationEOKType;
                                                        string unf_json = JsonConvert.SerializeObject(notificationEOK);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = notificationEOK.commonInfo.purchaseNumber;
                                                        fscn.PublishDate = notificationEOK.commonInfo.publishDTInEIS;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        //SaveNotification(pnum, exp_json, etype, zipPath, xml_f_name, 44, pdate);
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }

                                                case "epNotificationEOKOU": //epNotificationEOKOU;notificationEOKOUType - Извещение о проведении ЭOK-ОУ
                                                    {
                                                        notificationEOKOUType notificationEOKOU = exportd.Items[0] as notificationEOKOUType;
                                                        string unf_json = JsonConvert.SerializeObject(notificationEOKOU);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = notificationEOKOU.commonInfo.purchaseNumber;
                                                        fscn.PublishDate = notificationEOKOU.commonInfo.publishDTInEIS;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }

                                                case "epNotificationEZK": //epNotificationEZK;notificationEZKType - Извещение о проведении ЭЗК;
                                                    {
                                                        notificationEZKType notificationEZK = exportd.Items[0] as notificationEZKType;
                                                        string unf_json = JsonConvert.SerializeObject(notificationEZK);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = notificationEZK.commonInfo.purchaseNumber;
                                                        fscn.PublishDate = notificationEZK.commonInfo.publishDTInEIS;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }

                                                case "epNotificationEZP": //epNotificationEZP;notificationEZPType - Извещение о проведении ЭЗП;
                                                    {
                                                        notificationEZPType notificationEZP = exportd.Items[0] as notificationEZPType;
                                                        string unf_json = JsonConvert.SerializeObject(notificationEZP);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = notificationEZP.commonInfo.purchaseNumber;
                                                        fscn.PublishDate = notificationEZP.commonInfo.publishDTInEIS;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }

                                                case "epProlongationEOK": //epProlongationEOK;prolongationEOKType - Извещение о продлении срока подачи заявок на участие в ЭOK;
                                                    {
                                                        prolongationEOKType prolongationEOK = exportd.Items[0] as prolongationEOKType;
                                                        string unf_json = JsonConvert.SerializeObject(prolongationEOK);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = prolongationEOK.commonInfo.purchaseNumber;
                                                        fscn.PublishDate = prolongationEOK.commonInfo.docPublishDTInEIS;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }

                                                case "epProlongationEOKOU": //epProlongationEOKOU; prolongationEOKOUType - Извещение о продлении срока подачи заявок на участие в ЭOK - ОУ;
                                                    {
                                                        prolongationEOKOUType prolongationEOKOU = exportd.Items[0] as prolongationEOKOUType;
                                                        string unf_json = JsonConvert.SerializeObject(prolongationEOKOU);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = prolongationEOKOU.commonInfo.purchaseNumber;
                                                        fscn.PublishDate = prolongationEOKOU.commonInfo.docPublishDTInEIS;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }

                                                case "epProlongationEZK": //epProlongationEZK; prolongationEZKType - Извещение о продлении срока подачи заявок на участие в ЭЗК;
                                                    {
                                                        prolongationEZKType prolongationEZKT = exportd.Items[0] as prolongationEZKType;
                                                        string unf_json = JsonConvert.SerializeObject(prolongationEZKT);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = prolongationEZKT.commonInfo.purchaseNumber;
                                                        fscn.PublishDate = prolongationEZKT.commonInfo.docPublishDTInEIS;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }

                                                case "fcs_notificationEFDateChange": //fcs_notificationEFDateChange; zfcs_notificationEFDateChangeType - Уведомление об изменении даты и времени проведения электронного аукциона
                                                    {
                                                        zfcs_notificationEFDateChangeType zfcsnEFDC = exportd.Items[0] as zfcs_notificationEFDateChangeType;
                                                        string unf_json = JsonConvert.SerializeObject(zfcsnEFDC);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = zfcsnEFDC.purchaseNumber;
                                                        fscn.PublishDate = zfcsnEFDC.docPublishDate;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }

                                                case "fcsClarification": //fcsClarification; zfcs_clarificationType - Разъяснение положений документации;
                                                    {
                                                        zfcs_clarificationType zfcs_clarification = exportd.Items[0] as zfcs_clarificationType;
                                                        string unf_json = JsonConvert.SerializeObject(zfcs_clarification);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = zfcs_clarification.purchaseNumber;
                                                        fscn.PublishDate = zfcs_clarification.docPublishDate;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }

                                                case "fcsNotification111": //fcsNotification111; zfcs_notificationI111Type - Извещение о проведении закупки способом определения поставщика(подрядчика, исполнителя),
                                                                           //установленным Правительством Российской Федерации в соответствии со ст. 111 Федерального закона № 44 - ФЗ;
                                                    {
                                                        zfcs_notificationI111Type zfcs_notificationI111 = exportd.Items[0] as zfcs_notificationI111Type;
                                                        string unf_json = JsonConvert.SerializeObject(zfcs_notificationI111);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = zfcs_notificationI111.purchaseNumber;
                                                        fscn.PublishDate = zfcs_notificationI111.docPublishDate;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }

                                                case "fcsNotificationCancel": //fcsNotificationCancel; zfcs_notificationCancelType - Извещение об отмене определения поставщика(подрядчика, исполнителя);
                                                    {
                                                        zfcs_notificationCancelType zfcs_notificationCancel = exportd.Items[0] as zfcs_notificationCancelType;
                                                        string unf_json = JsonConvert.SerializeObject(zfcs_notificationCancel);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = zfcs_notificationCancel.purchaseNumber;
                                                        fscn.PublishDate = zfcs_notificationCancel.docPublishDate;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }
                                                case "fcsNotificationCancelFailure": //fcsNotificationCancelFailure; zfcs_notificationCancelFailureType - Отмена извещения об отмене определения поставщика(подрядчика, исполнителя)(в части лота);
                                                    {
                                                        zfcs_notificationCancelFailureType zfcs_notificationCancelFailure = exportd.Items[0] as zfcs_notificationCancelFailureType;
                                                        string unf_json = JsonConvert.SerializeObject(zfcs_notificationCancelFailure);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = zfcs_notificationCancelFailure.purchaseNumber;
                                                        fscn.PublishDate = zfcs_notificationCancelFailure.docPublishDate;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }
                                                case "fcsNotificationEF": //fcsNotificationEF; zfcs_notificationEFType - Извещение о проведении ЭА(электронный аукцион); внесение изменений;
                                                    {
                                                        zfcs_notificationEFType zfcs_notificationEF = exportd.Items[0] as zfcs_notificationEFType;
                                                        string unf_json = JsonConvert.SerializeObject(zfcs_notificationEF);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = zfcs_notificationEF.purchaseNumber;
                                                        fscn.PublishDate = zfcs_notificationEF.docPublishDate;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }
                                                case "fcsNotificationZakA": //fcsNotificationZakA; zfcs_notificationZakAType - Извещение о проведении ЗакА(закрытый аукцион); внесение изменений;
                                                    {
                                                        zfcs_notificationZakAType zfcs_notificationZakA = exportd.Items[0] as zfcs_notificationZakAType;
                                                        string unf_json = JsonConvert.SerializeObject(zfcs_notificationZakA);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = zfcs_notificationZakA.purchaseNumber;
                                                        fscn.PublishDate = zfcs_notificationZakA.docPublishDate;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }

                                                case "fcsPlacementResult": //fcsPlacementResult; zfcs_placementResultType - Результат проведения процедуры определения поставщика;
                                                    {
                                                        zfcs_placementResultType zfcs_placementResult = exportd.Items[0] as zfcs_placementResultType;
                                                        string unf_json = JsonConvert.SerializeObject(zfcs_placementResult);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = zfcs_placementResult.purchaseNumber;
                                                        fscn.ProtocolNum = zfcs_placementResult.protocolNumber;
                                                        fscn.PublishDate = zfcs_placementResult.createDate;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }

                                                case "pprf615NotificationCancel": //pprf615NotificationCancel; notificationCancelType - Извещение об отмене закупки по ПП РФ № 615;
                                                    {
                                                        notificationCancelType pprf615NotificationCancel = exportd.Items[0] as notificationCancelType;
                                                        string unf_json = JsonConvert.SerializeObject(pprf615NotificationCancel);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = pprf615NotificationCancel.commonInfo.purchaseNumber;
                                                        //fscn.ProtocolNum = pprf615NotificationCancel.cancelReasonInfo;
                                                        fscn.PublishDate = pprf615NotificationCancel.commonInfo.docPublishDate;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }
                                                case "pprf615NotificationEF": //pprf615NotificationEF; notificationEFType - Извещение о проведении ЭА(электронный аукцион) по ПП РФ № 615; внесение изменений;
                                                    {
                                                        notificationEFType notificationEF = exportd.Items[0] as notificationEFType;
                                                        string unf_json = JsonConvert.SerializeObject(notificationEF);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = notificationEF.commonInfo.purchaseNumber;
                                                        //fscn.ProtocolNum = notificationEF.purchaseResponsibleInfo;
                                                        fscn.PublishDate = notificationEF.commonInfo.docPublishDate;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }

                                                case "pprf615NotificationPO": //pprf615NotificationPO; notificationPOType - Извещение о проведении ПО(предварительный отбор) по ПП РФ № 615; внесение изменений;
                                                    {
                                                        notificationPOType notificationPO = exportd.Items[0] as notificationPOType;
                                                        string unf_json = JsonConvert.SerializeObject(notificationPO);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = notificationPO.commonInfo.purchaseNumber;
                                                        //fscn.ProtocolNum = notificationPO.commonInfo.
                                                        fscn.PublishDate = notificationPO.commonInfo.docPublishDate;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }

                                                case "epProlongationCancelEOK": //epProlongationCancelEOK; prolongationCancelEOKType - Отмена извещения о продлении срока подачи заявок на участие в ЭОK
                                                    {
                                                        prolongationCancelEOKType prolongationCancelEOK = exportd.Items[0] as prolongationCancelEOKType;
                                                        string unf_json = JsonConvert.SerializeObject(prolongationCancelEOK);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = prolongationCancelEOK.commonInfo.purchaseNumber;
                                                        //fscn.ProtocolNum = notificationPO.commonInfo.
                                                        fscn.PublishDate = prolongationCancelEOK.commonInfo.docPublishDTInEIS;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }

                                                case "epProlongationCancelEZK": //epProlongationCancelEZK; prolongationCancelEZKType - Отмена извещения о продлении срока подачи заявок на участие в ЭЗK
                                                    {
                                                        prolongationCancelEZKType prolongationCancelEZK = exportd.Items[0] as prolongationCancelEZKType;
                                                        string unf_json = JsonConvert.SerializeObject(prolongationCancelEZK);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = prolongationCancelEZK.commonInfo.purchaseNumber;
                                                        //fscn.ProtocolNum = notificationPO.commonInfo.
                                                        fscn.PublishDate = prolongationCancelEZK.commonInfo.docPublishDTInEIS;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }

                                                case "fcsNotificationOK": //fcsNotificationOK; zfcs_notificationOKType - Извещение о проведении OK(открытый конкурс); внесение изменений;
                                                    {
                                                        zfcs_notificationOKType fcsNotificationOK = exportd.Items[0] as zfcs_notificationOKType;
                                                        string unf_json = JsonConvert.SerializeObject(fcsNotificationOK);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = fcsNotificationOK.purchaseNumber;
                                                        //fscn.ProtocolNum = notificationPO.commonInfo.
                                                        fscn.PublishDate = fcsNotificationOK.docPublishDate;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }

                                                case "fcsNotificationOrgChange": //fcsNotificationOrgChange; zfcs_notificationOrgChangeType - Уведомление об изменении организации, осуществляющей закупку;
                                                    {
                                                        zfcs_notificationOrgChangeType fcsNotificationOrgChange = exportd.Items[0] as zfcs_notificationOrgChangeType;
                                                        string unf_json = JsonConvert.SerializeObject(fcsNotificationOrgChange);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = fcsNotificationOrgChange.purchase.purchaseNumber;
                                                        //fscn.ProtocolNum = notificationPO.commonInfo.
                                                        fscn.PublishDate = fcsNotificationOrgChange.docPublishDate;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }

                                                case "fcsNotificationZakK": //fcsNotificationZakK; zfcs_notificationZakKType - Извещение о проведении ЗакK(закрытый конкурс); внесение изменений;
                                                    {
                                                        zfcs_notificationZakKType fcsNotificationZakK = exportd.Items[0] as zfcs_notificationZakKType;
                                                        string unf_json = JsonConvert.SerializeObject(fcsNotificationZakK);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = fcsNotificationZakK.purchaseNumber;
                                                        //fscn.ProtocolNum = notificationPO.commonInfo.
                                                        fscn.PublishDate = fcsNotificationZakK.docPublishDate;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }

                                                case "fcsNotificationZakKOU": //fcsNotificationZakKOU; zfcs_notificationZakKOUType - Извещение о проведении ЗакK - ОУ(закрытый конкурс с ограниченным участием); внесение изменений;
                                                    {
                                                        zfcs_notificationZakKOUType fcsNotificationZakKOU = exportd.Items[0] as zfcs_notificationZakKOUType;
                                                        string unf_json = JsonConvert.SerializeObject(fcsNotificationZakKOU);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = fcsNotificationZakKOU.purchaseNumber;
                                                        //fscn.ProtocolNum = notificationPO.commonInfo.
                                                        fscn.PublishDate = fcsNotificationZakKOU.docPublishDate;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
                                                        break;
                                                    }

                                                case "fcsNotificationZK": //fcsNotificationZK; zfcs_notificationZKType - Извещение о проведении ЗK(запрос котировок); внесение изменений;
                                                    {
                                                        zfcs_notificationZKType fcsNotificationZK = exportd.Items[0] as zfcs_notificationZKType;
                                                        string unf_json = JsonConvert.SerializeObject(fcsNotificationZK);

                                                        var fscn = new Notifications();

                                                        //fscn.Wname = "";
                                                        fscn.R_body = unf_json;
                                                        fscn.Xml_body = read_xml_text;
                                                        fscn.Hash = hashstr;
                                                        fscn.ContractConclusion = "";
                                                        fscn.Inn = "";
                                                        fscn.AppRating = 0;
                                                        fscn.Zip_file = nFile.Full_path;
                                                        fscn.File_name = entry.FullName;
                                                        fscn.Fz_type = 44;
                                                        //fscn.JournalNumber = ;
                                                        fscn.Purchase_num = fcsNotificationZK.purchaseNumber;
                                                        //fscn.ProtocolNum = notificationPO.commonInfo.
                                                        fscn.PublishDate = fcsNotificationZK.docPublishDate;
                                                        fscn.Type_notif = exportd.Items[0].GetType().Name;
                                                        notifications.Add(fscn);
                                                        //_dataServices.SaveNotification(notifications);
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
                                                        string fnel = $"{exportd.ItemsElementName[0].ToString()}";

                                                        using (StreamWriter sw1 = new StreamWriter(@$"D:\\FZ\\Types44\\Notifications\\{fnel}", true, System.Text.Encoding.Default))
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


                Console.WriteLine($"Всего добавляется записей в БД: {notifications.Count}");
                _dataServices.SaveNotification(notifications);
                nFile.Status = Status.Processed;
                _dataServices.UpdateCasheFiles(nFile);

                Directory.Delete(extractPath, true);
            });

        }

    }
}
