using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace DownLoaderZakupki.Models.Enum
{
    public enum FLType
    {
        /// <summary>
        /// 44 ФЗ
        /// </summary>
        [Description("44-ФЗ")]
        Fl44 = 44,

        /// <summary>
        /// 223 ФЗ
        /// </summary>
        [Description("223-ФЗ")]
        Fl223 = 223,

        /// <summary>
        /// 615 ПП
        /// </summary>
        [Description("ПП РФ 615")]
        Fl615 = 615,

        /// <summary>
        /// 94 ФЗ
        /// </summary>
        [Description("94-ФЗ")]
        Fl94 = 94
    }

    public enum Status
    { 
        /// <summary>
        /// Файл в очереди на загрузку
        /// </summary>
        Exist = 1,
        /// <summary>
        /// Файл загружен
        /// </summary>
        Uploaded = 2,
        /// <summary>
        /// Файл обработан
        /// </summary>
        Processed = 3,
        /// <summary>
        /// Файл обработан и удалён
        /// </summary>
        Deleted = 4,
        /// <summary>
        /// Ошибка обработки файла
        /// </summary>
        Data_Error = 5

    }
}

