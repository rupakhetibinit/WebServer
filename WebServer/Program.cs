using WebServer;
using JSONParser;
using TestFramework;

// var httpServer = new HttpServer("127.0.0.1", 8080);

// await httpServer.Start();

// const string something = "Whatever";

// using var lexer = new Lexer(something);


var list = new List<int> { 1, 2, 3 };
//var list = new List<int>() {1, 2, 3 };


static List<int> GetInts(List<int> values)
{
  var something = new List<int>(values)
  {
      2
  };
  something.RemoveAt(values.Count - 1);
  return something;
}

var second_list = GetInts(list);

Assert.That(list, second_list);

// while (true)
// {
//     var c = lexer.ReadChar();
//     if (c == '\0')
//     {
//         break;
//     }

//     Console.Write(c.ToString());
// }
