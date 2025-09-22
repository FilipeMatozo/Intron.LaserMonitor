using Intron.LaserMonitor.Contracts.Services;
using Intron.LaserMonitor.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Collections.Generic;
using System.IO;

namespace Intron.LaserMonitor.Services
{
    public class ExcelExportService : IExcelExportService
    {
        public void Export(IEnumerable<Measurement> data, string filePath)
        {
            ExcelPackage.License.SetNonCommercialOrganization("My Noncommercial organization");

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Medições Laser");

            worksheet.Cells[1, 1].Value = "Timestamp";
            worksheet.Cells[1, 2].Value = "Distância (mm)";
            worksheet.Cells[1, 3].Value = "Distância Absoluta (mm)";
            worksheet.Cells[1, 1, 1, 3].Style.Font.Bold = true;

            int row = 2;
            foreach (var point in data)
            {
                worksheet.Cells[row, 1].Value = point.Timestamp;
                worksheet.Cells[row, 1].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss.000";
                worksheet.Cells[row, 2].Value = point.Distance;
                worksheet.Cells[row, 3].Value = point.DistanceAbsolute;
                row++;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            File.WriteAllBytes(filePath, package.GetAsByteArray());
        }

        public void Export(IEnumerable<IEnumerable<Measurement>> allBatches, string filePath)
        {
            // EPPlus licensing (ajuste conforme seu projeto)
            ExcelPackage.License.SetNonCommercialPersonal("Intron");

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Medições Laser");

            // Cabeçalhos fixos
            ws.Cells[1, 1].Value = "Timestamp";
            ws.Cells[1, 2].Value = "Distância Absoluta (mm)";
            ws.Cells[1, 1, 1, 2].Style.Font.Bold = true;

            // Configs
            const string TimestampFormat = "yyyy-mm-dd hh:mm:ss.000";
            const string NumberFormatMm = "0.000";

            int row = 2;                // linha atual de escrita (contínua)
            int seriesNumber = 0;       // contador de colunas de Distância Relativa
            int firstRelativeCol = 3;   // C = 3

            // Percorre cada lote (série lógica)
            foreach (var series in allBatches)
            {
                if (series == null || !series.Any())
                    continue;

                seriesNumber++;
                int relCol = firstRelativeCol + (seriesNumber - 1);

                // Cabeçalho da coluna deste lote
                ws.Cells[1, relCol].Value = seriesNumber == 1
                    ? "Distância Relativa #1 (mm)"
                    : $"Distância Relativa #{seriesNumber} (mm)";
                ws.Cells[1, relCol].Style.Font.Bold = true;

                // Preenche linha a linha, mantendo Timestamp/Absoluta contínuos
                foreach (var m in series)
                {
                    // Coluna A: Timestamp
                    ws.Cells[row, 1].Value = m.Timestamp;
                    ws.Cells[row, 1].Style.Numberformat.Format = TimestampFormat;

                    // Coluna B: Distância Absoluta (mm) - contínua
                    ws.Cells[row, 2].Value = m.DistanceAbsolute;
                    ws.Cells[row, 2].Style.Numberformat.Format = NumberFormatMm;

                    // Coluna Relativa do lote atual
                    ws.Cells[row, relCol].Value = m.Distance;
                    ws.Cells[row, relCol].Style.Numberformat.Format = NumberFormatMm;

                    row++;
                }
            }

            // Visual
            if (ws.Dimension is not null)
            {
                // Congelar linha de cabeçalho
                ws.View.FreezePanes(2, 1);

                // Borda na linha de cabeçalho
                using (var r = ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                {
                    r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                ws.Cells[ws.Dimension.Address].AutoFitColumns();

                // Larguras mínimas agradáveis
                ws.Column(1).Width = Math.Max(ws.Column(1).Width, 20); // Timestamp
                ws.Column(2).Width = Math.Max(ws.Column(2).Width, 18); // Abs
                for (int c = 3; c <= ws.Dimension.End.Column; c++)
                    ws.Column(c).Width = Math.Max(ws.Column(c).Width, 18);
            }

            // Salva
            File.WriteAllBytes(filePath, package.GetAsByteArray());
            package.Dispose();
        }
    }
}