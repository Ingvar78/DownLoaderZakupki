using System;
using System.Collections.Generic;
using System.Text;

namespace DownLoaderZakupki.Configurations
{
    public class CommonSettings
    {
        public string BasePath { get; set; }
        public int KeepDay { get; set; }
        /// <summary>
        /// Дата начала загрузки (2017-01-01)
        /// </summary>
        public string StartDate { get; set; }

        public PartUsed partUsed { get; set; }

    }

    public class PartUsed
    {
        public bool UseUpload { get; set; }
        public bool UseFz44Settings { get; set; }
        public bool UseFz223Settings { get; set; }
    }
    
}
