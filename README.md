## Give a Star! :star:
If you liked the project or if 'Azure Service Bus' helped you, please give a star ;)

## Technologies implemented:

- ASP.NET Core 8.0 (with .NET Core 8.0)
- .NET Core Native DI
- Worker Service
- Azure Function
- Azure Service Bus

## Running the project
In the project root folder run the command:  
docker-compose up -d  
docker-compose up -d  --build  

## Azure Service Bus Emulator
https://learn.microsoft.com/en-us/azure/service-bus-messaging/test-locally-with-service-bus-emulator?tabs=docker-linux-container

## File 'local.settings.json'
Create the file 'local.settings.json' inside the folder '.\src\AzureFunction\' as below:  
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    // Local
    "ServiceBusConnection": "Endpoint=sb://localhost:5672;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;"
  }
}