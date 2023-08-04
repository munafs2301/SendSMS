# SendSMS
## Overview
This project is a microservice that listens for a message on a queue, consumes an external API and publishes an event to ;istening consumers.
The specific implementation gets a message from a client, it listens on a specific message queue named "SmsService". It consumes message from this queue

## User Credentials for testing
Email= "frontoffice@processrus.com"
Password = "FrontOffice@01"

Email= "backoffice@processrus.com"
Password = "BackOffice@01

Email= "admin@processrus.com"
Password = "Admin@01"

## What I would do if I had more time
I will create background jobs for 3 stages of ProcessClientMessageTask. So, if there is a failure it retries the process.

## Direction for testing
1. Run  Application (At the root of the project, type the command     **dotnet run**    and press enter)
2. Insert URLs into Postman request
3. Pass the bearer token to the Authorization tab when testing the Access Endpoint. (You can get the token from the response payload of the Login endpoint)

## Assumptions
1. Add Assumption to readme that a cron job will run periodically to pick up failed messages
2. Assume there is background service to run the ProcessClientMessageTask throughout the application lifescop because it has to listen for SmsCommand 
3. Assume We will implement a cache mechanism for persistence purpose. Will be implemented in IMessageRepo
4. Assume there will be a retry mechanism for connection in the EventBus implementation
