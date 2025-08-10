using System.Data;
using System.Text.Json;

namespace CareerNexus.Common
{
    public static class Helper
    {
        public static List<T> ToModelList<T>(this DataTable table) where T : new()
        {
            var properties = typeof(T).GetProperties().Where(p => p.CanWrite).ToList();
            var result = new List<T>();

            foreach (DataRow row in table.Rows)
            {
                T item = new T();

                foreach (var prop in properties)
                {
                    if (!table.Columns.Contains(prop.Name) || row[prop.Name] == DBNull.Value)
                        continue;

                    try
                    {
                        var value = row[prop.Name];

                        // Handle Dictionary<string, string>
                        if (prop.PropertyType == typeof(Dictionary<string, string>))
                        {
                            var json = value.ToString();
                            var deserialized = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                            prop.SetValue(item, deserialized);
                        }
                        // Handle List<string>
                        else if (prop.PropertyType == typeof(List<string>))
                        {
                            var json = value.ToString();
                            var deserialized = JsonSerializer.Deserialize<List<string>>(json);
                            prop.SetValue(item, deserialized);
                        }
                        else
                        {
                            var safeValue = Convert.ChangeType(value, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                            prop.SetValue(item, safeValue);
                        }
                    }
                    catch
                    {
                        // Optionally log the failed property
                    }
                }

                result.Add(item);
            }

            return result;
        }
    }
}
