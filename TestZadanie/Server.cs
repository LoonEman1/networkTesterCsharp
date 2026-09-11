using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TestZadanie
{
    public class Server
    {
        private readonly int port;

        private long received;
        private long bytes;
        private long lost;

        private ulong lastSeq;
        bool hasSequence;

        private long lastBytes;

        private long lastReceiveTime;
        private long lastSendTime;

        private double jitterMs;


        public Server(int port)
        {
            this.port = port;
        }


        public async Task StartAsync(CancellationToken token)
        {
            using var udp = new UdpClient(port);
            _ = PrintAsync(token);

            while (!token.IsCancellationRequested)
            {
                var result = await udp.ReceiveAsync(token);
                var packet = Packet.FromBytes(result.Buffer);
                if (packet == null)
                    continue;

                if (hasSequence && packet.Sequence > lastSeq + 1)
                {
                    lost += (long)(packet.Sequence - lastSeq - 1);
                }
                lastSeq = packet.Sequence;
                hasSequence = true;

                long receiveTime = Stopwatch.GetTimestamp();

                if (received > 0)
                {
                    long receiveDiff = receiveTime - lastReceiveTime;
                    long sendDiff = packet.Timestamp - lastSendTime;

                    jitterMs = Math.Abs(receiveDiff - sendDiff)
                               * 1000.0 / Stopwatch.Frequency;
                }
                lastReceiveTime = receiveTime;
                lastSendTime = packet.Timestamp;
                Interlocked.Increment(ref received);
                Interlocked.Add(ref bytes, result.Buffer.Length);
            }
        }

        async Task PrintAsync(CancellationToken token)
        {
            Directory.CreateDirectory("Logs");

            if (!File.Exists("Logs/server.csv"))
                File.WriteAllText("Logs/server.csv",
                    "Time;Packets;Bytes;Speed(B/s);Lost;Jitter(ms)\n");

            while (!token.IsCancellationRequested)
            {
                await Task.Delay(1000);

                long speed = bytes - lastBytes;
                lastBytes = bytes;

                Console.WriteLine("----------------------------------");
                Console.WriteLine($"Получено пакетов: {received}");
                Console.WriteLine($"Получено данных: {bytes} байт");
                Console.WriteLine($"Скорость приема: {speed} байт/сек");
                Console.WriteLine($"Потеряно пакетов: {lost}");
                Console.WriteLine($"Jitter: {jitterMs:F3} ms");

                File.AppendAllText("Logs/server.csv",
                    $"{DateTime.Now};{received};{bytes};{speed};{lost};{jitterMs:F3}\n");
            }
        }
    }
}