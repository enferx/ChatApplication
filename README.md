Application made with sockets that allows user to enter a chat room and chat. Additionaly, it allows to send command messages using rebus as service bus provider with sql server. 
The only thing you must ensure it's to have visual studio with localdb enabled at the following connection string:
Data Source = (LocalDb)\MSSQLLocalDB; Initial Catalog = RebusSource; Integrated Security = SSPI;". The tables will be created with the execution.

Steps
1. Set chat application as startup project. Login with two users and interchange messages. Let one or message to be the following sequence: /stock=aapl.us. This would fire service
bus events.
2. Stop the chat application and start the botparser. It will receive the messages and process the result.
