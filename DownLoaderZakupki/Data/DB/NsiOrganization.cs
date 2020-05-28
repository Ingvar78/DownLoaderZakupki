using DownLoaderZakupki.Models.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DownLoaderZakupki.Data.DB
{
    public class NsiOrganization
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Код по СПЗ
        /// </summary>
        [Column(TypeName = "varchar(12)")]
        public string RegNumber { get; set; }

        /// <summary>
        /// Краткое наименование 
        /// </summary>
        [Column(TypeName = "varchar(1024)")]
        public string ShortName { get; set; }
        /// <summary>
        /// Полное наименование 
        /// </summary>
        [Column(TypeName = "varchar(2000)")]
        public string FullName { get; set; }

        /// <summary>
        /// Данные из выгрузки XML
        /// </summary>
        [Column(TypeName = "jsonb")]

        public string NsiData { get; set; }

        [Column(TypeName = "jsonb")]
        public string Accounts { get; set; }
        /// <summary>
        /// Дата регистрации в ЕИС
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// ИНН заказчика
        /// </summary>
        [Column(TypeName = "varchar(12)")]
        public string Inn { get; set; }

        /// <summary>
        /// КПП заказчика
        /// </summary>
        [Column(TypeName = "varchar(20)")]
        public string Kpp { get; set; }

        /// <summary>
        /// ОГРН заказчика
        /// </summary>
        [Column(TypeName = "varchar(20)")]
        public string Ogrn { get; set; }

        /// <summary>
        /// ОКОПФ заказчика
        /// </summary>
        [Column(TypeName = "varchar(20)")]
        public string Okopf { get; set; }

        /// <summary>
        /// ОКПО заказчика
        /// </summary>
        [Column(TypeName = "varchar(20)")]
        public string Okpo { get; set; }

        /// <summary>
        /// ОКВЕД заказчика
        /// </summary>
        [Column(TypeName = "varchar(20)")]
        public string Okved { get; set; }

        /// <summary>
        /// ОКТМО заказчика
        /// </summary>
        [Column(TypeName = "varchar(20)")]
        public string Oktmo { get; set; }
        /// <summary>
        /// Актуальность записи
        /// </summary>
        public bool IsActual { get; set; }

        [Column(TypeName = "varchar(256)")]
        public string Email { get; set; }

        [Column(TypeName = "varchar(30)")]
        public string Phone { get; set; }
        [Column(TypeName = "varchar(30)")]
        public string Fax { get; set; }
        [Column(TypeName = "varchar(1024)")]
        public string Url { get; set; }

        public int TimeZone { get; set; }
        /// <summary>
        /// Контактное лицо
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string ContactPerson { get; set; }
        /// <summary>
        /// Фактический адрес
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string FactualAddress { get; set; }

        /// <summary>
        /// Почтовый адрес
        /// </summary>
        [Column(TypeName = "varchar(1024)")]
        public string PostalAddress { get; set; }

        public FLType Fz_type { get; set; }

    }
}
