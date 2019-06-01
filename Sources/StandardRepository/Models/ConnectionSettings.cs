namespace StandardRepository.Models
{
    public class ConnectionSettings
    {
        public string DbName { get; set; }
        public string DbHost { get; set; }
        public string DbPort { get; set; }
        public string DbUser { get; set; }
        public string DbPassword { get; set; }

        public string DbNameMaster { get; set; }
        public string DbUserMaster { get; set; }

        public ConnectionSettings()
        {
            DbNameMaster = "postgres";
            DbUserMaster = "postgres";
        }
    }
}