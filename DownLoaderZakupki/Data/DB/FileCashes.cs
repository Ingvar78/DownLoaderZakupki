using DownLoaderZakupki.Models.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DownLoaderZakupki.Data.DB
{
    /// <summary>
    /// Кэш файлов для загрузки архивов по ФЗ44/223
    /// </summary>
    public class FileCashes
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
        public Status Status { get; set; }
        public FLType Fz_type { get; set; }
        public DateTime Modifid_date { get; set; }
    }

    /// <summary>
    /// Кэш для загрузки файлов справочников 
    /// </summary>
    public class NsiFileCashes
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
        public Status Status { get; set; }
        public FLType Fz_type { get; set; }
        public DateTime Modifid_date { get; set; }
    }
}
