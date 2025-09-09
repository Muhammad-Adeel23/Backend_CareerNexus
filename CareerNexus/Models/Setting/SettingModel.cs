namespace CareerNexus.Models.Setting
{
    public class SettingModel
    {

        public long Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public bool IsActive { get; set; }
    }
}
