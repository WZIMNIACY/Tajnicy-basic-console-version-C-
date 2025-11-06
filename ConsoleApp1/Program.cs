using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MyNamespace 
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            Game game = new Game(new Deck());
            game.Start();


            while (game.GameIsOn)
            {
                game.GameInfo();
                game.UserGiveAnswer();
                game.GameKeepGoing();

            }


            Console.Clear();


            game.Tabble();





        }
    }
}