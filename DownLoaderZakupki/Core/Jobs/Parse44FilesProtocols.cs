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
                                                //epProtocolEOK1; protocolEOK1Type - Протокол рассмотрения и оценки первых частей заявок на участие в ЭOK;
                                                //epProtocolEOK2; protocolEOK2Type - Протокол рассмотрения и оценки вторых частей заявок на участие в ЭOK;
                                                //epProtocolEOK3;protocolEOK3Type - Протокол подведения итогов ЭOK;
                                                //epProtocolEOKD1;protocolEOKD1Type - Протокол первого этапа ЭOKД;
                                                //epProtocolEOKOU1;protocolEOKOU1Type - Протокол рассмотрения и оценки первых частей заявок на участие в ЭOK-ОУ;
                                                //epProtocolEOKOU2; protocolEOKOU2Type - Протокол рассмотрения и оценки вторых частей заявок на участие в ЭOK-ОУ;
                                                //epProtocolEOKOU3; protocolEOKOU3Type - Протокол подведения итогов ЭOK - ОУ;
                                                //epProtocolEOKOUSingleApp; protocolEOKOUSingleAppType - Протокол рассмотрения единственной заявки на участие ЭOK-ОУ;
                                                //epProtocolEOKOUSinglePart; protocolEOKOUSinglePartType - Протокол рассмотрения заявки единственного участника ЭOK - ОУ;
                                                //epProtocolEOKSingleApp; protocolEOKSingleAppType - Протокол рассмотрения единственной заявки на участие ЭOK;
                                                //epProtocolEOKSinglePart; protocolEOKSinglePartType - Протокол рассмотрения заявки единственного участника ЭOK;
                                                //epProtocolEZK1;protocolEZK1Type - Протокол рассмотрения заявок на участие в ЭЗК;
                                                //epProtocolEZK2; protocolEZK2Type - Протокол рассмотрения и оценки заявок на участие в ЭЗК;
                                                //epProtocolEZP1;protocolEZP1Type - Протокол проведения ЭЗП;
                                                //epProtocolEZP1Extract;protocolEZP1ExtractType - Выписка из протокола проведения ЭЗП;
                                                //epProtocolEZP2;protocolEZP2Type - Итоговый протокол ЭЗП;

                                                //fcsProtocolCancel;zfcs_protocolCancelType - Информация об отмене протокола;
                                                //fcsProtocolDeviation;zfcs_protocolDeviationType - Протокол признания участника уклонившимся от заключения контракта; внесение изменений;

                                                //fcsProtocolEF1;zfcs_protocolEF1Type - Протокол рассмотрения заявок на участие в электронном аукционе;
                                                //fcsProtocolEF2; zfcs_protocolEF2Type - Протокол проведения электронного аукциона;
                                                //fcsProtocolEF3; zfcs_protocolEF3Type - Протокол подведения итогов электронного аукциона;
                                                //fcsProtocolEFInvalidation; zfcs_protocolEFInvalidationType - Протокол о признании электронного аукциона несостоявшимся;
                                                //fcsProtocolEFSingleApp; zfcs_protocolEFSingleAppType - Протокол рассмотрения единственной заявки на участие в электронном аукционе;
                                                //fcsProtocolEFSinglePart; zfcs_protocolEFSinglePartType - Протокол рассмотрения заявки единственного участника электронного аукциона;
                                                //fcsProtocolEvasion; zfcs_protocolEvasionType - Протокол отказа от заключения контракта; внесение изменений;
                                                //fcsProtocolOK1; zfcs_protocolOK1Type - Протокол вскрытия конвертов с заявками на участие в ОК; внесение изменений;
                                                //fcsProtocolOK2; zfcs_protocolOK2Type - Протокол рассмотрения и оценки заявок на участие в конкурсе в ОК; внесение изменений;
                                                //fcsProtocolOKSingleApp;zfcs_protocolOKSingleAppType - Протокол рассмотрения единственной заявки в ОК; внесение изменений; 
                                                //fcsProtocolPO;zfcs_protocolPOType - Протокол предварительного отбора в ПО; внесение изменений;
                                                //fcsProtocolZK;zfcs_protocolZKType - Протокол рассмотрения и оценки заявок в ЗК;

                                                //pprf615ProtocolEF1; protocolEF1Type - Протокол рассмотрения заявок на участие в электронном аукционе по ПП РФ № 615; внесение изменений;
                                                //pprf615ProtocolEF2; protocolEF2Type - Протокол проведения электронного аукциона по ПП РФ № 615; внесение изменений;
                                                //pprf615ProtocolPO; protocolPOType - Протокол предварительного отбора в ПО по ПП РФ № 615; внесение изменений;




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
