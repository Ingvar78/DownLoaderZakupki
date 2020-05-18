using DownLoaderZakupki.Core.Interfaces;
using DownLoaderZakupki.Data.Managers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DownLoaderZakupki.Core
{
    public class InjectorBootStrapper
    {
            public static void RegisterServices(IServiceCollection services)
            {
                //Доступ к БД приложения
                services.AddTransient<IGovDbManager, GovDbManager>();
                //
                services.AddTransient<JobsRegister>();

            }
        

    }
}
