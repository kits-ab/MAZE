namespace MAZE
{
    public class EventStoreSettings
    {
        public string UserName { get; set; } = "admin";

        public string Password { get; set; } = "changeit";

        public string FullyQualifiedDomainName { get; set; } = "localhost";

        public int Port { get; set; } = 1113;
    }
}
