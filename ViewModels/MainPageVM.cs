using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mopups.Services;
using System.Collections.ObjectModel;
using Vakilaw.Models;
using Vakilaw.Services;
using Vakilaw.Views;
using static Vakilaw.Views.LawyerSubmitPopup;
using static Vakilaw.ViewModels.LawBankVM;

namespace Vakilaw.ViewModels;

public partial class MainPageVM : ObservableObject
{
    private readonly DatabaseService _database;

    [ObservableProperty]
    private ObservableCollection<LawItem> bookmarkedLaws;

    private readonly UserService _userService;

    [ObservableProperty] private bool isLawyer;
    [ObservableProperty] private bool canRegisterLawyer;

    [ObservableProperty] private bool showRegisterLabel;
    [ObservableProperty] private bool lawyerLicensevisibility = false;
    [ObservableProperty] private string lawyerFullName;
    [ObservableProperty] private string lawyerLicense;

    [ObservableProperty] private ObservableCollection<string> cities;
    [ObservableProperty] private string selectedCity;

    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private int pageSize = 7;

    [ObservableProperty]
    private bool hasMorePages = true;

    public MainPageVM(UserService userService, DatabaseService database)
    {
        _database = database;
        LoadBookmarks();

        // گوش دادن به تغییرات
        WeakReferenceMessenger.Default.Register<BookmarkChangedMessage>(this, (r, m) =>
        {
            if (m.Law.IsBookmarked)
            {
                if (!BookmarkedLaws.Any(x => x.Id == m.Law.Id))
                    BookmarkedLaws.Add(m.Law);
            }
            else
            {
                var item = BookmarkedLaws.FirstOrDefault(x => x.Id == m.Law.Id);
                if (item != null)
                    BookmarkedLaws.Remove(item);
            }
        });


        _userService = userService;

        // شهرها رو از کل لیست وکلا استخراج کن (AllLawyers)
        Cities = new ObservableCollection<string>(
            AllLawyers
                .Where(l => !string.IsNullOrWhiteSpace(l.City))
                .Select(l => l.City)
                .Distinct()
                .OrderBy(c => c)
                .ToList()
        );

        LoadUserState();
        LoadCities();

        // گوش دادن به پیام ثبت وکیل
        WeakReferenceMessenger.Default.Register<LawyerRegisteredMessage>(this, async (r, m) =>
        {
            IsLawyer = true;
            CanRegisterLawyer = false;
            ShowRegisterLabel = false;
            LawyerLicensevisibility = true;
            LawyerFullName = m.Value;
            LawyerLicense = m.LicenseNumber;

            await LoadNotes(); // همین متد صفحه‌بندی
        });

        Task.Run(async () => await LoadNotes());
    }

    private async void LoadBookmarks()
    {
        var items = await _database.GetBookmarkedLawsAsync();
        BookmarkedLaws = new ObservableCollection<LawItem>(items);
    }

    [RelayCommand]
    private async Task LawBankPage()
    {
        await Shell.Current.GoToAsync("LawBankPage");
    }

