using System;
using System.Threading.Tasks;
using System.Diagnostics;
using Android.Print;
using Android.Webkit;
using Android.Content;
using Microsoft.Maui.ApplicationModel;
using Vakilaw.Services;

namespace Vakilaw.Platforms.Android
{
    public class PrinterService : IPrinterService
    {
        private const int FontSizePt = 14;

        public Task PrintTextAsync(string text, string jobName = "Vakilaw Contract")
        {
            var tcs = new TaskCompletionSource<bool>();

            try
            {
                Debug.WriteLine("PrinterService (Android) - Start print");

                var activity = Platform.CurrentActivity ?? throw new InvalidOperationException("Current Activity is null");

                var webView = new global::Android.Webkit.WebView(activity);

                var settings = webView.Settings;
                settings.JavaScriptEnabled = false;
                settings.DefaultTextEncodingName = "utf-8";
                settings.LoadWithOverviewMode = true;
                settings.UseWideViewPort = true;

                // تولید HTML راست‌چین با فونت فارسی
                string html = GeneratePrintableHtml(text);

                // WebViewClient سفارشی
                webView.SetWebViewClient(new PrintWebViewClient(activity, jobName, (success, ex) =>
                {
                    if (ex != null)
                    {
                        Debug.WriteLine($"Print error: {ex}");
                        tcs.TrySetException(ex);
                    }
                    else
                    {
                        tcs.TrySetResult(success);
                    }
                }));

                webView.LoadDataWithBaseURL("file:///android_asset/", html, "text/html", "utf-8", null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PrinterService (Android) exception: {ex}");
                tcs.TrySetException(ex);
            }

            return tcs.Task;
        }

        private string GeneratePrintableHtml(string content)
        {
            return $@"
<!doctype html>
<html>
<head>
<meta charset='utf-8'>
<meta name='viewport' content='width=device-width, initial-scale=1'>
<style>
  @font-face {{
    font-family: 'IRANSansWeb';
    src: url('file:///android_asset/Fonts/IRANSansWeb.ttf') format('truetype');
  }}

  html, body {{
    margin: 0;
    padding: 0;
    direction: rtl;
    text-align: right;
    font-family: 'IRANSansWeb', serif;
    font-size: 14pt;
    line-height: 1.45;
    -webkit-hyphens: auto;
    hyphens: auto;
    word-wrap: break-word;
    white-space: pre-wrap;
  }}

  h2 {{
    text-align: center;
    margin: 0;           /* حذف فاصله اضافی بالا و پایین */
    padding: 2px 0;      /* کمترین فاصله بین تیتر و متن */
    font-size: 16pt;
    line-height: 1.2;
  }}

  .page {{
    box-sizing: border-box;
    padding: 8px 16px 16px 16px; /* حاشیه بالای کمتر (8px) و بقیه 16px */
    width: 100%;
    page-break-after: auto;
  }}

  .contract-text {{
    margin-top: 4px;       /* فاصله خیلی کم بین تیتر و متن */
    text-align: right;
    line-height: 1.45;
    word-wrap: break-word;
    white-space: pre-wrap;
  }}

  p, h1, h2, h3, pre {{
    orphans: 3;
    widows: 3;
    -webkit-column-break-inside: avoid;
    page-break-inside: avoid;
    break-inside: avoid;
  }}
</style>
</head>
<body>
  <div class='page'>
    <h2>📝 قرارداد حقوقی</h2>
    <div class='contract-text'>
      {content.Replace("\n", "<br/>")}
    </div>
  </div>
</body>
</html>";
        }


        private class PrintWebViewClient : WebViewClient
        {
            private readonly global::Android.App.Activity _activity;
            private readonly string _jobName;
            private readonly Action<bool, Exception?> _onFinished;

            public PrintWebViewClient(global::Android.App.Activity activity, string jobName, Action<bool, Exception?> onFinished)
            {
                _activity = activity;
                _jobName = jobName;
                _onFinished = onFinished;
            }

            public override async void OnPageFinished(global::Android.Webkit.WebView view, string url)
            {
                base.OnPageFinished(view, url);

                try
                {
                    await Task.Delay(300); // صبر برای لود کامل CSS و فونت

                    var printManager = (PrintManager)_activity.GetSystemService(Context.PrintService);
                    var printAdapter = view.CreatePrintDocumentAdapter();
                    printManager.Print(_jobName, printAdapter, null);

                    Debug.WriteLine("Print job started.");
                    _onFinished?.Invoke(true, null);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Print failed: {ex}");
                    _onFinished?.Invoke(false, ex);
                }
            }
        }
    }
}