# SendSMS
## Overview
This project is a microservice that listens for a message on a queue, consumes an external API and publishes an event to listening consumers.
The specific implementation is a microservice that sends SMS to users

###FLOW
=>It listens for SMS commands on a specific message queue named "SmsService". 
=> It consumes messages from this queue.
=> Then it sends the message as a request to a third-party SMS service.
=> When a request is sent successfully, It publishes SmsSentEvent to listening microservices.

Note: At every stage, the status of the sent messages is updated. So there is a persistence mechanism.

## What I would do if I had more time
I will create background jobs for 3 stages of ProcessClientMessageTask. So, if there is a failure it retries the process.

## Direction for running and testing the project
#### SmsService
1. Open your command terminal.
2. At the root of the "SmsService" folder, enter the following command: dotnet run
3. The project ran successfully if you see this message: "Now listening on: https://localhost:7157"
#### SmsService.Tests
1. Open your command terminal.
2. At the root of the "SmsService" folder, enter the following command: dotnet test
3. The test ran successfully if you see this message: "Passed!  - Failed:     0, Passed:     4, Skipped:     0, Total:     4"

## Assumptions
1. There is a cron job that will run periodically to pick up failed messages and resend
2. Assume there is a background service to run the ProcessClientMessageTask throughout the application life scope because it has to listen for SmsCommand 
3. We will implement a persistence mechanism. Will be implemented in IMessageRepo.
4. There will be a retry mechanism for connection in the EventBus implementation
