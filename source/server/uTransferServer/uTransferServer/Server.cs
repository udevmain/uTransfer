using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace uTransferServer
{
    public class Server
    {
        private TcpListener _listener;
        private bool _isRunning = false;

        public Server(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public void Start()
        {
            _listener.Start();
            _isRunning = true;
            Logger.Log("Server started. Waiting connection...");

            ListenForClientsAsync();
        }

        private async Task ListenForClientsAsync()
        {
            while (_isRunning)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    Logger.Log($"New connection: {client.Client.RemoteEndPoint}");
                    _ = HandleClientAsync(client);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Connection error: {ex.Message}");
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            using (client)
            {
                var _stream = client.GetStream();
                var _reader = new StreamReader(_stream, Encoding.UTF8);
                var _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };

                while (client.Connected)
                {
                    string _command = null;

                    try
                    {
                        _command = await _reader.ReadLineAsync();
                    }
                    catch
                    {
                        Logger.Log("Command reading error. Client disconnected");
                    }

                    if (string.IsNullOrEmpty(_command))
                        break;

                    Logger.Log($"Getted command: {_command}");
                    await ProcessCommand(_command, _writer);
                }
            }

            Logger.Log("Client disconnected.");
        }

        private async Task ProcessCommand(string command, StreamWriter writer)
        {
            string[] parts = command.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;

            string cmd = parts[0].ToUpperInvariant();
            string argument = parts.Length > 1 ? parts[1] : string.Empty;

            switch (cmd)
            {
                case "LIST":
                    await HandleListCommand(argument, writer);
                    break;

                case "DOWNLOAD":
                    await HandleDownloadCommand(argument, writer);
                    break;

                case "UPLOAD":
                    await writer.WriteLineAsync("Upload not released");
                    break;

                default:
                    await writer.WriteLineAsync($"Unknown command: {cmd}");
                    break;
            }
        }

        private async Task HandleListCommand(string path, StreamWriter writer)
        {
            if (!Directory.Exists(path))
            {
                await writer.WriteLineAsync($"{path} does not exist");
                return;
            }

            string[] directories = Directory.GetDirectories(path);
            string[] files = Directory.GetFiles(path);

            await writer.WriteLineAsync($"Folders ({directories.Length}):");
            foreach (var dir in directories)
            {
                await writer.WriteLineAsync($"[DIR] {Path.GetFileName(dir)}");
            }

            await writer.WriteLineAsync($"Files ({files.Length}):");
            foreach (var file in files)
            {
                await writer.WriteLineAsync($"[FILE] {Path.GetFileName(file)}");
            }

            await writer.WriteLineAsync("END");
        }

        private async Task HandleDownloadCommand(string path, StreamWriter writer)
        {
            if (!File.Exists(path))
            {
                await writer.WriteLineAsync($"{path} does not exist");
                return;
            }

            var fileInfo = new FileInfo(path);
            await writer.WriteLineAsync($"SIZE {fileInfo.Length}");

            var clientReady = await writer.BaseStream.ReadAsync(new byte[1], 0, 0);

            using (var fileStream = File.OpenRead(path))
            {
                await fileStream.CopyToAsync(writer.BaseStream);
            }

            Logger.Log($"File sended: {path}");
        }
    }
}