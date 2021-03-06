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

namespace DownLoaderZakupki.Core.Jobs
{
    partial class Parse44FilesJob
    {

        void ParseProtocols(List<FileCashes> FileCashes)
        {


            foreach (var nFile in FileCashes)
            {
                string zipPath = (_fzSettings44.WorkPath + nFile.Full_path);
                string extractPath = (_fzSettings44.WorkPath + "/extract" + nFile.Full_path);
                var protocols = new List<Protocols>();

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

                                        Console.WriteLine($"{hashstr}");

                                        using (StreamReader reader = new StreamReader(xmlin, Encoding.UTF8, false))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(export));

                                            XmlSerializer xmlser = new XmlSerializer(typeof(export));
                                            export exportd = xmlser.Deserialize(reader) as export;
                                            Console.WriteLine($"{exportd.ItemsElementName[0].ToString()}");


                                            var settings = new JsonSerializerSettings()
                                            {
                                                Formatting = Newtonsoft.Json.Formatting.Indented,
                                                TypeNameHandling = TypeNameHandling.Auto
                                            };

                                            switch (exportd.ItemsElementName[0].ToString())
                                            {
                                                case "epProtocolCancel": //epProtocolCancel;protocolCancelType1 - Информация об отмене протокола электронной процедуры;
                                                    {
                                                        protocolCancelType1 epProtocolCancel = exportd.Items[0] as protocolCancelType1;
                                                        string unf_json = JsonConvert.SerializeObject(epProtocolCancel);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = epProtocolCancel.commonInfo.purchaseNumber;
                                                        frpotocols.Protocol_num = epProtocolCancel.commonInfo.canceledProtocolNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = epProtocolCancel.commonInfo.publishDTInEIS;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "epProtocolEOK1": //epProtocolEOK1; protocolEOK1Type - Протокол рассмотрения и оценки первых частей заявок на участие в ЭOK;
                                                    {
                                                        protocolEOK1Type epProtocolEOK1 = exportd.Items[0] as protocolEOK1Type;
                                                        string unf_json = JsonConvert.SerializeObject(epProtocolEOK1);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = epProtocolEOK1.commonInfo.purchaseNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = epProtocolEOK1.commonInfo.publishDTInEIS;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }

                                                case "epProtocolEOK2": //epProtocolEOK2; protocolEOK2Type - Протокол рассмотрения и оценки вторых частей заявок на участие в ЭOK;
                                                    {
                                                        protocolEOK2Type epProtocolEOK2 = exportd.Items[0] as protocolEOK2Type;
                                                        string unf_json = JsonConvert.SerializeObject(epProtocolEOK2);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = epProtocolEOK2.commonInfo.purchaseNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = epProtocolEOK2.commonInfo.publishDTInEIS;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "epProtocolEOK3": //epProtocolEOK3;protocolEOK3Type - Протокол подведения итогов ЭOK;
                                                    {
                                                        protocolEOK3Type epProtocolEOK3 = exportd.Items[0] as protocolEOK3Type;
                                                        string unf_json = JsonConvert.SerializeObject(epProtocolEOK3);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = epProtocolEOK3.commonInfo.purchaseNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = epProtocolEOK3.commonInfo.publishDTInEIS;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "epProtocolEOKD1": //epProtocolEOKD1;protocolEOKD1Type - Протокол первого этапа ЭOKД;
                                                    {
                                                        protocolEOKD1Type epProtocolEOKD1 = exportd.Items[0] as protocolEOKD1Type;
                                                        string unf_json = JsonConvert.SerializeObject(epProtocolEOKD1);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = epProtocolEOKD1.commonInfo.purchaseNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = epProtocolEOKD1.commonInfo.publishDTInEIS;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "epProtocolEOKOU1": //epProtocolEOKOU1;protocolEOKOU1Type - Протокол рассмотрения и оценки первых частей заявок на участие в ЭOK-ОУ;
                                                    {
                                                        protocolEOKOU1Type epProtocolEOKOU1 = exportd.Items[0] as protocolEOKOU1Type;
                                                        string unf_json = JsonConvert.SerializeObject(epProtocolEOKOU1);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = epProtocolEOKOU1.commonInfo.purchaseNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = epProtocolEOKOU1.commonInfo.publishDTInEIS;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "epProtocolEOKOU2": //epProtocolEOKOU2; protocolEOKOU2Type - Протокол рассмотрения и оценки вторых частей заявок на участие в ЭOK-ОУ;
                                                    {
                                                        protocolEOKOU2Type epProtocolEOKOU2 = exportd.Items[0] as protocolEOKOU2Type;
                                                        string unf_json = JsonConvert.SerializeObject(epProtocolEOKOU2);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = epProtocolEOKOU2.commonInfo.purchaseNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = epProtocolEOKOU2.commonInfo.publishDTInEIS;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "epProtocolEOKOU3": //epProtocolEOKOU3; protocolEOKOU3Type - Протокол подведения итогов ЭOK - ОУ;
                                                    {
                                                        protocolEOKOU3Type epProtocolEOKOU3 = exportd.Items[0] as protocolEOKOU3Type;
                                                        string unf_json = JsonConvert.SerializeObject(epProtocolEOKOU3);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = epProtocolEOKOU3.commonInfo.purchaseNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = epProtocolEOKOU3.commonInfo.publishDTInEIS;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "epProtocolEOKOUSingleApp": //epProtocolEOKOUSingleApp; protocolEOKOUSingleAppType - Протокол рассмотрения единственной заявки на участие ЭOK-ОУ;
                                                    {
                                                        protocolEOKOUSingleAppType epProtocolEOKOUSingleApp = exportd.Items[0] as protocolEOKOUSingleAppType;
                                                        string unf_json = JsonConvert.SerializeObject(epProtocolEOKOUSingleApp);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = epProtocolEOKOUSingleApp.commonInfo.purchaseNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = epProtocolEOKOUSingleApp.commonInfo.publishDTInEIS;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "epProtocolEOKOUSinglePart": //epProtocolEOKOUSinglePart; protocolEOKOUSinglePartType - Протокол рассмотрения заявки единственного участника ЭOK - ОУ;
                                                    {
                                                        protocolEOKOUSinglePartType epProtocolEOKOUSinglePart = exportd.Items[0] as protocolEOKOUSinglePartType;
                                                        string unf_json = JsonConvert.SerializeObject(epProtocolEOKOUSinglePart);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = epProtocolEOKOUSinglePart.commonInfo.purchaseNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = epProtocolEOKOUSinglePart.commonInfo.publishDTInEIS;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "epProtocolEOKSingleApp": //epProtocolEOKSingleApp; protocolEOKSingleAppType - Протокол рассмотрения единственной заявки на участие ЭOK;
                                                    {
                                                        protocolEOKSingleAppType epProtocolEOKSingleApp = exportd.Items[0] as protocolEOKSingleAppType;
                                                        string unf_json = JsonConvert.SerializeObject(epProtocolEOKSingleApp);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = epProtocolEOKSingleApp.commonInfo.purchaseNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = epProtocolEOKSingleApp.commonInfo.publishDTInEIS;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "epProtocolEOKSinglePart": //epProtocolEOKSinglePart; protocolEOKSinglePartType - Протокол рассмотрения заявки единственного участника ЭOK;
                                                    {
                                                        protocolEOKSinglePartType epProtocolEOKSinglePart = exportd.Items[0] as protocolEOKSinglePartType;
                                                        string unf_json = JsonConvert.SerializeObject(epProtocolEOKSinglePart);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = epProtocolEOKSinglePart.commonInfo.purchaseNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = epProtocolEOKSinglePart.commonInfo.publishDTInEIS;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "epProtocolEZK1": //epProtocolEZK1;protocolEZK1Type - Протокол рассмотрения заявок на участие в ЭЗК;
                                                    {
                                                        protocolEZK1Type epProtocolEZK1 = exportd.Items[0] as protocolEZK1Type;
                                                        string unf_json = JsonConvert.SerializeObject(epProtocolEZK1);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = epProtocolEZK1.commonInfo.purchaseNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = epProtocolEZK1.commonInfo.publishDTInEIS;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "epProtocolEZK2": //epProtocolEZK2; protocolEZK2Type - Протокол рассмотрения и оценки заявок на участие в ЭЗК;
                                                    {
                                                        protocolEZK2Type epProtocolEZK2 = exportd.Items[0] as protocolEZK2Type;
                                                        string unf_json = JsonConvert.SerializeObject(epProtocolEZK2);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = epProtocolEZK2.commonInfo.purchaseNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = epProtocolEZK2.commonInfo.publishDTInEIS;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "epProtocolEZP1": //epProtocolEZP1;protocolEZP1Type - Протокол проведения ЭЗП;
                                                    {
                                                        protocolEZP1Type epProtocolEZP1 = exportd.Items[0] as protocolEZP1Type;
                                                        string unf_json = JsonConvert.SerializeObject(epProtocolEZP1);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = epProtocolEZP1.commonInfo.purchaseNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = epProtocolEZP1.commonInfo.publishDTInEIS;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "epProtocolEZP1Extract": //epProtocolEZP1Extract;protocolEZP1ExtractType - Выписка из протокола проведения ЭЗП;
                                                    {
                                                        protocolEZP1ExtractType epProtocolEZP1Extract = exportd.Items[0] as protocolEZP1ExtractType;
                                                        string unf_json = JsonConvert.SerializeObject(epProtocolEZP1Extract);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = epProtocolEZP1Extract.commonInfo.purchaseNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = epProtocolEZP1Extract.commonInfo.publishDTInEIS;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "epProtocolEZP2": //epProtocolEZP2;protocolEZP2Type - Итоговый протокол ЭЗП;
                                                    {
                                                        protocolEZP2Type epProtocolEZP2 = exportd.Items[0] as protocolEZP2Type;
                                                        string unf_json = JsonConvert.SerializeObject(epProtocolEZP2);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = epProtocolEZP2.commonInfo.purchaseNumber;
                                                        //frpotocols.Protocol_num = epProtocolEZP2.protocolNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = epProtocolEZP2.commonInfo.publishDTInEIS;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }

                                                
                                                case "fcsProtocolCancel": //fcsProtocolCancel;zfcs_protocolCancelType - Информация об отмене протокола;
                                                    {
                                                        zfcs_protocolCancelType fcsProtocolCancel = exportd.Items[0] as zfcs_protocolCancelType;
                                                        string unf_json = JsonConvert.SerializeObject(fcsProtocolCancel);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = fcsProtocolCancel.purchaseNumber;
                                                        frpotocols.Protocol_num = fcsProtocolCancel.protocolNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = fcsProtocolCancel.docPublishDate;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "fcsProtocolDeviation": //fcsProtocolDeviation;zfcs_protocolDeviationType - Протокол признания участника уклонившимся от заключения контракта; внесение изменений;
                                                    {
                                                        zfcs_protocolDeviationType fcsProtocolDeviation = exportd.Items[0] as zfcs_protocolDeviationType;
                                                        string unf_json = JsonConvert.SerializeObject(fcsProtocolDeviation);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = fcsProtocolDeviation.purchaseNumber;
                                                        frpotocols.Protocol_num = fcsProtocolDeviation.protocolNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = fcsProtocolDeviation.publishDate;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }

                                                
                                                case "fcsProtocolEF1": //fcsProtocolEF1;zfcs_protocolEF1Type - Протокол рассмотрения заявок на участие в электронном аукционе;
                                                    {
                                                        zfcs_protocolEF1Type fcsProtocolEF1 = exportd.Items[0] as zfcs_protocolEF1Type;
                                                        string unf_json = JsonConvert.SerializeObject(fcsProtocolEF1);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = fcsProtocolEF1.purchaseNumber;
                                                        frpotocols.Protocol_num = fcsProtocolEF1.protocolNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = fcsProtocolEF1.publishDate;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "fcsProtocolEF2": //fcsProtocolEF2; zfcs_protocolEF2Type - Протокол проведения электронного аукциона;
                                                    {
                                                        zfcs_protocolEF2Type fcsProtocolEF2 = exportd.Items[0] as zfcs_protocolEF2Type;
                                                        string unf_json = JsonConvert.SerializeObject(fcsProtocolEF2);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = fcsProtocolEF2.purchaseNumber;
                                                        frpotocols.Protocol_num = fcsProtocolEF2.protocolNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = fcsProtocolEF2.publishDate;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "fcsProtocolEF3": //fcsProtocolEF3; zfcs_protocolEF3Type - Протокол подведения итогов электронного аукциона;
                                                    {
                                                        zfcs_protocolEF3Type fcsProtocolEF3 = exportd.Items[0] as zfcs_protocolEF3Type;
                                                        string unf_json = JsonConvert.SerializeObject(fcsProtocolEF3);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = fcsProtocolEF3.purchaseNumber;
                                                        frpotocols.Protocol_num = fcsProtocolEF3.protocolNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = fcsProtocolEF3.publishDate;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "fcsProtocolEFInvalidation": //fcsProtocolEFInvalidation; zfcs_protocolEFInvalidationType - Протокол о признании электронного аукциона несостоявшимся;
                                                    {
                                                        zfcs_protocolEFInvalidationType fcsProtocolEFInvalidation = exportd.Items[0] as zfcs_protocolEFInvalidationType;
                                                        string unf_json = JsonConvert.SerializeObject(fcsProtocolEFInvalidation);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = fcsProtocolEFInvalidation.purchaseNumber;
                                                        frpotocols.Protocol_num = fcsProtocolEFInvalidation.protocolNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = fcsProtocolEFInvalidation.publishDate;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "fcsProtocolEFSingleApp": //fcsProtocolEFSingleApp; zfcs_protocolEFSingleAppType - Протокол рассмотрения единственной заявки на участие в электронном аукционе;
                                                    {
                                                        zfcs_protocolEFSingleAppType fcsProtocolEFSingleApp = exportd.Items[0] as zfcs_protocolEFSingleAppType;
                                                        string unf_json = JsonConvert.SerializeObject(fcsProtocolEFSingleApp);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = fcsProtocolEFSingleApp.purchaseNumber;
                                                        frpotocols.Protocol_num = fcsProtocolEFSingleApp.protocolNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = fcsProtocolEFSingleApp.publishDate;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "fcsProtocolEFSinglePart": //fcsProtocolEFSinglePart; zfcs_protocolEFSinglePartType - Протокол рассмотрения заявки единственного участника электронного аукциона;
                                                    {
                                                        zfcs_protocolEFSinglePartType fcsProtocolEFSinglePart = exportd.Items[0] as zfcs_protocolEFSinglePartType;
                                                        string unf_json = JsonConvert.SerializeObject(fcsProtocolEFSinglePart);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = fcsProtocolEFSinglePart.purchaseNumber;
                                                        frpotocols.Protocol_num = fcsProtocolEFSinglePart.protocolNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = fcsProtocolEFSinglePart.publishDate;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "fcsProtocolEvasion": //fcsProtocolEvasion; zfcs_protocolEvasionType - Протокол отказа от заключения контракта; внесение изменений;
                                                    {
                                                        zfcs_protocolEvasionType fcsProtocolEvasion = exportd.Items[0] as zfcs_protocolEvasionType;
                                                        string unf_json = JsonConvert.SerializeObject(fcsProtocolEvasion);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = fcsProtocolEvasion.purchaseNumber;
                                                        frpotocols.Protocol_num = fcsProtocolEvasion.protocolNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = fcsProtocolEvasion.publishDate;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "fcsProtocolOK1": //fcsProtocolOK1; zfcs_protocolOK1Type - Протокол вскрытия конвертов с заявками на участие в ОК; внесение изменений;
                                                    {
                                                        zfcs_protocolOK1Type fcsProtocolOK1 = exportd.Items[0] as zfcs_protocolOK1Type;
                                                        string unf_json = JsonConvert.SerializeObject(fcsProtocolOK1);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = fcsProtocolOK1.purchaseNumber;
                                                        frpotocols.Protocol_num = fcsProtocolOK1.protocolNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = fcsProtocolOK1.publishDate;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "fcsProtocolOK2": //fcsProtocolOK2; zfcs_protocolOK2Type - Протокол рассмотрения и оценки заявок на участие в конкурсе в ОК; внесение изменений;
                                                    {
                                                        zfcs_protocolOK2Type fcsProtocolOK2 = exportd.Items[0] as zfcs_protocolOK2Type;
                                                        string unf_json = JsonConvert.SerializeObject(fcsProtocolOK2);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = fcsProtocolOK2.purchaseNumber;
                                                        frpotocols.Protocol_num = fcsProtocolOK2.protocolNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = fcsProtocolOK2.publishDate;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "fcsProtocolOKSingleApp": //fcsProtocolOKSingleApp;zfcs_protocolOKSingleAppType - Протокол рассмотрения единственной заявки в ОК; внесение изменений; 
                                                    {
                                                        zfcs_protocolOKSingleAppType fcsProtocolOKSingleApp = exportd.Items[0] as zfcs_protocolOKSingleAppType;
                                                        string unf_json = JsonConvert.SerializeObject(fcsProtocolOKSingleApp);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = fcsProtocolOKSingleApp.purchaseNumber;
                                                        frpotocols.Protocol_num = fcsProtocolOKSingleApp.protocolNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = fcsProtocolOKSingleApp.publishDate;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "fcsProtocolPO": //fcsProtocolPO;zfcs_protocolPOType - Протокол предварительного отбора в ПО; внесение изменений;
                                                    {
                                                        zfcs_protocolPOType fcsProtocolPO = exportd.Items[0] as zfcs_protocolPOType;
                                                        string unf_json = JsonConvert.SerializeObject(fcsProtocolPO);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = fcsProtocolPO.purchaseNumber;
                                                        frpotocols.Protocol_num = fcsProtocolPO.protocolNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = fcsProtocolPO.publishDate;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "fcsProtocolZK": //fcsProtocolZK;zfcs_protocolZKType - Протокол рассмотрения и оценки заявок в ЗК;
                                                    {
                                                        zfcs_protocolZKType fcsProtocolZK = exportd.Items[0] as zfcs_protocolZKType;
                                                        string unf_json = JsonConvert.SerializeObject(fcsProtocolZK);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = fcsProtocolZK.purchaseNumber;
                                                        frpotocols.Protocol_num = fcsProtocolZK.protocolNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = fcsProtocolZK.publishDate;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }

                                                
                                                case "pprf615ProtocolEF1": //pprf615ProtocolEF1; protocolEF1Type - Протокол рассмотрения заявок на участие в электронном аукционе по ПП РФ № 615; внесение изменений;
                                                    {
                                                        protocolEF1Type pprf615ProtocolEF1 = exportd.Items[0] as protocolEF1Type;
                                                        string unf_json = JsonConvert.SerializeObject(pprf615ProtocolEF1);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = pprf615ProtocolEF1.commonInfo.purchaseNumber;
                                                        frpotocols.Protocol_num = pprf615ProtocolEF1.commonInfo.docNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = pprf615ProtocolEF1.commonInfo.docPublishDate;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "pprf615ProtocolEF2": //pprf615ProtocolEF2; protocolEF2Type - Протокол проведения электронного аукциона по ПП РФ № 615; внесение изменений;
                                                    {
                                                        protocolEF2Type pprf615ProtocolEF2 = exportd.Items[0] as protocolEF2Type;
                                                        string unf_json = JsonConvert.SerializeObject(pprf615ProtocolEF2);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = pprf615ProtocolEF2.commonInfo.purchaseNumber;
                                                        frpotocols.Protocol_num = pprf615ProtocolEF2.commonInfo.docNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = pprf615ProtocolEF2.commonInfo.docPublishDate;
                                                        protocols.Add(frpotocols);
                                                        break;
                                                    }
                                                
                                                case "pprf615ProtocolPO": //pprf615ProtocolPO; protocolPOType - Протокол предварительного отбора в ПО по ПП РФ № 615; внесение изменений;
                                                    {
                                                        protocolPOType pprf615ProtocolPO = exportd.Items[0] as protocolPOType;
                                                        string unf_json = JsonConvert.SerializeObject(pprf615ProtocolPO);

                                                        var frpotocols = new Protocols();
                                                        frpotocols.Purchase_num = pprf615ProtocolPO.commonInfo.purchaseNumber;
                                                        frpotocols.Protocol_num = pprf615ProtocolPO.commonInfo.docNumber;
                                                        frpotocols.R_body = unf_json;
                                                        frpotocols.Xml_body = read_xml_text;
                                                        frpotocols.Hash = hashstr;
                                                        frpotocols.Zip_file = nFile.Full_path;
                                                        frpotocols.File_name = entry.FullName;
                                                        frpotocols.Fz_type = 44;
                                                        frpotocols.Type_protocol = exportd.Items[0].GetType().Name;
                                                        frpotocols.PublishDate = pprf615ProtocolPO.commonInfo.docPublishDate;
                                                        protocols.Add(frpotocols);
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

                                                        using (StreamWriter sw1 = new StreamWriter(@$"D:\\FZ\\Types44\\Protocols\\{fnel}", true, System.Text.Encoding.Default))
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


                Console.WriteLine($"Всего добавляется записей в БД: {protocols.Count}");
                _dataServices.SaveProtocols(protocols);
                nFile.Status = Status.Processed;
                _dataServices.UpdateCasheFiles(nFile);

                Directory.Delete(extractPath, true);
            }


        }
    }
}
