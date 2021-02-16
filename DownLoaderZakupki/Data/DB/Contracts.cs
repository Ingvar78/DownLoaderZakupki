using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DownLoaderZakupki.Data.DB
{
    public class Contracts
    {
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// Номер контракта
        /// </summary>
        [Column(TypeName = "varchar(20)")]
        public string Contract_num { get; set; }
        ///// <summary>
        ///// Номер извещения об осуществлении закупки
        ///// </summary>
        //public string Purchase_num { get; set; }
        /// <summary>
        /// Тело извещения
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string R_body { get; set; }

        /// <summary>
        /// Тело извещения XML
        /// </summary>
        [Column(TypeName = "xml")]
        public string Xml_body { get; set; }
        /// <summary>
        /// Хэш XML данных
        /// </summary>
        [ Column(TypeName = "varchar(64)")]
        public string Hash { get; set; }
        /// <summary>
        /// Тип извещения проекта контрактов
        /// </summary>
        [Column(TypeName = "varchar(64)")]
        public string Type_contract { get; set; }
        /// <summary>
        /// Архив источник
        /// </summary>
        [Column(TypeName = "varchar(256)")]
        public string Zip_file { get; set; }
        /// <summary>
        /// Файл источник
        /// </summary>
        [Column(TypeName = "varchar(128)")]
        public string File_name { get; set; }
        /// <summary>
        /// Тип ФЗ
        /// </summary>
        [Column(TypeName = "smallint")]
        public int Fz_type { get; set; }
        /// <summary>
        /// Дата публикации + publishDate Дата и время размещения документа в ЕИС. 
        /// При передаче заполняется датой размещения документа в ЕИС
        /// Элемент игнорируется при приёме. 
        /// </summary>

        [Column(TypeName = "timestamp without time zone")]
        public DateTime PublishDate { get; set; }
    }
}
