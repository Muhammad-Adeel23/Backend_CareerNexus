using System.ComponentModel;

namespace CareerNexus.Common
{
    public class Enumerator
    {
    }
    public enum Databaseoperations
    {
        [Description("Select")]
        Select=0,
        [Description("Insert")]
        Insert = 1,
        [Description("Update")]
        Update = 2,
        [Description("Delete")]
        Delete = 3,
        [Description("Get")]
        Get = 4,
        [Description("GetDataSet")]
        GetDataSet = 5,
        [Description("GetDataTable")]
        GetDataTable = 6,
        [Description("BulkInsert")]
        BulkInsert = 7, 
        [Description("BulkUpdate")]
        BulkUpdate = 8,
        [Description("SQLBlock")]
        SQLBlock = 9,
    }
}
