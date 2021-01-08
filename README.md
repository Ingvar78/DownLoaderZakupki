# DownLoaderZakupki 

### Приложение предназначено для демонстрации работы с FluentFTP, FluentScheduler, Postgres, LINQ.

### В проекте реализовано:
- получение списка файлов и его загрузка с FTP с использованием FluentFTP и FluentScheduler.
- сохранение/получение списка файлов для последующей загрузки с FTP.
- приложение загружает данные по ФЗ-44 / ФЗ-223, и соответствующие справочники для последующей обработки
- обработка данных будет реализован на примере отдельных справочников (Сохранение справочников реализовано для 44ФЗ #4)


### Сборка и запуск: 
Для Windows:
dotnet publish -c release -o publish
Для Linux:
dotnet publish -c release -o publishLinux -r linux-x64
or 
dotnet publish --framework netcoreapp3.1 --runtime linux-x64 -o publishLinux 
or 
dotnet publish -c release -r linux-x64 --self-contained -o publishLinux 
or
dotnet publish .\DownLoaderZakupki.sln -c release -o publish -r linux-x64

Для запуска приложения на Linux: 
1) chmod +x DownLoaderZakupki
2) отредактировать appsettings.json и указать правильно секции: "BasePath" and "WorkPath" и подключение к серверу Postgres
- Пример конфигурации для Linux: appsettings.linux.json

Для запуска приложения на Windows: 
1) отредактировать appsettings.json и указать правильно секции: "BasePath" and "WorkPath" и подключение к серверу Postgres
2) Пример конфигурации для  Windows: appsettings.json 
3) Для запуска приложения на Windows: 
dotnet DownLoaderZakupki.dll или запуск исполняемого файла DownLoaderZakupki.exe


Файлы миграции отсутствуют в репозитории, для создания миграции отредактировать соответствующий кусок кода и выполнить: 
1) dotnet ef migrations add InitialCreate
2) dotnet ef database update -v

### Установка .Net Core на Linux 
- Ubuntu https://docs.microsoft.com/en-us/dotnet/core/install/linux-package-manager-ubuntu-1910
- Centos 7 https://docs.microsoft.com/en-us/dotnet/core/install/linux-package-manager-centos7

ecuador splashdown