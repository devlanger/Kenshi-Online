using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Kenshi.Shared
{
    public class NetworkCommandProcessor
    {
        public static Dictionary<string, Action<string[]>> Commands = new Dictionary<string, Action<string[]>>
        {

        };

        public static void RegisterCommand(string command, Action<string[]> action)
        {
            if (Commands.ContainsKey(command))
            {
                return;
            }
            
            Commands.Add(command, action);
        }
        
        public static async Task<bool> ProccessCommand(string input, HubConnection connection)
        {
            string[] parameters = input.Split(' ');

            try
            {
                switch (parameters[0])
                {
                    case "dc":
                    case "disconnect":
                        return true;
                    case "create_game":
                        await connection.SendAsync("CreateGameRoom", input.Replace(parameters[0], ""));
                        break;
                    case "delete_game":
                        await connection.SendAsync("DeleteGameRoom", parameters[1]);
                        Console.WriteLine($"delete game {parameters[1]}");
                        break;
                    case "delete_all_games":
                        await connection.SendAsync("DeleteAllGameRooms");
                        Console.WriteLine($"delete all games");
                        break;
                    case "join_game":
                        await connection.SendAsync("JoinGameRoom", parameters[1]);
                        break;
                    case "leave_game":
                        await connection.SendAsync("LeaveGameRoom");
                        break;
                    case "games":
                        await connection.SendAsync("ListGameRooms");
                        break;
                    case "chat_msg":
                        await connection.SendAsync("SendChatMessageToAll", parameters[1]);
                        break;
                    case "connect":
                        //ConnectToGameServer(parameters[1], int.Parse(parameters[2]));
                        break;
                }

                if (Commands.ContainsKey(parameters[0]))
                {
                    Commands[parameters[0]].Invoke(parameters);
                }
                /*if (!string.IsNullOrEmpty(input))
                {
                    commandsHistory.Push(input);
                }
            */
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error executing command." + e);
            }

            return false;
        }
    }
}