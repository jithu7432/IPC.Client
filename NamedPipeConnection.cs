using System.Buffers.Binary;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;

namespace Test.Client;

public class NamedPipeConnection {
    private readonly ILogger<NamedPipeConnection> _logger;


    public NamedPipeConnection(ILogger<NamedPipeConnection> logger) {
        _logger = logger;
    }

    private NamedPipeClientStream? _pipe;
    private readonly Encoding _encoding = Encoding.UTF8;


    public async Task StartAsync(CancellationToken token) {
        _logger.LogDebug("Attempting named pipe connection");
        _pipe = new NamedPipeClientStream(".", "abc", PipeDirection.InOut, PipeOptions.Asynchronous);
        await _pipe.ConnectAsync(token);

        const int CHUNK = 512;

        while (_pipe.IsConnected && _pipe.CanRead) {
            var size = new byte[4];
            var read = await _pipe.ReadAsync(size, 0, 4, token);
            var a = BinaryPrimitives.ReadInt32BigEndian(size);
            Console.WriteLine($"Reads: `{a}`");

            var nSize = (int)Math.Ceiling((double)a / CHUNK);
            Console.WriteLine($"gonna read {nSize} time(s)");

            var s = new StringBuilder();

            for (var i = 0; i < nSize; ++i) {
                var buffer = new byte[CHUNK];
                var bytesRead = await _pipe.ReadAsync(buffer, 0, CHUNK, token);
                // Console.WriteLine($"Read {bytesRead} byte(s)");

                if (bytesRead <= 0) {
                    _logger.LogDebug("Cannot read buffer, reached EOS");
                    break;
                }

                s.Append(_encoding.GetString(buffer));
            }

            _logger.LogDebug("Buffer reads: `{Data}`", s);
        }
    }

    public async Task Write(object data) {
        if (_pipe is null) {
            _logger.LogError("Pipe is broken");
            return;
        }

        if (!_pipe.IsConnected) {
            _logger.LogError("Pipe connection unavailable");
            return;
        }

        if (!_pipe.CanWrite) {
            _logger.LogError("Pipe is not available for writing");
            return;
        }

        await _pipe.WriteAsync(_encoding.GetBytes(JsonSerializer.Serialize(data)));
    }

    public async Task StopAsync() {
        if (_pipe is not null) {
            if (_pipe.IsConnected) {
                _pipe.Close();
            }

            await _pipe.DisposeAsync();
        }
    }
}
