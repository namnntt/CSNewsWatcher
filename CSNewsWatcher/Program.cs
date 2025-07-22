using PuppeteerSharp;

namespace CSNewsWatcher
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // **THÊM LẠI DÒNG QUAN TRỌNG NÀY**
            // Tải trình duyệt MỘT LẦN DUY NHẤT khi chương trình khởi động
            Console.WriteLine("Đang kiểm tra và tải phiên bản trình duyệt tương thích...");
            await new BrowserFetcher().DownloadAsync();
            Console.WriteLine("Trình duyệt đã sẵn sàng.");
            Console.WriteLine("------------------------------------------");

            var watcher = new CSNewsWatcherVer1();

            while (true)
            {
                await watcher.CheckForNewPost();
                Console.WriteLine("------------------------------------------");
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }
    }
}
