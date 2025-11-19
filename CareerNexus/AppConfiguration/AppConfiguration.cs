namespace CareerNexus.AppConfiguration
{
    public class AppConfiguration
    {
        public static IConfigurationRoot _config = null;
        public static string _environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        public static string _appConfigPath = _environmentName == "Development" ? $"{Environment.CurrentDirectory}\\appsettings.Development.json" : $"{Environment.CurrentDirectory}\\appsettings.Production.json";

        public static string _dbConnection = string.Empty;
        public static string _secret = string.Empty;
        public static string _validIssuer = string.Empty;
        public static string _validAudience = string.Empty;
        public static int _tokenDurationInMinutes = 0;

        public static IConfigurationRoot LoadConfiguration()
        {
            if (_config == null)
            {
                _config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                LoadAppSettings();
            }

            return _config;
        }

        private static void LoadAppSettings()
        {
            _dbConnection = _config.GetConnectionString("DefaultConnection")
                 ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

            _secret = _config["AppKeys:Secret"];
            _validIssuer = _config["AppKeys:ValidIssuer"];
            _validAudience = _config["AppKeys:ValidAudience"];
            int.TryParse(_config["AppKeys:TokenDurationInMinutes"], out _tokenDurationInMinutes);
        }
    }
    }
