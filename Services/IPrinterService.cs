using System.Threading.Tasks;

namespace Vakilaw.Services
{
    public interface IPrinterService
    {
        Task PrintTextAsync(string text, string jobName = "Vakilaw Contract");
    }
}