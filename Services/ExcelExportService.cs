using Intron.LaserMonitor.Contracts.Services;
using Intron.LaserMonitor.Models;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;

namespace Intron.LaserMonitor.Services
{
    public class ExcelExportService : IExcelExportService
    {
        public void Export(IEnumerable<Measurement> data, string filePath)
        {
            ExcelPackage.License.SetNonCommercialOrganization("My Noncommercial organization");

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Medições Laser");

                worksheet.Cells[1, 1].Value = "Timestamp";
                worksheet.Cells[1, 2].Value = "Distância (m)";
                worksheet.Cells[1, 1, 1, 2].Style.Font.Bold = true;

                int row = 2;
                foreach (var point in data)
                {
                    worksheet.Cells[row, 1].Value = point.Timestamp;
                    worksheet.Cells[row, 1].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss.000";
                    worksheet.Cells[row, 2].Value = point.Distance;
                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                File.WriteAllBytes(filePath, package.GetAsByteArray());
            }
        }
    }
}