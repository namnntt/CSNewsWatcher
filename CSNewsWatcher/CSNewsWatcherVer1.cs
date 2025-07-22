
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSNewsWatcher
{
    internal class CSNewsWatcherVer1
    {
        private HashSet<string> _knownHrefs = new HashSet<string>();
        private bool _isFirstRun = true;

        private const string TargetUrl = "https://www.counter-strike.net/news";
        private const string PostSelector = "a.blogcapsule_BlogCapsule_3OBoG";
        private const string TitleSelector = "div.blogcapsule_Title_39UGs";

        public async Task CheckForNewPost()
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Đang khởi chạy trình duyệt để kiểm tra...");

            var launchOptions = new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--disable-gpu", "--no-sandbox", "--disable-setuid-sandbox", "--disable-dev-shm-usage" }
            };

            using var browser = await Puppeteer.LaunchAsync(launchOptions);
            using var page = await browser.NewPageAsync();

            try
            {
                await page.GoToAsync(TargetUrl, WaitUntilNavigation.Networkidle0);

                // Dòng này sẽ gây ra TimeoutException sau 30s nếu bị chuyển hướng
                await page.WaitForSelectorAsync(PostSelector, new WaitForSelectorOptions { Timeout = 30000 });

                var postElements = await page.QuerySelectorAllAsync(PostSelector);

                var currentHrefs = new List<string>();
                foreach (var element in postElements)
                {
                    var hrefProperty = await element.GetPropertyAsync("href");
                    currentHrefs.Add(await hrefProperty.JsonValueAsync<string>());
                }

                if (_isFirstRun)
                {
                    _knownHrefs = new HashSet<string>(currentHrefs);
                    Console.WriteLine($"Khởi tạo thành công. Đã ghi nhận {_knownHrefs.Count} bài viết.");
                    _isFirstRun = false;
                    return;
                }

                foreach (var href in currentHrefs)
                {
                    if (!_knownHrefs.Contains(href))
                    {
                        var newPostElement = (await page.QuerySelectorAllAsync($"{PostSelector}[href='{href}']")).FirstOrDefault();
                        string title = "Không có tiêu đề";
                        if (newPostElement != null)
                        {
                            var titleElement = await newPostElement.QuerySelectorAsync(TitleSelector);
                            if (titleElement != null)
                            {
                                title = await titleElement.EvaluateFunctionAsync<string>("el => el.textContent");
                            }
                        }

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\n========== BÀI VIẾT MỚI ==========");
                        Console.WriteLine($"Tiêu đề: {title.Trim()}");
                        Console.WriteLine($"Link: {href}");
                        Console.WriteLine($"==================================\n");
                        Console.ResetColor();

                        _knownHrefs.Add(href);
                    }
                }
            }
            catch (Exception ex)
            {
                // Bất kể là lỗi gì (mất mạng, timeout, bị chặn...), nó đều sẽ được bắt ở đây
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n!!! ĐÃ XẢY RA LỖI !!!");
                Console.WriteLine($"Lý do: {ex.Message}");
                Console.ResetColor();
            }
            finally
            {
                await browser.CloseAsync();
            }
        }

    }
}
