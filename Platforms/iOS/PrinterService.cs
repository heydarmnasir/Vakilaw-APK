using System;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using Vakilaw.Services;
using System.Diagnostics;

namespace Vakilaw.Platforms.iOS
{
    public class PrinterService : IPrinterService
    {
        public Task PrintTextAsync(string text, string jobName = "Vakilaw Contract")
        {
            var tcs = new TaskCompletionSource<bool>();

            try
            {
                Debug.WriteLine("PrinterService (iOS) - Start print");

                // ساخت HTML مشابه (برای iOS فرض بر اینه که فونت در Info.plist ثبت شده)
                string html = GeneratePrintableHtml(text);

                var printController = UIPrintInteractionController.SharedPrintController;
                var printInfo = UIPrintInfo.PrintInfo;
                printInfo.OutputType = UIPrintInfoOutputType.General;
                printInfo.JobName = jobName;
                printController.PrintInfo = printInfo;

                var formatter = new UIMarkupTextPrintFormatter(html)
                {
                    StartPage = 0
                };
                printController.PrintFormatter = formatter;

                // نمایش دیالوگ چاپ
                printController.Present(true, (controller, completed, error) =>
                {
                    if (error != null)
                    {
                        Debug.WriteLine($"iOS print error: {error.LocalizedDescription}");
                        tcs.TrySetException(new Exception(error.LocalizedDescription));
                    }
                    else
                    {
                        tcs.TrySetResult(completed);
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PrinterService (iOS) exception: {ex}");
                tcs.TrySetException(ex);
            }

            return tcs.Task;
        }

        private string GeneratePrintableHtml(string content)
        {
            // فرض شده فونت در Info.plist به عنوان UIAppFonts اضافه شده و در دسترس است
            return $@"
<!doctype html>
<html>
<head>
<meta charset='utf-8'>
<style>
  @font-face {{
    font-family: 'IRANSansWeb';
    src: url('IRANSansWeb.ttf'); /* iOS فونت پس از bundle شدن قابل دسترسی است */
  }}
  body {{
    direction: rtl;
    text-align: right;
    font-family: 'IRANSansWeb', serif;
    font-size: 14pt;
    line-height: 1.6;
    white-space: pre-wrap;
    word-wrap: break-word;
  }}
  p {{ orphans:3; widows:3; page-break-inside:avoid; }}
</style>
</head>
<body>
  <h2 style='text-align:center;'>📝 قرارداد حقوقی</h2>
  <div>{System.Net.WebUtility.HtmlEncode(content).Replace("\n", "<br/>")}</div>
</body>
</html>";
        }
    }
}