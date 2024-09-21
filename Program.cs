namespace Ipc.Client;

public static class Program {
    public async static Task Main(string[] args) {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddHostedService<BackgroundWorker>();
        builder.Services.AddHostedService<AnotherBackgroundWorker>();
        
        builder.Services.AddSingleton<NamedPipeConnection>();

        var host = builder.Build();
        await host.RunAsync();
    }
}

