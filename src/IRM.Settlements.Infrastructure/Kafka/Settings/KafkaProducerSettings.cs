using Confluent.Kafka;

namespace IRM.Settlements.Infrastructure.Kafka.Settings;

public class KafkaProducerSettings
{
    public const string SectionName = nameof(KafkaProducerSettings);
    public required string BootstrapServers { get; set; }
    public SaslMechanism SaslMechanism { get; set; } = SaslMechanism.ScramSha512;
    public SecurityProtocol SecurityProtocol { get; set; } = SecurityProtocol.SaslSsl;
    public required string SslCaLocation { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string TopicName { get; set; }
}
