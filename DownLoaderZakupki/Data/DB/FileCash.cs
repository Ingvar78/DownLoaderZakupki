using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DownLoaderZakupki.Data.DB
{
    public class FileCash
    {
        [Key]
        public int Id { get; set; }
        [Column(TypeName = "varchar(128)")]
        public string Zip_file { get; set; }
        [Column(TypeName = "varchar(256)")]
        public string Full_path { get; set; }

        [Column(TypeName = "varchar(64)")]
        public string BaseDir { get; set; }

        [Column(TypeName = "varchar(64)")]
        public string Dirtype { get; set; }
        /// <summary>
        /// Дата последнего изменения файла
        /// </summary>
        public DateTime Date { get; set; }

        [Column(TypeName = "bigint")]
        public long Size { get; set; }
        public int Status { get; set; }
        public int Fz_type { get; set; }
        public DateTime Modifid_date { get; set; }
    }
}
