using PuppeteerSharp;

namespace CSNewsWatcher
{
    internal class Program
    {

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            await new BrowserFetcher().DownloadAsync();
            Console.WriteLine("Trình duyệt đã sẵn sàng.");

            var watcher = new CSWatcherVer1();

            while (true)
            {
                await watcher.CheckAndNotify();
                Console.WriteLine("------------------------------------------");
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }

    }
}
