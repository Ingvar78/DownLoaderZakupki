using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DownLoaderZakupki.Configurations
{
    public class BaseSettings
    {
        /// <summary>
        /// Путь к арбочей директории
        /// </summary>
        public string WorkPath { get; set; }
        /// <summary>
        /// Частота повторения
        /// </summary>
        public int RunEveryDay { get; set; }
        /// <summary>
        /// Количество параллельных потоков выполнения
        /// </summary>
        public int Parallels { get; set; }
        /// <summary>
        /// Размер очереди для обработки
        /// </summary>
        public int Queue { get; set; }
        /// <summary>
        /// Время запуска по расписанию
        /// </summary>
        public TimeSpan RunAtTime { get; set; }
        /// <summary>
        /// Размер пустых архивов для данного типа
        /// </summary>
        public long EmptyZipSize { get; set; }
        /// <summary>
        /// Директория на FTP - источник данных
        /// </summary>
        public string BaseDir { get; set; }
    }

    public class FZSettings44 : BaseSettings
    {
        /// <summary>
        /// Список регионов
        /// </summary>
        public string Regions { get; set; }
        public List<string> RegionsList => Regions == null ? new List<string>() : Regions.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
        /// <summary>
        /// Список директорий на FTP требующих обработки
        /// </summary>
        public string DirsDocs { get; set; }
        public List<string> DocDirList => DirsDocs == null ? new List<string>() : DirsDocs.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
        public int KeepDay { get; set; }

    }

    public class FZSettings223 : FZSettings44
    {

    }

    public class NsiSettings44 : BaseSettings
    {
        /// <summary>
        /// Список директорий на FTP требующих обработки
        /// </summary>
        public string DirsDocs { get; set; }
        public List<string> DocDirList => DirsDocs == null ? new List<string>() : DirsDocs.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    public class NsiSettings223 : BaseSettings
    {
        /// <summary>
        /// Список директорий на FTP требующих обработки
        /// </summary>
        public string DirsDocs { get; set; }
        public List<string> DocDirList => DirsDocs == null ? new List<string>() : DirsDocs.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
        /// <summary>
        /// Путь к csv фалам с данными площадок
        /// </summary>
        public string NsiVSRZ { get; set; }

    }
}
