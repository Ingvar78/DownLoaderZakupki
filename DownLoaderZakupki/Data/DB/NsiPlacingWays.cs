using DownLoaderZakupki.Models.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DownLoaderZakupki.Data.DB
{
    /// <summary>
    /// Способы размещения заказов
    /// </summary>
    public class NsiPlacingWays
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "bigint")]
        public long PlacingWayId { get; set; }

        /// <summary>
        /// Данные выгрузки
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string PlacingWayData { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string Code { get; set; }


        [Column(TypeName = "varchar(3000)")]
        public string Name { get; set; }

        //[Column(TypeName = "varchar(20)")]
        public int SSType { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string Type { get; set; }
        /// <summary>
        /// Тип ФЗ
        /// </summary>
        public FLType Fz_type { get; set; }

        public bool Actual { get; set; }
        public bool IsProcedure { get; set; }
        public bool IsExclude { get; set; }
        public bool IsClosing { get; set; }
    }
}    