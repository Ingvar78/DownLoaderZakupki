using System;
using System.Collections.Generic;
using System.Text;

namespace DownLoaderZakupki.Configurations
{
    public class CommonSettings
    {
        /// <summary>
        /// Основная рабочая директория
        /// </summary>
        public string BasePath { get; set; }
        /// <summary>
        /// Директория для ошибок и необработанных файлов.
        /// </summary>
        public string DebugPath { get; set; }

        /// <summary>
        /// Срок зранения архивов в днях для экономии места
        /// </summary>
        public int KeepDay { get; set; }
        /// <summary>
        /// Дата начала загрузки (2017-01-01)
        /// </summary>
        public string StartDate { get; set; }
        /// <summary>
        /// Управление задачами загрузки
        /// </summary>
        public PartUsed partUsed { get; set; }

        public FtpCredential FtpCredential { get; set; }

    }

    public class PartUsed
    {
        /// <summary>
        /// Использовать загрузку 
        /// </summary>
        public bool UseUpload { get; set; }
        /// <summary>
        /// Загружать данные ФЗ 44 (Да, Нет)
        /// </summary>
        public bool UseFz44Settings { get; set; }

        /// <summary>
        /// Загружать данные ФЗ 223 (Да, Нет)
        /// </summary>
        public bool UseFz223Settings { get; set; }

        public bool UseNsiSettings44 { get; set; }
        public bool UseNsiSettings223 { get; set; }

    }

    public class Credentional
    {
        public string Url { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }

    public class FtpCredential
    {
        /// <summary>
        /// Настройки авторизации на FTP для 44ФЗ
        /// </summary>
        public Credentional FZ44 { get; set; }
        /// <summary>
        /// Настройки авторизации на FTP для 223ФЗ
        /// </summary>
        public Credentional FZ223 { get; set; }
    }

}
