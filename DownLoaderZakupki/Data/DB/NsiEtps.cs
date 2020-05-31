using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DownLoaderZakupki.Data.DB
{
    /// <summary>
    /// Справочник: Электронные площадки
    /// </summary>
    public class NsiEtps
    {

        [Key]
        public int Id { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string Code { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string Name { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string Description { get; set; }
        [Column(TypeName = "varchar(30)")]
        public string Phone { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string Address { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string Email { get; set; }
        [Column(TypeName = "varchar(2000)")]
        public string FullName { get; set; }
        [Column(TypeName = "varchar(10)")]
        public string INN { get; set; }
        [Column(TypeName = "varchar(9)")]
        public string KPP { get; set; }
        public bool Actual { get; set; }

    }
}
