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

        [Column(TypeName = "varchar(20)")]
        public string code { get; set; }

        [Column(TypeName = "varchar(3000)")]
        public string name { get; set; }

        [Column(TypeName = "varchar(350)")]
        public string objectName { get; set; }

        [Column(TypeName = "varchar(4)")]
        public string type { get; set; }

        /// <summary>
        /// Храним в формате 
        /// </summary>
        [Column(TypeName = "jsonb")]
        public docType docType { get; set; }

        /// <summary>
        /// Храним в формате 
        /// </summary>
        [Column(TypeName = "jsonb")]
        public placingWayType placingWay { get; set; }

        public bool actual { get; set; }

        public FLType fz_type { get; set; }
    }

    public class placingWayType
    {
        public string code { get; set; }

        public string name { get; set; }
    }

    public class docType
    {
        public string code { get; set; }
        public string name { get; set; }
    }
}
