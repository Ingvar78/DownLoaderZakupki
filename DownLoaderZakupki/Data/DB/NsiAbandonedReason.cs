using DownLoaderZakupki.Models.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DownLoaderZakupki.Data.DB
{
    public class NsiAbandonedReason
    {
        [Key]
        public int Id { get; set; }

        
        [Column(TypeName = "bigint")]
        public double OosId { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string Code { get; set; }

        [Column(TypeName = "varchar(3000)")]
        public string Name { get; set; }

        [Column(TypeName = "varchar(350)")]
        public string objectName { get; set; }

        [Column(TypeName = "varchar(4)")]
        public string Type { get; set; }

        /// <summary>
        /// Храним в формате 
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string docType { get; set; }

        /// <summary>
        /// Храним в формате 
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string PlacingWay { get; set; }

        public bool Actual { get; set; }

        public FLType Fz_type { get; set; }
    }
}
