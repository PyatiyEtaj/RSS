using ExcelDataReader;
using ExtensionLib.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace ExtensionLib.Excel
{
    public class ExcelParserListWorkingGroup
    {
        private List<WorkType> _workTypes { get; set; }
        private List<WorkingPeriod> _workingPeriods { get; set; }
        private List<ConstructionPhase> _constructionPhases { get; set; }
        private List<GroupingOfWork> _groupingOfWorks { get; set; }
        
        private DataSet GetDataSet(string excelPath)
        {
            if (!File.Exists(excelPath)) throw new Exception($"Отсутствует файл {excelPath}");
            using (var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    return reader.AsDataSet();
                }
            }
            throw new Exception($"Ошибка во время чтения файла {excelPath}");
        }

        private T ConvertCell<T>(object element)
            => element is DBNull ? default : (T)element;

        private RPKShipher CreateShipher(DataRow row) =>
            new RPKShipher()
            {
                S = $"{row[0]}",
                F = $"{row[1]}",
                C = $"{row[2]}",
                M = $"{row[3]}",
                X_M = $"{row[4]}",
                P = $"{row[5]}"
            };
        private WorkingPeriod GetWorkingPeriod(DataRow row)
        {
            var code = CreateShipher(row);
            string period = ConvertCell<string>(row[6]);
            int priority = (int)ConvertCell<double>(row[7]);
            return new WorkingPeriod()
            {
                Period = period,
                RPKShipher = code,
                Priority = priority
            };
        }
        private List<WorkingPeriod> ReadWorkingPeriod(DataTable table)
        {
            var wp = new List<WorkingPeriod>();
            for (int i = 2; i < table.Rows.Count; i++)
            {
                wp.Add(GetWorkingPeriod(table.Rows[i]));
            }
            return wp;
        }

        private ConstructionPhase GetConstructionPhase(DataRow row)
        {
            var code = CreateShipher(row);
            string phase = ConvertCell<string>(row[6]);
            int priority = (int)ConvertCell<double>(row[7]);
            string name = ConvertCell<string>(row[8]);
            return new ConstructionPhase()
            {
                RPKShipher = code,
                Name = name,
                Phase = phase,
                Priority = priority
            };
        }
        private List<ConstructionPhase> ReadConstructionPhase(DataTable table)
        {
            var cp = new List<ConstructionPhase>();
            for (int i = 2; i < table.Rows.Count; i++)
            {
                cp.Add(GetConstructionPhase(table.Rows[i]));
            }
            return cp;
        }

        private GroupingOfWork GetGroupingOfWork(DataRow row)
        {
            var code = CreateShipher(row);
            string rso = ConvertCell<string>(row[6]);
            string kp = ConvertCell<string>(row[7]);
            int number = (int)ConvertCell<double>(row[8]);
            int priority = (int)ConvertCell<double>(row[9]);
            return new GroupingOfWork()
            {
                RPKShipher = code,
                GroupingRSO = rso,
                GroupingKP = kp,
                Number = number,
                Priority = priority
            };
        }
        private List<GroupingOfWork> ReadGroupingOfWork(DataTable table)
        {
            var gow = new List<GroupingOfWork>();
            for (int i = 2; i < table.Rows.Count; i++)
            {
                gow.Add(GetGroupingOfWork(table.Rows[i]));
            }
            return gow;
        }

        private WorkType GetWorkType(DataRow row)
        {
            var code = CreateShipher(row);
            string worktypeRSOKP = ConvertCell<string>(row[6]);
            int priority = (int)ConvertCell<double>(row[7]);
            string name = ConvertCell<string>(row[8]);
            string worktype = ConvertCell<string>(row[9]);
            string unit = ConvertCell<string>(row[10]);
            string shortName = ConvertCell<string>(row[11]);
            string accrual = ConvertCell<string>(row[12]);
            int order = (int)ConvertCell<double>(row[13]);
            return new WorkType()
            {
                RPKShipher = code,
                Accrual = accrual,
                Name = name,
                Order = order,
                ShortName = shortName,
                Type = worktype,
                TypeRSOKP = worktypeRSOKP,
                Unit = unit,
                Priority = priority
            };
        }
        private List<WorkType> ReadWorkType(DataTable table)
        {
            var wt = new List<WorkType>();
            for (int i = 2; i < table.Rows.Count; i++)
            {
                wt.Add(GetWorkType(table.Rows[i]));
            }
            return wt;
        }

        public (
            List<WorkType> workTypes, 
            List<WorkingPeriod> workingPeriods,
            List<ConstructionPhase> constructionPhases,
            List<GroupingOfWork> groupingOfWorks
        ) Read(string excelPath)
        {
            var dataset = GetDataSet(excelPath);
            var list = new List<DataTable>();
            foreach (DataTable item in dataset.Tables)
            {
                list.Add(item);
            }
            Parallel.ForEach(list, new ParallelOptions { MaxDegreeOfParallelism = 4 }, (table) =>
            {
                switch (table.TableName.Trim())
                {
                    case "Период работ":
                        _workingPeriods = ReadWorkingPeriod(table);
                        break;
                    case "Этап строительства":
                        _constructionPhases = ReadConstructionPhase(table);
                        break;
                    case "Группировка работ":
                        _groupingOfWorks = ReadGroupingOfWork(table);
                        break;
                    case "Тип работ":
                        _workTypes = ReadWorkType(table);
                        break;
                    default:
                        break;
                }
            });

            return (
                _workTypes, 
                _workingPeriods, 
                _constructionPhases, 
                _groupingOfWorks);
        }
        public override string ToString()
            => $"wp={_workingPeriods.Count}  cp={_constructionPhases.Count}  gop={_groupingOfWorks.Count}  wt={_workTypes.Count}";
    }
}
