# Feedback

## Showtime API implementation details

I tried to keep the API as simple as possible. I didn't want to add too many features that could distract from the main goal of the test. But I did add some features that I think are important for a production ready API, also I tried to follow some best practices and design patterns, clean code and SOLID principles.

### Run the API

In order to test the API, you can just run:
```powershell
docker-compose build
```
```powershell
docker-compose up
```

Then run the cURLs in the cUrls.txt file. You can also use the Swagger UI at [https://localhost:7629/swagger/index.html](https://localhost:7629/swagger/index.html). 
You can also comment out the **showtime-api** service in the docker-compose file and run the **Showtime.API** project in Visual Studio.

There is also a **Postman** collection located here [https://api.postman.com/collections/2809009-aac36e07-e09b-4825-ac3e-c43224ba952d?access_key=PMAT-01GY5SKXESZ7MJN6TCEPC45ZYT](https://api.postman.com/collections/2809009-aac36e07-e09b-4825-ac3e-c43224ba952d?access_key=PMAT-01GY5SKXESZ7MJN6TCEPC45ZYT), it contains the same requests as the cURLs.
 
### Important notes
- The API was upgraded to use **.NET 7** and the latest versions of the packages.
- For the **GRPC client** implementation an authenticated channel is used, the api key is in the appsettings.json file. This is not the best approach, it should be stored in a secure place like Azure Key Vault.
- The API is using **MediatR** to implement commands following the CQRS pattern. For simplicity all the commands and queries are in the same controller.
- Validations are handled with **MediatR behaviors** and **FluentValidation**.
- There is a **global exception handler** to return a consistent error response. It's implemented with filters althought it can be implemented with a **middleware** as well.
- **Execution tracking** is handled with a **MediatR behavior** that logs the execution time of each request.
- For logging, **Serilog** is setup with the **Console** and **Seq** sinks. You can check the logs in the Seq UI at [http://localhost:5341/](http://localhost:5341/) or in the console if you prefer.
- The API is copying a certificate into the docker container to support https which is not a recommended approach.

Hope this helps.
Thanks :)