using CareerNexus.Common;
using CareerNexus.Models.Setting;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CareerNexus.Services.Setting
{
    public class SettingService:ISettingService
    {
        private static readonly string connectionString = "Server=localhost\\SQLEXPRESS;Database=CareerNexus;User Id=Adeel123;Password=test123;TrustServerCertificate=True;";

        public static List<T> ConvertToList<T>(DataTable dt)
        {
            var columnNames = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName.ToLower()).ToList();
            var properties = typeof(T).GetProperties();
            return dt.AsEnumerable().Select(row =>
            {
                var objT = Activator.CreateInstance<T>();
                foreach (var pro in properties)
                {
                    if (columnNames.Contains(pro.Name.ToLower()))
                    {
                        try
                        {
                            pro.SetValue(objT, row[pro.Name]);
                        }
                        catch (Exception ex) { }
                    }
                }
                return objT;
            }).ToList();
        }



        private SettingModel DecryptedSensitiveKeys(SettingModel data)
        {
            var decryptKey = new Encrypt_BT();
            if (data != null)
            {
                if (data.Key == SystemVariables.ElasticSearchpassword)
                {
                    if (decryptKey.isEncrypted(data.Value))
                    {
                        data.Value = decryptKey.DecryptString(data.Value);
                    }
                }
            }

            return data;
        }
        public static string GetSettingsKeyValue(string settingKey)
        {
            SettingService settingService = new SettingService();
            var setting = settingService.GetSettingsKey(settingKey);
            if (setting != null)
            {
                return setting.Value;
            }
            else
            {
                return null;
            }
        }
        public SettingModel GetSettingsKey(string key, bool? logFlagOnAndOff = null)
        {
            try
            {
                var cmd = new SqlCommand  
                {
                    CommandType = CommandType.Text,
                    CommandText = $"SELECT * FROM SETTING WHERE [key] = '{key}'"
                };
                DataTable dt = DBEngine.GetDataTable(cmd, Databaseoperations.SQLBlock, cmd.CommandText, logFlagOnAndOff);
                var data = ConvertToList<SettingModel>(dt)?.FirstOrDefault();

                if (data != null)
                {
                    data = DecryptedSensitiveKeys(data);
                }
                return data;

            }
            catch (Exception ex)
            {
                throw;
            }

        }
    }
}
