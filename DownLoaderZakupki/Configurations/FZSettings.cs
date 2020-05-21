using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DownLoaderZakupki.Configurations
{
    public class FZSettings
    {
        public string WorkPath { get; set; }
        public int RunEveryDay { get; set; }
        public int Parallels { get; set; }
        public int Queue { get; set; }
        public TimeSpan RunAtTime { get; set; }
        public long EmptyZipSize { get; set; }
        public string BaseDir { get; set; }
    }

    public class FZSettings44 : FZSettings
    {
        public string Regions { get; set; }
        public List<string> RegionsList => Regions == null ? new List<string>() : Regions.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
        public string DirsDocs { get; set; }
        public List<string> DocDirList => DirsDocs == null ? new List<string>() : DirsDocs.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
        public int KeepDay { get; set; }

    }

    public class FZSettings223 : FZSettings44
    {

    }
}
