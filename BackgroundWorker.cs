namespace Ipc.Client;

public class BackgroundWorker : BackgroundService {
    private readonly NamedPipeConnection _connection;

    public BackgroundWorker(NamedPipeConnection connection) {
        _connection = connection;
    }

    protected async override Task ExecuteAsync(CancellationToken token) {
        await _connection.StartAsync(token);
    }
}

public class AnotherBackgroundWorker : BackgroundService {
    private readonly NamedPipeConnection _connection;

    public AnotherBackgroundWorker(NamedPipeConnection connection) {
        _connection = connection;
    }

    protected async override Task ExecuteAsync(CancellationToken token) {
        while (!token.IsCancellationRequested) {
            var key = Console.ReadKey(true);
            Console.WriteLine($"Key: {key.KeyChar}");

            switch (key.KeyChar) {
                case 'h':
                    await _connection.Write(new { kind = "get_projects" });
                    break;
                default:
                    break;
            }
        }
    }
}
