namespace Smdb.Api;

public class Program
{
    public static async Task Main()
    {
        App app = new App();
        await app.Start();
    }
}
