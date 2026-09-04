using System.Threading.Channels;

namespace SecurityAwareness.Application.Interfaces;

public interface IEmailQueueChannel
{
    ChannelWriter<EmailMessage> Writer { get; }
    ChannelReader<EmailMessage> Reader { get; }
}
