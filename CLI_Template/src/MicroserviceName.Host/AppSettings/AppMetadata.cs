namespace MicroserviceName.Host.AppSettings
{
    public class AppMetadataSettings
    {
        public string Name { get; set; } = null!;
        public string Version { get; set; } = null!;
        public string? Slice { get; set; }
        public string? Port { get; set; }
    }
}
