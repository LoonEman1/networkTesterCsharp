using TestZadanie;
using System.Net.Sockets;

Console.WriteLine("Тестер UDP сети");
Console.Write("Режим (сервер/клиент): ");
var mode = Console.ReadLine()?.ToLower();

using var cts = new CancellationTokenSource();


if (mode == "сервер")
{
    Console.Write("Порт: ");
    if (!int.TryParse(Console.ReadLine(), out int port))
    {
        Console.WriteLine("Некорректный порт");
        return;
    }
    var server = new Server(port);
    _ = server.StartAsync(cts.Token);

}
else if (mode == "клиент")
{
    Console.Write("IP сервера: ");
    var ip = Console.ReadLine()!;
    Console.Write("Порт сервера: ");
    int port = int.Parse(Console.ReadLine()!);
    Console.Write("Пакетов в секунду: ");
    int speed = int.Parse(Console.ReadLine()!);
    Console.Write("Размер пакета: ");
    int size = int.Parse(Console.ReadLine()!);

    var client = new Client(ip, port, speed, size);
    _ = client.StartAsync(cts.Token);
}
else
{
    Console.WriteLine("Неизвестный режим");
    return;
}

Console.WriteLine("Нажмите q + Enter для остановки");
while (Console.ReadLine()?.ToLower() != "q") { }

cts.Cancel();
