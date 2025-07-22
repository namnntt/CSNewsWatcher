using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CSNewsWatcher { 
public class CSWatcherVer1
    {
    private HashSet<string> _knownHrefs = new HashSet<string>();
    private bool _isFirstRun = true;

    private const string TargetUrl = "https://www.counter-strike.net/news";
    private const string PostSelector = "a.blogcapsule_BlogCapsule_3OBoG";
    private const string TitleSelector = "div.blogcapsule_Title_39UGs";

    public async Task CheckAndNotify()
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Đang khởi chạy trình duyệt để kiểm tra...");

        var launchOptions = new LaunchOptions { Headless = true, Args = new[] { "--disable-gpu", "--no-sandbox" } };
        using var browser = await Puppeteer.LaunchAsync(launchOptions);
        using var page = await browser.NewPageAsync();

        try
        {
            await page.GoToAsync(TargetUrl, WaitUntilNavigation.Networkidle0);
            await page.WaitForSelectorAsync(PostSelector, new WaitForSelectorOptions { Timeout = 30000 });
            var postElements = await page.QuerySelectorAllAsync(PostSelector);
            var currentHrefs = (await Task.WhenAll(postElements.Select(async el => await (await el.GetPropertyAsync("href")).JsonValueAsync<string>()))).ToList();

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
                            title = (await titleElement.EvaluateFunctionAsync<string>("el => el.textContent")).Trim();
                        }
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n========== BÀI VIẾT MỚI ==========");
                    Console.WriteLine($"Tiêu đề: {title}");
                    Console.WriteLine($"Link: {href}");
                    Console.ResetColor();

                    // GỌI HÀM GỬI EMAIL
                    await SendEmailNotification(title, href);

                    _knownHrefs.Add(href);
                }
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n!!! ĐÃ XẢY RA LỖI !!! - Lý do: {ex.Message}");
            Console.ResetColor();
        }
        finally
        {
            await browser.CloseAsync();
        }
    }

    private async Task SendEmailNotification(string title, string link)
    {
        // --- CẤU HÌNH THÔNG TIN EMAIL CỦA BẠN ---
        string fromMail = "thatlasailamquade@gmail.com";        // ĐỊA CHỈ GMAIL CỦA BẠN
        string fromPassword = "mlzi kelg hjwy gzew";    // MẬT KHẨU ỨNG DỤNG 16 KÝ TỰ
        string toMail = "namnntt@gmail.com";

        // Tạo nội dung email
        var mailMessage = new MailMessage
        {
            From = new MailAddress(fromMail, "CS News Bot"),
            Subject = $"CS:GO có bài viết mới: {title}",
            Body = $"<h1>{title}</h1>" +
                   $"<p>Một bài viết mới vừa được đăng trên trang chủ Counter-Strike.</p>" +
                   $"<p>Nhấn vào đây để đọc: <a href='{link}'>{link}</a></p>",
            IsBodyHtml = true,
        };
        mailMessage.To.Add(toMail);

        // Cấu hình máy chủ SMTP của Google
        var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(fromMail, fromPassword),
            EnableSsl = true,
        };

        try
        {
            Console.WriteLine($"Đang gửi email thông báo đến {toMail}...");
            await smtpClient.SendMailAsync(mailMessage);
            Console.WriteLine("Gửi email thành công!");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Lỗi khi gửi mail: {ex.Message}");
            Console.ResetColor();
        }
    }
}
}
