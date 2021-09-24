
using ExtensionLib.Entity;
using ExtensionLib.Excel;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using RevitService.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitService.Services.KS.Logic
{
    internal class KsCreator
    {
        private const int _tableStart = 30;
        private int _row = _tableStart;
        private delegate void ForRange(ExcelRange r);
        private readonly ExcelParser _excelParser = new();
        private readonly SocketServerEntities.Util.SpinLock sl = new();
        //private ExcelParserListWorkingGroup _excelWorkingGroup = new();

        private async Task SaveAsync(ExcelPackage p, string name)
            => await p.SaveAsAsync(new FileInfo(name));


        private void Perform(ExcelWorksheet w, string cells, ForRange forRange)
        {
            using (var r = w.Cells[cells])
            {
                forRange(r);
                r.Style.Font.Name = "New Times Roman";
                r.Style.Font.Size = 10;
            }
        }

        private void MakeABody(ExcelWorksheet w, List<ForgeElement> elements)
        {
            var classMaterials = _excelParser.Read(FilePathes.ExcelPrices);
            //var (workTypes, workingPeriods, constructionPhases, groupingOfWorks)
            //    = _excelWorkingGroup.Read(FilePathes.ExcelWorkGroups);
            int rownumber = 1;
            Parallel.ForEach(
                elements.GroupBy(x => new { x.Construction, x.Material, x.CharMat, x.Params }),
                (groupped) =>
                {
                    double square = 0.0, volume = 0.0;
                    RPKShipher shipher = new($"*.*.{groupped.Key.Construction}.{groupped.Key.Material}.{groupped.Key.CharMat}.{groupped.Key.Params}");
                    var cm = classMaterials.FirstOrDefault(x => shipher.CompareTo(x.RPKShipher) == RPKShipherCompEnum.SemiEqual);
                    if (cm is null) return;

                    StringBuilder name = new($" {cm.Materials[0].Name} [{shipher.ShortCode}]");
                    foreach (var section in groupped.GroupBy(x => x.Section))
                    {
                        name.Append($"\n{section.Key}: ");
                        foreach (var floor in section.GroupBy(x => x.Floor))
                        {
                            name.Append($" {floor.Key}");
                            double locals = 0.0, localv = 0.0;
                            foreach (var el in floor)
                            {
                                locals += Math.Round(el.Square ?? 0.0, 3);
                                localv += Math.Round(el.Volume ?? 0.0, 3);
                            }
                            name.Append($"(m2: {locals}, m3: {localv})");
                            square += Math.Round(locals, 3);
                            volume += Math.Round(localv, 3);
                        }
                    }
                    sl.Lock();
                    w.Cells[$"A{_row}"].Value = rownumber++;
                    w.Cells[$"C{_row}"].Value = name;
                    w.Cells[$"D{_row}"].Value = cm.Materials[0].Unit;
                    w.Cells[$"D{_row}"].Value = cm.Materials[0].Unit;
                    w.Cells[$"E{_row}"].Value = $"площадь: {square}\nобъем: {volume}";
                    w.Cells[$"A{_row}:M{_row}"].Style.Font.Bold = true;
                    _row++;
                    int rowstart = _row;
                    if (cm.Materials.Count > 1)
                    {
                        for (int i = 1; i < cm.Materials.Count; i++)
                        {
                            var coef = cm.Materials[i].Coefficient;
                            w.Cells[$"C{_row}"].Value = cm.Materials[i].Name;
                            w.Cells[$"D{_row}"].Value = cm.Materials[i].Unit;
                            w.Cells[$"F{_row}"].Value = coef;
                            _row++;
                        }
                        w.Cells[$"C{rowstart}:H{_row - 1}"].Style.Font.Italic = true;
                    }
                    sl.Unlock();
                }
            );
        }

        public async Task<bool> CreateAsync(List<ForgeElement> elements, string template, string excelName)
        {
            if (elements.Count < 1) return false;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var p = new ExcelPackage(new FileInfo(template)))
            {
                var worksheet = p.Workbook.Worksheets[0];
                MakeABody(worksheet, elements);
                worksheet.Cells[$"A{_tableStart}:M{_row - 1}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[$"A{_tableStart}:M{_row - 1}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[$"A{_tableStart}:M{_row - 1}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[$"A{_tableStart}:M{_row - 1}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[$"A{_tableStart}:M{_row - 1}"].Style.Font.Size = 10;
                worksheet.Cells[$"A{_tableStart}:M{_row - 1}"].Style.Font.Name = "New Times Roman";
                worksheet.Cells.Style.WrapText = true;

                worksheet.Cells[$"A{_tableStart}:B{_row - 1}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[$"D{_tableStart}:M{_row - 1}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                await SaveAsync(p, excelName);
            }
            return true;
        }
    }
}
