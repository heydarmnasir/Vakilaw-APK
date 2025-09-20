using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using Vakilaw.Services;

namespace Vakilaw.ViewModels;

public partial class DocumentsViewModel : ObservableObject
{
    [ObservableProperty] private bool isContractsVisible;
    [ObservableProperty] private bool isPleadingsVisible;
    [ObservableProperty] private bool isPetitionsVisible;

    [ObservableProperty] private string selectedTab; // "Contract", "Pleading", "Petition"


    [ObservableProperty] private string contractContent;

    string ConContent = "ماده 1- طرفین قرارداد : پیرو تنظیم وکالت نامه شماره ………. مورخ ……………….. قرارداد حق‌الوکاله به شرح زیر فی مابین خانم/آقای: ……………………….. فرزند ………………… شماره شناسنامه ………….. کدملی ………………… صادره از ……….. شماره تماس ………………. ساکن …………………………………………………………………….\r\nبه عنوان موکل و\r\nآقا/خانم ………………………. فرزند …………………. شماره شناسنامه …………… کدملی ………………… صادره از ……………… شماره تماس ………………… ساکن …………………………………………………………………….\r\nبه عنوان وکیل تنظیم گردیده که برای طرفین لازم‌الاجرا می‌باشد.\r\n \r\nماده 2- موضوع وکالت: موضوع وکالت عبارت است از …………………………………………\r\n \r\nماده 3- مبلغ قرارداد (حق الوکاله): حق الوکاله طبق توافق طرفین مبلغ …………………….. ریال معادل ………………. تومان تعیین گردید.\r\nتبصره: ما بین طرفین مقرر گردید که مبلغ …………………….. ریال معادل ………………… تومان نقداً و مابقی پس از اخذ حکم به نفع موکل دریافت گردد.\r\n \r\nماده 4- شرایط قرارداد: 1-تعهدات وکیل در اجرای این قرارداد، موکول به وصول مبلغ اولیه حق‌الوکاله خواهد بود.\r\n2-وکیل فوق‌الذکر موظف است در راستای احقاق حقوق موکل یا موکلین تمامی تلاش و کوشش خود را مبذول داشته و از هر طریقی که لازم بداند، اقدامات مقتضی را جهت مصلحت موکل به انجام رساند.\r\n3- موکل با اطلاع از اثر عدم پیشرفت کار و عواقب احتمالی آن قرارداد را امضاء نمود؛ بنابراین عدم پیشرفت کار برائت موکل را از پرداخت مبالغ مندرج در ماده 3 قرارداد حاصل نمی‌نماید.\r\n4- در صورت عزل وکیل یا ضم وکیل به وکیل، صلح و سازش طرفین دعوا با مداخله وکیل یا بدون مداخله وکیل، انصراف موکل از تعقیب موضوع در هر مرحله که باشد، استرداد دعوی توسط موکل یا طرف دعوی، وضع مقررات جدید یا به هر علتی که موضوع دعوا فیصله یابد، موکل متعهد است در تمام موارد فوق‌الذکر حق‌الوکاله مندرج در ماده 3 قرارداد را نقداً به صورت یکجا در مقابل اخذ رسید به وکیل پرداخت نماید.\r\n5- چنانچه موضوع وکالت ولو با ارسال اظهارنامه یا مذاکره شفاهی یا مصالحه به نتیحه برسد، موکل ملزم به پرداخت تمام حق الوکاله است.\r\n6-موکل در مورد حق‌الوکاله و کلیه اقدامات وکیل در جریان دادرسی هیچگونه اعتراض و ادعایی اعم از کیفری، حقوقی و انتظامی نداشته و به موجب این مقرره با رضایت کامل، تمامی اختلافات و دعاوی احتمالی آتی را با وکیل انتخابی خویش به صلح خاتمه می‌دهد.\r\n7- چنانچه پس از امضاء قرارداد و قبل از هرگونه اقدامی از جانب وکیل، موکل اقدام به عزل وکیل نماید، در این صورت موکل موظف است نیمی از مبلغ پیش پرداخت را به وکیل بپردازد.\r\n8- تعیین اوقات دادرسی با محکمه بوده و وکیل هیچگونه مسئولیتی در کندی و تسریع دادرسی ندارد.\r\n9- تعهد وکیل در مقابل موکل فقط دفاع از حقوق موکل در حد توانایی علمی و فنی خویش و با توجه به قوانین و مقررات مربوطه است و تصمیم گیرنده در خصوص دعوا قاضی است و وکیل هیچ تسلطی بر وی ندارد.\r\n10- این قرارداد شامل مرحله اجرای حکم نخواهد بود و چنانچه موکل تمایل به پیگیری مرحله اجرائی حکم توسط وکیل داشته باشد، می‌بایستی با توافق طرفین حق‌الوکاله آن مرحله نیز مشخص و نقداً به وکیل نیز پرداخت گردد؛ در غیر این صورت وکیل مزبور هیچ مسئولیتی در پیگیری مراحل اجرائی حکم نخواهد داشت.\r\n11- تهیه وسایل اجرای قرارهای دادگاه، معرفی شهود و مطلعین، احضار و جلب متهم به عهده وکیل نبوده و موکل موظف است نسبت به این امور اقدام کند.\r\n12- پرداخت هزینه‌های دادرسی و سایر هزینه‌ها از قبیل الصاق و ابطال تمبر، دستمزد کارشناس منتخب دادگاه، داور مورد رضایت طرفین، هزینه درج آگهی، هزینه مسافرت و غیره که برای رسیدگی و پیگیری پرونده مطروحه و احقاق حقوق موکل لازم باشد؛ به عهده موکل می‌باشد که در هر مرحله که لازم باشد، موکل موظف است نسبت به پرداخت نقدی هزینه موارد مذکور اقدام نماید؛ در غیر این صورت وکیل هیچ مسئولیتی به عهده نخواهد داشت.\r\n13- پرداخت مالیات و سهم کانون وکلاء به عهده وکیل خواهد بود.\r\n \r\nماده 5- مرجع حل اختلاف: در صورت بروز اختلاف ما بین طرفین به ترتیب از طریق مذاکره بین‌الطرفینی و سپس داوری و در نهایت مرجع صالح قانونی اقدام خواهد شد.\r\n \r\nماده 6- نُسخ قرارداد: این قرارداد مشتمل بر هفت ماده و یک تبصره در تاریخ ……………….. به تعداد …….. نسخه با اعتبار واحد تنظیم، امضاء و بین طرفین جهت اجراء مبادله گردید.\r\n\nامضاء وکیل\t\t\t\t\t\t\t\tامضاء موکل";

