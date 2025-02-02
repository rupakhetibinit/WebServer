using WebServer;

var httpServer = new HttpServer("127.0.0.1", 8080);

await httpServer.Start();

