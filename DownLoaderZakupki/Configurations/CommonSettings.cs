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

        public FtpCredential FtpCredential { get; set; }

    }

    public class PartUsed
    {
        public bool UseUpload { get; set; }
        public bool UseFz44Settings { get; set; }
        public bool UseFz223Settings { get; set; }
    }

    public class Credentional
    {
        public string Url { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }

    public class FtpCredential
    {
        public Credentional FZ44 { get; set; }
        public Credentional FZ223 { get; set; }
    }

}
