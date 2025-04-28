using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class ClientConsole
{
    static async Task Main(string[] args)
    {
        Console.Write("Server IP: ");
        string ip = Console.ReadLine();

        using (var _client = new TcpClient())
        {
            await _client.ConnectAsync(ip, 5000);
            var _stream = _client.GetStream();
            var _reader = new StreamReader(_stream, Encoding.UTF8);
            var _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };

            Console.WriteLine("Connected!");

            while (true)
            {
                string _input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(_input)) break;
                await _writer.WriteLineAsync(_input);
                while (true)
                {
                    string response = await _reader.ReadLineAsync();
                    if (response == null || response == "END") break;

                    Console.WriteLine(response);
                }
            }
        }
    }
}
