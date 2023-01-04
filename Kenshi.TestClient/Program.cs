using System;
using Microsoft.AspNetCore.SignalR.Client;

namespace Kenshi.TestClient
{
    class Program
    {
        private static Stack<string> commandsHistory = new Stack<string>();

        static async Task Main(string[] args)
        {
            // Create a connection to the SignalR hub
            var connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/gameHub")
                .Build();

            // Start the connection
            await connection.StartAsync();
            
            // Subscribe to messages from the hub
            connection.On<string>("ListGameRooms", (message) =>
            {
                Console.WriteLine(message);
            });

            while (true)
            {
                var input = Console.ReadLine();
                string[] parameters = input.Split(" ");

                try
                {
                    switch (parameters[0])
                    {
                        case "dc" or "disconnect":
                            return;
                        case "create_game":
                            await connection.SendAsync("CreateGameRoom", "test-room");
                            break;
                        case "delete_game":
                            await connection.SendAsync("DeleteGameRoom", int.Parse(parameters[1]));
                            Console.WriteLine($"delete game {int.Parse(parameters[1])}");
                            break;
                        case "join_game":
                            await connection.SendAsync("JoinGameRoom", parameters[1]);
                            break;
                        case "games":
                            await connection.SendAsync("ListGameRooms");
                            break;
                    }

                    if (!string.IsNullOrEmpty(input))
                    {
                        commandsHistory.Push(input);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error executing command." + e);
                }
                
                Thread.Sleep(50);
            }
        }
    }
}