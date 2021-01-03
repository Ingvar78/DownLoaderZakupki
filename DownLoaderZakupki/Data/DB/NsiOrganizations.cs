using DownLoaderZakupki.Models.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DownLoaderZakupki.Data.DB
{
    /// <summary>
    /// Организации зарегестрированные в ЕИС
    /// </summary>
    public class NsiOrganizations
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Код по СПЗ
        /// </summary>
        [Column(TypeName = "varchar(12)")]
        public string RegNumber { get; set; }

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

        /// <summary>
        /// Данные счетов 
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string Accounts { get; set; }
        /// <summary>
        /// Дата регистрации в ЕИС
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// ИНН заказчика
        /// </summary>
        [Column(TypeName = "varchar(10)")]
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
        /// Актуальность записи
        /// </summary>
        public bool IsActual { get; set; }

        /// <summary>
        /// Тип источника данных для разбора отличий по ФЗ-44 и ФЗ-223
        /// </summary>
        public FLType Fz_type { get; set; }

    }
}
