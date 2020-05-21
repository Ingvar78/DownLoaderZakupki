# DownLoaderZakupki

Загрузка данных с ftp госзакупок (Пример работы с  FluentFTP, Fluent)

В проекте реализовано получение списка файлов и его загрузка с FTP с использованием
FluentFTP и FluentScheduler

# Для создания локальной БД воспользоваться : 
1) dotnet ef migrations add InitialCreate
2) dotnet ef database update -v

# Запуск на Linux 
Установка .Net Core: 
https://docs.microsoft.com/en-us/dotnet/core/install/linux-package-manager-ubuntu-1910
https://docs.microsoft.com/en-us/dotnet/core/install/linux-package-manager-centos7
appsettings.linux.json - конфиг для Linux

