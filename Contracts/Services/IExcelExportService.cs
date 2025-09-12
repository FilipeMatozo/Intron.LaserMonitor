using Intron.LaserMonitor.Models;

namespace Intron.LaserMonitor.Contracts.Services;

public interface IExcelExportService
{
    void Export(IEnumerable<Measurement> data, string filePath);
}
