using DownLoaderZakupki.Data.Access;
using System;
using System.Collections.Generic;
using System.Text;

namespace DownLoaderZakupki.Core.Interfaces
{
    internal interface IDbManager
    {
        void InitDb();
    }

    internal interface IGovDbManager:IDbManager
    {
        IGovDbContext GetContext();
    }
}
