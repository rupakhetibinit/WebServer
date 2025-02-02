using WSServer;
var wsServer = new Server("127.0.0.1", 8080);

await wsServer.Start();