    [ObservableProperty]
    private ObservableCollection<Lawyer> allLawyers = new ObservableCollection<Lawyer>
    {
         #region خرمشهر
            new Lawyer { FullName="حامد باوی", PhoneNumber="09163335054", City="خرمشهر", Address="خرمشهر، خیابان خیام، مجتمع وکلا" },
            new Lawyer { FullName="حسین محمدی نصیر", PhoneNumber="09169002699", City="خرمشهر", Address="خرمشهر، خیابان خیام، مجتمع وکلا طبقه 1" },
            new Lawyer { FullName="اسحاق عدلی", PhoneNumber="09166325140", City="خرمشهر", Address="خرمشهر، خیابان خیام، مجتمع وکلا طبقه 1" },
            new Lawyer { FullName="حامد قیم", PhoneNumber="09379841497", City="خرمشهر", Address="خرمشهر، خیابان خیام" },
            new Lawyer { FullName="علی بچاری", PhoneNumber="09166302358", City="خرمشهر", Address="خرمشهر چهار راه مطهری جنب بانک رفاه ط اول دفتر وکالت علی بچاری" },
            new Lawyer { FullName="غلامعباس بخنوه", PhoneNumber="09161324492", City="خرمشهر", Address="خرمشهر خ 40 متری نبش خیام روبروی کلانتری11طبقه دوم" },
            new Lawyer { FullName="میلاد برومی", PhoneNumber="09166328688", City="خرمشهر", Address="خرمشهر فلکه الله دفتر آقای حمید رضا فتلی ساکی" },
            new Lawyer { FullName="رسول بیت سیاح", PhoneNumber="09166041872", City="خرمشهر", Address="خرمشهر خ سیناساختمان 4طبقه دوم" },
            new Lawyer { FullName="زهرا جلیلیان", PhoneNumber="09163335910", City="خرمشهر", Address="خرمشهر چهارراه مطهری ابتدای خیابان چهل متری طبقه سوم" },
            new Lawyer { FullName="سعدی حبیبی", PhoneNumber="0916324475", City="خرمشهر", Address="خرمشهر خ خیام جنب پرده سرای پردیس ط فوقانی" },
            new Lawyer { FullName="سید علی حسینی", PhoneNumber="09161324261", City="خرمشهر", Address="خرمشهر خیابان چهل متری روبروی خیابان سایت اداری ابتدای خیابان نسیم کوچه اول" },
            new Lawyer { FullName="زینب خلیقی", PhoneNumber="09167669517", City="خرمشهر", Address="خرمشهر- چهاراه مطهری-پاساز آیینه- ط سوم- دفتر خانم زهرا جلیلیان" },
            new Lawyer { FullName="مهسا رستگاریان", PhoneNumber="09171039367", City="خرمشهر", Address="خرمشهر خ چهل متری کوچه ریحان" },
            new Lawyer { FullName="امیر زهیری", PhoneNumber="09166336495", City="خرمشهر", Address="خرمشهر چهارراه مطهری" },
            new Lawyer { FullName="عبدالسلام ساکی", PhoneNumber="09166117610", City="خرمشهر", Address="خرمشهر جاده ساحلی جنب درمانگاه بهبهانیها" },
            new Lawyer { FullName="سکینه سلیمانی", PhoneNumber="09166310781", City="خرمشهر", Address="خرمشهر مجتمع تجاری اداری امام رضا ط دوم واحد 205" },
            new Lawyer { FullName="سعدیه شریفی", PhoneNumber="09375669589", City="خرمشهر", Address="خرمشهر خ 40 متری فرعی خیام ساختمان چند طبقه وکلا طبقه سوم دفتر وکالت زهرا شکریان" },
            new Lawyer { FullName="خدیجه شمخانی", PhoneNumber="09306758601", City="خرمشهر", Address="خرمشهر فلکه اله خیابان خیام مجتمع کارون دفتر وکالت خانم زهرا شکریان" },
            new Lawyer { FullName="زهرا شکریان", PhoneNumber="09166095582", City="خرمشهر", Address="خرمشهر فلکه اله خیابان خیام مجتمع کارون دفتر وکالت خانم زهرا شکریان" },
            new Lawyer { FullName="مجید غلام زاده", PhoneNumber="09166312706", City="خرمشهر", Address="خرمشهر روبروی خ سابق اراک روبروی آژانس نسیم" },
            new Lawyer { FullName="حمیدرضا فتلی ساکی", PhoneNumber="09163317883", City="خرمشهر", Address="خرمشهر ابتدای بلوار چهل متری روبروی بانک صادرات کوچه ریحانی" },
            new Lawyer { FullName="علی فتلی ساکی", PhoneNumber="09163338071", City="خرمشهر", Address="خرمشهر خیابان چهل متری خیابان خیام ساختمان کارون ط دوم واحد 4" },
            new Lawyer { FullName="فرناز فرزام پور", PhoneNumber="09177525811", City="خرمشهر", Address="خرمشهر جزیره مینو بلوار امام خمینی بعد از پاسگاه" },
            new Lawyer { FullName="نجمه مستخدم", PhoneNumber="09190775939", City="خرمشهر", Address="خرمشهر بلوار ساحلی جنب درمانگاه بهبهانی آقای عبدالسلام ساکی" },
            new Lawyer { FullName="محبوبه معرفی", PhoneNumber="09166741479", City="خرمشهر", Address="خرمشهر- فلکه اله- پاساژ ستارگان- ط 3- دفتر خانم جلیلیان" },
            new Lawyer { FullName="زهرا هاشمی", PhoneNumber="09163105260", City="خرمشهر", Address="خوزستان خرمشهر خ مطهری ساختمان کارون" },
            new Lawyer { FullName="سید راضی هاشمی", PhoneNumber="09166334105", City="خرمشهر", Address="خرمشهر چهار راه مطهری جنب بانک ملت بن بست حمید طبقه دوم" },
            new Lawyer { FullName="سعید یوسفی منش", PhoneNumber="09166310798", City="خرمشهر", Address="خرمشهر خیابان خیام ساختمان طبقه 4" },
            new Lawyer { FullName="حسن آب چهره", PhoneNumber="۰۹۱۶۷۲۴۳۸۹۲", City="خرمشهر", Address="خوزستان-خرمشهر- نقدی- خ میلانیان خ شهید فهمیده بن بست اول" },
            new Lawyer { FullName="محمد حسن ابوالفتح بیگی دزفولی", PhoneNumber="۰۹۱۲۱۲۴۳۸۵۱", City="خرمشهر", Address="خرمشهر خ ۴۰ متری نبش خیابان کرامت دفتر وکالت ابوالفتح بیگی کرامت" },
            new Lawyer { FullName="الهام ابوعلی", PhoneNumber="۰۹۱۶۳۳۱۱۴۳۱", City="خرمشهر", Address="خرمشهر میدان شهید مطهری -روبروی بازار روز طبقه دوم ساختمان مهر" },
            new Lawyer { FullName="علی زویداوی", PhoneNumber="۰۹۱۶۵۰۰۳۰۲۵", City="خرمشهر", Address="خرمشهر -فلکه دروازه خ فخررازی-نبش رودکی -مجتمع تجاری و مسکونی سالار طبقه اول واحد یک" },
            new Lawyer { FullName="بهسا سیاحتی", PhoneNumber="۰۹۱۶۶۱۴۴۸۶۴", City="خرمشهر", Address="خرمشهر خ ۴۰ متری روبروی کلانتری شماره ۱۱ نبش خ خیام ط فوقانی بانک ثامن الحجج" },
            new Lawyer { FullName="سعاد سعیدی هیزانی", PhoneNumber="۰۹۱۶۳۳۲۳۱۰۲", City="خرمشهر", Address="خرمشهر ابتدای خ ۴۰ متری جنب بانک رفاه نبشخ ریحان مجتمع قدرت طبقه سوم واحد ۴" },
            new Lawyer { FullName="سیده شریفه سلمان زاده", PhoneNumber="۰۹۱۳۴۵۷۸۳۵۸", City="خرمشهر", Address="خوزستان-خرمشهر- کوی شهید موسوی-خیابان ۹کشاورز جنب مسجد امام رضا-پلاک۴۵۶" },
            new Lawyer { FullName="شیما صبوری زاده", PhoneNumber="۰۹۱۶۳۳۲۵۳۵۸", City="خرمشهر", Address="خرمشهر خ ۴۰ متری روبه روی ساختمان مهد طبقه ۲" },
            new Lawyer { FullName="شیما صیدی", PhoneNumber="۰۹۲۱۱۶۵۶۱۹۴", City="خرمشهر", Address="خرمشهر چهارراه مطهری ابتدای خ ۴۰ متری مجتمع ستاره ها طبقه سوم" },
            new Lawyer { FullName="هنگامه طباخیان", PhoneNumber="۰۹۱۶۳۳۱۳۸۲۵", City="خرمشهر", Address="خرمشهر خ چهل متر خ رودکی پ ۷" },
            new Lawyer { FullName="کاظم عبودی", PhoneNumber="۰۹۱۶۹۳۴۵۰۹۵", City="خرمشهر", Address="خوزستان-خرمشهر- خیابان امام پشت بهزسیتی کوچه ۱۲ متری پ ۵۹" },
            new Lawyer { FullName="علی فاضلی", PhoneNumber="۰۹۱۶۳۳۲۴۱۴۷", City="خرمشهر", Address="خرمشهر خ ۴۰ متری نبش ریحان جنب بانک رفاه طبقه سوم موسسه عسکریه" },
            new Lawyer { FullName="احمد آدینه وند", PhoneNumber="۰۹۱۶۸۰۹۹۳۹۵", City="خرمشهر", Address="خوزستان-خرمشهر- خیابان رودکی نرسیده به فلکه اردیبهشت-پلاک۹" },
            new Lawyer { FullName="علی عابدی", PhoneNumber="۰۹۱۰۱۹۶۹۴۳۱", City="خرمشهر", Address="خرمشهر چهار راه مطهری خیابان خیام روبروی مدرسه آرهاشم" },
            new Lawyer { FullName="فاطمه حیدرزاده", PhoneNumber="۰۹۱۶۵۲۳۶۲۲۱", City="خرمشهر", Address="خرمشهر چهل متری نبش خ ریحان جنب بانک تجارت" },
            #endregion

         #region آبادان
            new Lawyer { FullName="آیدا آتش نژاد", PhoneNumber="۰۹۱۶۳۲۳۰۳۴۹", City="آبادان", Address="آبادان خ منتظری جنب هتل زیتون دفتر وکالت آقای فریدون شجاعی" },
            new Lawyer { FullName="نعیم آلبوغبیش", PhoneNumber="۰۹۱۶۹۲۱۰۲۰۶", City="آبادان", Address="آبادان خ امیری فرعی شهریار ساختمان کیهان طبقه 2 دفتر محمد صالح مویدی" },
            new Lawyer { FullName="سینا آیین پرست", PhoneNumber="۰۹۳۹۰۳۵۰۶۱۲", City="آبادان", Address="آبادان خ امام روبروی مجتمع اروند مجتمع پارسیک ط 3 دفتر سعید گلشن زاده" },
            new Lawyer { FullName="کریم احتشام نیا", PhoneNumber="۰۹۱۶۶۳۱۴۰۰۳", City="آبادان", Address="آبادان بلوار دبستان مجتمع ضیا طبقه 3 واحد 4" },
            new Lawyer { FullName="منصور احمد پور", PhoneNumber="۰۹۱۶۶۰۰۰۷۹۴", City="آبادان", Address="آبادان خ شهید منتظری جنب اداره کشتیرانی" },
            new Lawyer { FullName="عبدالامیر البوعلی", PhoneNumber="۰۹۱۶۶۳۴۶۷۵۶", City="آبادان", Address="آبادان خیابان امیری ساختمان بانک سامان دفتر وکالت آقای عالمی" },
            new Lawyer { FullName="ابراهیم امانی", PhoneNumber="۰۹۱۶۳۵۲۰۰۳۹", City="آبادان", Address="آبادان- خ گمرک-دفتر حمید سراجیان" },
            new Lawyer { FullName="اسرافیل امیری", PhoneNumber="۰۹۱۹۸۹۴۵۶۵۲", City="آبادان", Address="آبادان خ امیری روبرو پاساژ پارمیدا طبقه سوم" },
            new Lawyer { FullName="فاطمه بحرانی", PhoneNumber="۰۹۳۹۸۰۵۹۳۵۷", City="آبادان", Address="آبادان- خ امیری- فرعی شهریار- ساختمان کیهان- دفتر آقای محمد صالح مویدی" },
            new Lawyer { FullName="سیده فاطمه بهشتی", PhoneNumber="۰۹۱۱۱۲۷۳۸۹۲", City="آبادان", Address="آبادان بین چهار راه امیری و امام مجتمع صدرا طبقه سوم" },
            new Lawyer { FullName="جهانشاه پریرخ", PhoneNumber="۰۹۱۶۱۳۱۱۴۳۹", City="آبادان", Address="آبادان خیابان دبستان پلاک 5" },
            new Lawyer { FullName="بهاره توکلی پور", PhoneNumber="۰۹۱۶۸۰۵۲۶۳۹", City="آبادان", Address="آبادان خ گمرک ساختمان 42 طبقه همکف دفتر آقای سراجی پور" },
            new Lawyer { FullName="ایاد ثامری", PhoneNumber="۰۹۳۹۵۶۵۰۴۱۴", City="آبادان", Address="آبادان خیابان طالقانی ساختمان توسکا طبقه 2" },
            new Lawyer { FullName="بهنام جلیلیان", PhoneNumber="۰۹۱۶۳۳۱۷۰۸۵", City="آبادان", Address="آبادان خیابان امام فرعی هفتم مجتمع بهمن" },
            new Lawyer { FullName="فرهاد اسماعیلی فر", PhoneNumber="۰۹۱۷۳۱۹۴۷۷۵", City="آبادان", Address="آبادان چهارراه امیری خ زند بازار جمهوری ساختمان آذین" },
            new Lawyer { FullName="علی آلبوسوادی", PhoneNumber="۰۹۳۹۷۹۷۳۳۳۲", City="آبادان", Address="خوزستان-آبادان- کوی شهروند - کوچه شهروند۸ - پلاک ۶" },
            new Lawyer { FullName="غالب البوغبیش", PhoneNumber="۰۹۳۶۰۰۰۲۶۱۱", City="آبادان", Address="خوزستان-آبادان- ایستگاه ۶ قدس بهار ۴۰ پلاک ۵۰" },
            new Lawyer { FullName="حسین بغلانی", PhoneNumber="۰۹۱۶۶۳۰۳۴۸۰", City="آبادان", Address="آبادان خ دبستان نبش خ امام ـ مجتمع تجاری اداری ضیاء ط ۳ واحد ۳" },
            new Lawyer { FullName="یحیی پویا مهر", PhoneNumber="۰۹۱۶۶۳۱۰۸۷۰", City="آبادان", Address="آبادان-خیابان امیری ساختمان بانک سامان طبقه ۲ واحد ۳" },
            new Lawyer { FullName="اکرم حیدری", PhoneNumber="۰۹۱۲۴۸۵۲۸۰۲", City="آبادان", Address="ابادان خ امیری روبروی داروخانه مادر طبقه دوم ساختمان گالری نرجس" },
            new Lawyer { FullName="شیما دریس", PhoneNumber="۰۹۱۶۶۳۰۶۴۱۹", City="آبادان", Address="آبادان خ امیر کبیر ساختمان پارمیدا طبقه سوم واحد اول" },
            new Lawyer { FullName="محمد سعدونی", PhoneNumber="۰۹۱۶۶۱۶۳۹۳۳", City="آبادان", Address="آبادان-خیابان دبستان-خ امام-مجتمع تجاری ضیاء-ط۳-واحد۸" },
            new Lawyer { FullName="جادر سلیمانی", PhoneNumber="۰۹۱۰۶۹۶۰۴۹۴", City="آبادان", Address="ابادان-مجتمع سید ضیاء طبقه ۳ واحد۹" },
            new Lawyer { FullName="معصومه حمیدی", PhoneNumber="۰۹۱۶۶۱۳۳۷۸۴", City="آبادان", Address="ابادان خ زند نبش بازار جمهوری مجتمع ستاره جنوب واحد ۴۰۱" },
            new Lawyer { FullName="فرید داودی", PhoneNumber="۰۹۱۶۹۳۵۹۰۲۱", City="آبادان", Address="آبادان امیری بازار جمهوری پاساژ ستاره جنوب طبقه ۳ واحد۴۰۲" },
            new Lawyer { FullName="رضا دریس", PhoneNumber="۰۹۱۶۶۳۱۵۲۴۴", City="آبادان", Address="آبادان بین چهار راه امیری و امام مجتمع متین طبقه ۲ واحد ۴" },
            new Lawyer { FullName="سهام رابعی غلامی", PhoneNumber="۰۹۳۵۲۴۳۴۴۲۵", City="آبادان", Address="آبادان خیابان امیری فرعی شهریار ساختمان کیهان طبقه ۴" },
            new Lawyer { FullName="جواد جعفر پور", PhoneNumber="۰۹۱۶۳۳۰۱۳۰۱", City="آبادان", Address="آبادان خ امیری انتهای پاساژ جمهوری ساختمان آتروپات طبقه سوم واحد ۷" },
            new Lawyer { FullName="سجاد جهانبازی گوجانی", PhoneNumber="۰۹۱۲۱۸۲۱۳۵۴", City="آبادان", Address="خوزستان-آبادان-محله امیری-خ طالقانی-خ زین العابدین-پ۲۶-ط همکف" },
            new Lawyer { FullName="مرجان حلاج", PhoneNumber="09168797310", City="آبادان", Address="آبادان، خیابان امیری، مجتمع اداری زانوس، طبقه ششم، واحد" },
            new Lawyer { FullName="شریف کنعانی", PhoneNumber="09381259663", City="آبادان", Address="آبادان، بین چهارراه امیری امام، مجتمع متین، طبقه 2، واحد 4" },
            new Lawyer { FullName="فاطمه بهشتی", PhoneNumber="09169988404", City="آبادان", Address="آبادان، خیابان امیری، مجتمع متروپل، طبقه 8، واحد 804" },
            new Lawyer { FullName="فاطمه جمال", PhoneNumber="09364810309", City="آبادان", Address="آبادان، خیابان امیری فرعی شهریار، ساختمان کیهان، طبقه 2" },
            new Lawyer { FullName="بشیر طرفی", PhoneNumber="09167555351", City="آبادان", Address="آبادان، خیابان امیری، ساختمان اویسی، طبقه 3" },
            new Lawyer { FullName="عماد دغاغله", PhoneNumber="09167555351", City="آبادان", Address="آبادان، خیابان امیری، خیابان کاشانی، پلاک 20" },
            new Lawyer { FullName="یحیی آلبوسوادی", PhoneNumber="09307603639", City="آبادان", Address="آبادان خ شهید منتظری ساختمان زیتون طبقه اول دفتر آقای فریدون شجاعی" },
            new Lawyer { FullName="لیلا اردکانی نژاد", PhoneNumber="09161301392", City="آبادان", Address="آبادان امیری پاساژجمهوری ساختمان آتروپات دفتر آقای طرفی" },
            new Lawyer { FullName="علیرضا چهره افروز", PhoneNumber="09166336219", City="آبادان", Address="آبادان خیابان امام چهارراه امام ساختمان صدرا" },
            new Lawyer { FullName="مریم حیالی", PhoneNumber="09166058652", City="آبادان", Address="آبادان- خ امیری - نبش پرویزی- جنب داروخانه مادر- ساختمان زانوس- ط4- واحد 19" },
            new Lawyer { FullName="زهرا حیدری", PhoneNumber="09050905980", City="آبادان", Address="آبادان خیابان دبستان برج دبستان طبقه اول دفتر وکالت آقای محمدصالح مویدی" },
            new Lawyer { FullName="نعمت اله حیدری", PhoneNumber="09166312873", City="آبادان", Address="آبادان بازار جمهوری دفتر وکالت آقای امیر احمد محمدی" },
            new Lawyer { FullName="لیلا خالدی صالح پور", PhoneNumber="09163506640", City="آبادان", Address="آبادان خیابان امام فرعی هفتم مجتمع بهمن طبقه 3 دفتر آقای سعید گلشن زاده" },
            new Lawyer { FullName="مبینا شمس", PhoneNumber="09163308560", City="آبادان", Address="آبادان بین چهار راه امام و امیری روبروی مجتمع اروند دفتر آقای گلشن زاده" },
            new Lawyer { FullName="محمدرضا طرفی", PhoneNumber="09380192395", City="آبادان", Address="آبادان خیابان امیری ساختمان بانک سامان طبقه اول واحد 4" },
            new Lawyer { FullName="محمد طهماسبی بلداجی", PhoneNumber="09166311780", City="آبادان", Address="آبادان چهارراه امیری جنب بانک تجارت طبقه 3" },
            new Lawyer { FullName="سعید گلشن زاده", PhoneNumber="09173117586", City="آبادان", Address="آبادان بین چهار راه امام و امیری روبروی مجتمع اروند دفتر آقای گلشن زاده" },
            new Lawyer { FullName="امیر نصاری", PhoneNumber="09166311501", City="آبادان", Address="آبادان خ طالقانی ساختمان توت" },
            new Lawyer { FullName="بهاره کیان سرشت", PhoneNumber="09354082557", City="آبادان", Address="آبادان خیابان امیری نبش منتظری ساختمان بانک سامان دفتر وکالت آقای غانمی" },
            new Lawyer { FullName="نازنین یاقوتیان زاده", PhoneNumber="09166366506", City="آبادان", Address="آبادان خ امیری نبش چهار راه طبقه دوم" },
            new Lawyer { FullName="ستایش یحیی آبادی", PhoneNumber="09163305970", City="آبادان", Address="آبادان خیابان امیری - ساختمان پارمیدا - طبقه 5 واحد 25" },
            new Lawyer { FullName="افروز هندیانی", PhoneNumber="09163340256", City="آبادان", Address="آبادان دفتر وکالت آقای پریرخ" },
            new Lawyer { FullName="احمد نورانی", PhoneNumber="09336492693", City="آبادان", Address="آبادان خیابان امیری نبش منتظری ساختمان بانک سامان واحد 3" },
            new Lawyer { FullName="مجتبی نجفی", PhoneNumber="09139054867", City="آبادان", Address="آبادان خ طالقانی (زند) انتهای بازار جمهوری" },
            new Lawyer { FullName="محمدصالح مویدی", PhoneNumber="09163315646", City="آبادان", Address="آبادان- خ امیری- فرعی شهریار- ساختمان کیهان- دفتر آقای محمد صالح مویدی" },
            new Lawyer { FullName="جواد لامی نژاد", PhoneNumber="09166312648", City="آبادان", Address="آبادان خ امیری خ کاشانی ساختمان پارسیان طبقه 2 واحد 8" },
            new Lawyer { FullName="قیس فرحانی", PhoneNumber="09163347518", City="آبادان", Address="آبادان- بازار جمهوری- دفتر وکالت امیر احمد محمدی" },
            new Lawyer { FullName="فریدون شجاعی", PhoneNumber="09161310698", City="آبادان", Address="آبادان- خ شهید منتظری- ساختمان- پاسارگاد- طبقه اول" },
            new Lawyer { FullName="سمیه مولوی", PhoneNumber="09129001592", City="آبادان", Address="آبادان امیری فرعی شهریار دفتر آقای مویدی" },
            new Lawyer { FullName="امیرپارسا مرادی", PhoneNumber="09171757545", City="آبادان", Address="آبادان خ امیری خ زند ساختمان توسکا طبقه 2 واحد 3" },
            new Lawyer { FullName="زهرا محمدهاشمی", PhoneNumber="09163327118", City="آبادان", Address="آبادان خ دبستان پلاک 5" },
            new Lawyer { FullName="شهین فرید نیا", PhoneNumber="09123303566", City="آبادان", Address="آبادان منازل زمین شهری بلوار بهارستان پشت املاک یاران" },
            new Lawyer { FullName="راضیه فرخ زاده", PhoneNumber="09165038810", City="آبادان", Address="آبادان خیابان امیری دفتر امی احمد محمدی" },
            new Lawyer { FullName="علی غانمی", PhoneNumber="09163332129", City="آبادان", Address="آبادان خ امیری بازار کویتیها رئبروی پاساژ شهلایی" },
            new Lawyer { FullName="ابراهیم عرب پور", PhoneNumber="09160588286", City="آبادان", Address="آبادان خ طالقانی بازار جمهوری" },
            new Lawyer { FullName="سمیر سیحون", PhoneNumber="09163338559", City="آبادان", Address="آبادان خ آیت اله طالقانی جنب کتابفروشی مهران بن بست لادن طبقه فوقانی لوازم اداری تواناپور" },
            new Lawyer { FullName="نعیم سعیدی", PhoneNumber="09166332520", City="آبادان", Address="آبادان کوی بهار ایستگاه 9 ردیف 399 اتاق 8" },
            new Lawyer { FullName="حمید سراجی پور", PhoneNumber="09161313692", City="آبادان", Address="آبادان خیابان گمرک ساختمان 42 طبقه همکف" },
            new Lawyer { FullName="منا سبتاوی", PhoneNumber="09166338631", City="آبادان", Address="آبادان خ امیری جنب بانک سپه مرکزی ساختمان پارسا ط سوم واحد 8" },
            new Lawyer { FullName="علی رنجبر", PhoneNumber="09366425336", City="آبادان", Address="آبادان خ امیری فرعی شهریار ساختمان کیهان دفتر آقای مویدی" },
            new Lawyer { FullName="نازنین رضایی", PhoneNumber="09165752070", City="آبادان", Address="آبادان خیابان دبستان برج دبستان طبقه اول دفتر وکالت محمدصالح مویدی" },
            new Lawyer { FullName="سامان مهران", PhoneNumber="09122098778", City="آبادان", Address="آبادان بازار جمشید آباد خ بهمن 35 پلاک 44" },
            new Lawyer { FullName="قاسم ربیحاوی", PhoneNumber="09166260639", City="آبادان", Address="آبادان امیری ساختمان طبقه 4 واحد 404" },
            new Lawyer { FullName="میلاد گلک", PhoneNumber="09163340453", City="آبادان", Address="آبادان انتهای بازار جمهوری خیابان زند دفتر آقای امیراحمد محمدی" },
            new Lawyer { FullName="حسین یوسف نژادیان", PhoneNumber="09166310678", City="آبادان", Address="آبادان دفتر وکالت فریدون شجاعی" },
         #endregion                 
    };


