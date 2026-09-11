using System.Net;
using System.Net.Sockets;
using TestZadanie;

Console.WriteLine("Тестер UDP сети");
Console.Write("Режим (сервер/клиент): ");
var mode = Console.ReadLine()?.ToLower();

using var cts = new CancellationTokenSource();


if (mode == "сервер")
{
    Console.Write("Порт: ");
    if (!int.TryParse(Console.ReadLine(), out int port)
        || port < 1
        || port > 65535)
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

    if (!IPAddress.TryParse(ip, out _))
    {
        Console.WriteLine("Некорректный IP адрес");
        return;
    }

    Console.Write("Порт сервера: ");
    if (!int.TryParse(Console.ReadLine(), out int port) || port < 1 || port > 65535)
    {
        Console.WriteLine("Некорректный порт");
        return;
    }


    Console.Write("Скорость пакетов в секунду: ");
    if (!int.TryParse(Console.ReadLine(), out int speed) || speed <= 0)
    {
        Console.WriteLine("Скорость должна быть больше 0");
        return;
    }


    Console.Write("Размер пакета: ");
    if (!int.TryParse(Console.ReadLine(), out int size) || size < 24)
    {
        Console.WriteLine("Размер пакета должен быть не меньше 24 байт");
        return;
    }

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
