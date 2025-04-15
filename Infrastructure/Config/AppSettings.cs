namespace NavalBattle.Infrastructure.Config
{
    public class AppSettings
    {
        public ServiceBusSettings ServiceBus { get; set; }
        public ShipSettings Ship { get; set; }
    }

    public class ServiceBusSettings
    {
        public string ConnectionString { get; set; }
        public string TopicName { get; set; }
        public string SubscriptionName { get; set; }
    }

    public class ShipSettings
    {
        public string Name { get; set; }
        public string CryptoKey { get; set; }
        public string Salt { get; set; }
    }
} 