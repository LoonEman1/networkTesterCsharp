using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TestZadanie
{

    public class Client
    {


        string host;
        int port;
        int speed;
        int size;

        long sent;
        long bytes;

        public Client(string host, int port, int speed, int size)
        {
            this.host = host;
            this.port = port;
            this.speed = speed;
            this.size = size;
        }


        public async Task StartAsync(CancellationToken token)
        {
            using var socket = new UdpClient();
            if (!IPAddress.TryParse(host, out var ip))
            {
                Console.WriteLine("Некорректный IP адрес");
                return;
            }
            var endpoint = new IPEndPoint(ip, port);
            _ = PrintAsync(token);

            ulong seq = 0;
            double delay = Math.Max(1, 1000.0 / speed);


            while (!token.IsCancellationRequested)
            {
                var p = new Packet
                {
                    Sequence = seq++,
                    Timestamp = Stopwatch.GetTimestamp(),
                    Payload = new byte[Math.Max(0, size - 24)]
                };

                var data = p.ToBytes();
                await socket.SendAsync(data, endpoint);
                Interlocked.Increment(ref sent);
                Interlocked.Add(ref bytes, data.Length);

                await Task.Delay(TimeSpan.FromMilliseconds(delay), token);

            }
        }


        async Task PrintAsync(CancellationToken token)
        {
            Directory.CreateDirectory("Logs");
            if (!File.Exists("Logs/client.csv"))
                File.WriteAllText("Logs/client.csv", "Time;Packets;Bytes;Speed(B/s)\n");

            while (!token.IsCancellationRequested)
            {
                await Task.Delay(1000);
                Console.WriteLine($"Отправлено пакетов: {sent}, байт: {bytes}");
                File.AppendAllText("Logs/client.csv",
                    $"{DateTime.Now},{sent},{bytes}\n");
            }
        }
    }
}
