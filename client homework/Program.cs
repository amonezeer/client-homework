using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class RpsUdpClient
{
    private static UdpClient udpClient;
    private static IPEndPoint serverEndPoint;
    private static int score1 = 0, score2 = 0;

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        udpClient = new UdpClient();
        serverEndPoint = new IPEndPoint(IPAddress.Loopback, 5000);

        Console.WriteLine("Підключення до сервера...");
        SendMessage("CONNECT_REQUEST");

        string serverResponse = ReceiveMessage();
        if (serverResponse == "ACCEPT")
        {
            Console.WriteLine("[СЕРВЕР] Підключення прийнято. Починаємо гру!");
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════╗");
            Console.WriteLine("║         ГРА КАМІНЬ-НОЖИЦІ-ПАПІР            ║");
            Console.WriteLine("╚════════════════════════════════════════════╝");
            Console.ResetColor();
            GameLoop();
        }
        else
        {
            Console.WriteLine("[СЕРВЕР] Підключення відхилено.");
            udpClient.Close();
        }
    }

    static void GameLoop()
    {
        for (int round = 1; round <= 5; round++)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"\n|---------------------------------------|");
            Console.WriteLine($"                Раунд {round}               ");
            Console.WriteLine($"|---------------------------------------|");
            Console.ResetColor();
            Console.WriteLine($"\nРахунок: Гравець 1 - {score1} | Гравець 2 - {score2}\n");

            string move1 = GetMove("Гравець 1");

            if (move1 == "DRAW")
            {
                if (score1 == 1 && score2 == 1 || score1 == 2 && score2 == 2)
                {
                    SendMessage("DRAW_REQUEST");
                    Console.WriteLine("\nГравець 1 запропонував нічию...");
                    string response = ReceiveMessage();

                    if (response == "DRAW_REQUEST")
                    {
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        Console.Write("\nСуперник запропонував нічию. Прийняти? (Y/N): ");
                        string decision = Console.ReadLine()?.Trim().ToUpper();
                        if (decision == "Y")
                        {
                            SendMessage("DRAW_ACCEPT");
                            Console.ResetColor();
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine($"\nГра завершена нічиєю. Фінальний рахунок: {score1}-{score2}");
                            Console.ResetColor();
                            udpClient.Close();
                            return;
                        }
                        else
                        {
                            SendMessage("DRAW_REJECT");
                            Console.WriteLine("Нічия відхилена. Гра продовжується.");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Нічия можлива тільки при рахунку 1-1 або 2-2.");
                }
                continue;
            }

            if (move1 == "LOSE")
            {
                SendMessage("LOSE");
                Console.WriteLine("Гравець 1 визнав поразку. Гра завершена!");
                udpClient.Close();
                return;
            }

            string move2 = GetMove("Гравець 2");
            if (move2 == "LOSE")
            {
                SendMessage("LOSE");
                Console.WriteLine("Гравець 2 визнав поразку. Гра завершена!");
                udpClient.Close();
                return;
            }

            SendMessage(move1 + "," + move2);

            string responseMessage = ReceiveMessage();
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine($"\n >>> {responseMessage} <<<\n");

            if (responseMessage.StartsWith("Рахунок"))
            {
                string[] parts = responseMessage.Split(':')[1].Trim().Split('-');
                score1 = int.Parse(parts[0]);
                score2 = int.Parse(parts[1]);
            }
            Console.ResetColor();
        }
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"Фінальний рахунок: Гравець 1 - {score1} | Гравець 2 - {score2}");
        Console.ResetColor();
        udpClient.Close();
    }

    static string GetMove(string player)
    {
        if (player == "Гравець 1")
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  Виберіть опцію:");
                Console.WriteLine("  1 - Камінь");
                Console.WriteLine("  2 - Ножиці");
                Console.WriteLine("  3 - Папір");
                Console.WriteLine("  4 - Запропонувати нічию");
                Console.WriteLine("  5 - Визнати поразку");
                Console.ResetColor();

                Console.Write($"{player}, зробіть вибір: ");
                string input = Console.ReadLine();

                switch (input)
                {
                    case "1": return "Камінь";
                    case "2": return "Ножиці";
                    case "3": return "Папір";
                    case "4": return "DRAW";
                    case "5": return "LOSE";
                    default: Console.WriteLine("Некоректний вибір, спробуйте ще раз."); break;
                }
            }
        }
        else if (player == "Гравець 2")
        {
            Console.Write($"{player}, зробіть вибір: ");
            string input = Console.ReadLine();

            switch (input)
            {
                case "1": return "Камінь";
                case "2": return "Ножиці";
                case "3": return "Папір";
                case "4": return "DRAW";
                case "5": return "LOSE";
                default:
                    Console.WriteLine("Некоректний вибір, спробуйте ще раз.");
                    return GetMove(player);
            }
        }

        return "";
    }


    static void SendMessage(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        udpClient.Send(data, data.Length, serverEndPoint);
    }

    static string ReceiveMessage()
    {
        var receivedData = udpClient.Receive(ref serverEndPoint);
        return Encoding.UTF8.GetString(receivedData);
    }
}
