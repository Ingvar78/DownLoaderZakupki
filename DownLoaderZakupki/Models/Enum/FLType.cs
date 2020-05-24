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
}

