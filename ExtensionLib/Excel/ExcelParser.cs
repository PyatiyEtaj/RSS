﻿using ExcelDataReader;
using ExtensionLib.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace ExtensionLib.Excel
{
    public class ExcelParser
    {
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

        private T ConvertCell<T>(object element) where T : IConvertible
        {
            if (element is DBNull) return default;
            if (typeof(T) == typeof(double))
            {
                double.TryParse(element.ToString().Replace('.', ','), out double tmp);                
                element = tmp;
            }

            return (T)element;
        }

        private Material GetMaterialFromRow(DataRow row, int rownumber)
        {
            try
            {
                Material m = new Material()
                {
                    Price = ConvertCell<double>(row[4]),
                    Coefficient = ConvertCell<double>(row[5]),
                    Unit = ConvertCell<string>(row[6]),
                    Name = ConvertCell<string>(row[7]),
                    Note = ConvertCell<string>(row[8]),
                    Section = ConvertCell<string>(row[9]),
                    Number = (int)ConvertCell<double>(row[10])
                };

                return m;
            }
            catch (Exception)
            {
                throw new Exception($"[price_material_coefficient] Ошибка во время создания материала строка = {rownumber}");
            }
        }

        private ClassMaterial GetClassMaterial(DataRowCollection rows, ref int i)
        {
            var materials = new List<Material>();
            RPKShipher shipher = default;
            if (!(rows[i][0] is DBNull))
            {
                shipher = new RPKShipher ()
                {
                    C = $"{rows[i][0]}",
                    M = $"{rows[i][1]}",
                    X_M = $"{rows[i][2]}",
                    P = $"{rows[i][3]}"
                };
                materials.Add(GetMaterialFromRow(rows[i], i));
            }
            i++;
            while (i < rows.Count && rows[i][0] is DBNull)
            {
                materials.Add(GetMaterialFromRow(rows[i], i));
                i++;
            }
            for (int j = 1; j < materials.Count; j++)
            {
                if (materials[j].Name is null || materials[j].Name.Trim().Length == 0)
                    materials[j].Name = materials[0].Name;
                if (materials[j].Unit is null || materials[j].Unit.Trim().Length == 0)
                    materials[j].Unit = materials[0].Unit;
            }
            return new ClassMaterial ()
            {
                RPKShipher = shipher,
                Materials = materials
            };
        }

        private (RPKShipher shipher, double priceForWork) GetPriceForWork(DataRowCollection rows, int i)
        {
            if (!(rows[i][0] is DBNull))
            {
                try
                {
                    var shipher = new RPKShipher()
                    {
                        C = $"{rows[i][0]}",
                        M = $"{rows[i][1]}",
                        X_M = $"{rows[i][2]}",
                        P = $"{rows[i][3]}"
                    };
                    var priceForWork = ConvertCell<double>(rows[i][5]);
                    return (shipher, priceForWork);
                }
                catch (Exception ex)
                {
                    throw new Exception($"[price_database] Ошибка в строке {i} { ex.InnerException?.Message }");
                }
            }
            return (default, default);
        }

        public List<ClassMaterial> Read(string excelPath)
        {
            var classes = new List<ClassMaterial>();
            var dataset = GetDataSet(excelPath);
            foreach (DataTable table in dataset.Tables)
                if (table.TableName.Trim() == "price_material_coefficient")
                    for (int i = 1; i < table.Rows.Count;)
                    {
                        if (!table.Rows[i].ItemArray.All(o => String.IsNullOrEmpty(o?.ToString())))
                            classes.Add(GetClassMaterial(table.Rows, ref i));
                        else
                            i++;
                    }

            foreach (DataTable table in dataset.Tables)
                if (table.TableName.Trim() == "price_database")
                    for (int i = 1; i < table.Rows.Count; i++)
                    {
                        if (!table.Rows[i].ItemArray.All(o => String.IsNullOrEmpty(o?.ToString())))
                        {
                            var data = GetPriceForWork(table.Rows, i);                                          // >= 0
                            var cm = classes.FirstOrDefault(x => x.RPKShipher.CompareTo(data.shipher, false, true) != RPKShipherCompEnum.NotEqual);

                            if (cm != default)
                            {
                                cm.PriceForWork = data.priceForWork;
                            }
                        }                            
                    }

            return classes;
        }
    }
}
