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
        public string code;
        
        [Column(TypeName = "varchar(3000)")]
        public string name;

        public string objectName;

        public string type;

        /// <summary>
        /// Храним в формате 
        /// </summary>
        [Column(TypeName = "jsonb")]
        public docType docType;

        /// <summary>
        /// Храним в формате 
        /// </summary>
        [Column(TypeName = "jsonb")]
        public placingWayType placingWay;

        public bool actual;

        public FLType fz_type;
    }

    public class placingWayType
    {
        public string code;
        
        public string name;
    }

    public class docType
    {
        public string code;
        public string name;
    }
}