    private readonly IPrinterService _printerService;

    public DocumentsViewModel(IPrinterService printerService)
    {
        _printerService = printerService;
    }

    [RelayCommand]
    private async Task ShowContracts()
    {
        SelectedTab = "Contract";
        ResetPanels();
        IsContractsVisible = true;
        ContractContent = ConContent;
        await AnimatePanel("ContractsPanel");
    }

    [RelayCommand]
    private async Task ShowPleadings()
    {
        SelectedTab = "Pleading";
        ResetPanels();
        IsPleadingsVisible = true;
        await AnimatePanel("PleadingsPanel");
    }

    [RelayCommand]
    private async Task ShowPetitions()
    {
        SelectedTab = "Petition";
        ResetPanels();
        IsPetitionsVisible = true;
        await AnimatePanel("PetitionsPanel");
    }

    private void ResetPanels()
    {
        IsContractsVisible = false;
        IsPleadingsVisible = false;
        IsPetitionsVisible = false;
    }

    private async Task AnimatePanel(string panelName)
    {
        // اینجا بعداً در CodeBehind می‌تونیم انیمیشن FadeIn/Slide بذاریم
        await Task.Delay(50);
    }

    [RelayCommand]
    private async Task PrintContract()
    {
        if (string.IsNullOrWhiteSpace(ContractContent))
        {
            await App.Current.MainPage.DisplayAlert("خطا", "هیچ متنی برای چاپ وجود ندارد!", "باشه");
            return;
        }

        if (_printerService != null)
        {            
            await _printerService.PrintTextAsync(ContractContent, "قرارداد وکالت");
        }
        else
        {
            await App.Current.MainPage.DisplayAlert("خطا", "سرویس چاپ پیدا نشد!", "باشه");
        }
    }
}