    // لیست صفحه‌بندی شده
    [ObservableProperty]
    private ObservableCollection<Lawyer> lawyers = new();

    [ObservableProperty]
    private bool isBusy;

    [RelayCommand]
    public async Task LoadNotes()
    {
        IsBusy = true;
        await Task.Yield();

        // ۱. فیلتر بر اساس شهر
        List<Lawyer> filteredList;
        if (string.IsNullOrWhiteSpace(SelectedCity) || SelectedCity == "همه")
        {
            filteredList = AllLawyers.ToList();
        }
        else
        {
            filteredList = AllLawyers
                .Where(l => string.Equals(l.City, SelectedCity, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // ۲. صفحه‌بندی
        var pagedLawyers = filteredList
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        // ۳. پاک کردن ObservableCollection و اضافه کردن آیتم‌ها
        Lawyers.Clear();
        foreach (var lawyer in pagedLawyers)
            Lawyers.Add(lawyer);

        // ۴. بررسی وجود صفحه بعدی
        HasMorePages = (CurrentPage * PageSize) < filteredList.Count;

        IsBusy = false;
    }

    partial void OnSelectedCityChanged(string value)
    {
        CurrentPage = 1;
        LoadNotes().SafeFireAndForget();
    }

    [RelayCommand]
    public async Task LoadNextPageAsync()
    {
        if (!HasMorePages) return;
        CurrentPage++;
        await LoadNotes();
    }

    [RelayCommand]
    public async Task LoadPreviousPageAsync()
    {
        if (CurrentPage <= 1) return;
        CurrentPage--;
        await LoadNotes();
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsBusy = true;

        await Task.Yield();
        CurrentPage = 1;
        await LoadNotes();

        IsBusy = false;
    }


    private CancellationTokenSource _searchCts;

    [ObservableProperty]
    private string searchQuery;

    partial void OnSearchQueryChanged(string value)
    {
        SearchAsync().SafeFireAndForget();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var ct = _searchCts.Token;

        try
        {
            var q = SearchQuery?.Trim();
            if (string.IsNullOrEmpty(q))
            {
                // وقتی سرچ خالی باشه → برگرد به حالت صفحه‌بندی
                await LoadNotes();
                return;
            }

            // debounce کوتاه
            await Task.Delay(100, ct);

            var result = await Task.Run(() =>
            {
                // ابتدا فیلتر شهر را اعمال می‌کنیم
                IEnumerable<Lawyer> filtered = AllLawyers;
                if (!string.IsNullOrWhiteSpace(SelectedCity) && SelectedCity != "همه")
                {
                    filtered = filtered.Where(l =>
                        string.Equals(l.City, SelectedCity, StringComparison.OrdinalIgnoreCase));
                }

                // سپس جستجو روی کل لیست فیلتر شده
                filtered = filtered.Where(l =>
                    (!string.IsNullOrEmpty(l.FullName) && l.FullName.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(l.PhoneNumber) && l.PhoneNumber.Contains(q))
                );

                return filtered.ToList();
            }, ct);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Lawyers.Clear();
                foreach (var lawyer in result)
                    Lawyers.Add(lawyer);

                // وقتی سرچ می‌کنیم صفحه‌بندی غیرفعال بشه
                HasMorePages = false;
            });
        }
        catch (OperationCanceledException)
        {
            // نادیده گرفتن
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Search Error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ShowDetails(Lawyer lawyer)
    {
        if (lawyer == null) return;

        var popup = new LawyerDetailsPopup(lawyer.FullName, lawyer.PhoneNumber, lawyer.Address);
        await MopupService.Instance.PushAsync(popup);
    }

    private void LoadCities()
    {
        Cities = new ObservableCollection<string>
        {
            {"همه"},{"خرمشهر"},{"آبادان"}
        };
    }

    private void LoadUserState()
    {
        var role = Preferences.Get("UserRole", "Unknown");
        var isRegistered = Preferences.Get("IsLawyerRegistered", false);

        if (role == "Unknown")
        {
            IsLawyer = false;
            CanRegisterLawyer = true;
            ShowRegisterLabel = true;
            LawyerLicensevisibility = false;
        }
        else
        {
            IsLawyer = role == "Lawyer" && isRegistered;
            CanRegisterLawyer = !isRegistered;
            ShowRegisterLabel = !isRegistered;
            LawyerLicensevisibility = isRegistered;
            LawyerFullName = Preferences.Get("LawyerFullName", string.Empty);
            LawyerLicense = Preferences.Get("LawyerLicense", string.Empty);
        }
    }

    [RelayCommand]
    public async Task OpenLawyerPopup()
    {
        // ✍️ اگر هنوز ثبت‌نام نکرده، فرم ثبت‌نام رو نشون بده
        var popup = new LawyerSubmitPopup(_userService);
        await MopupService.Instance.PushAsync(popup);
    }

    public class Lawyer
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
    }
}