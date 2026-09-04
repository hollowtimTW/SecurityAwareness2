using System.Threading.Channels;
using SecurityAwareness.Application.Interfaces;

namespace SecurityAwareness.Application.Services;

/// <summary>
/// Singleton channel that buffers outgoing emails between Controller and BackgroundService.
/// </summary>
public class EmailQueueChannel : IEmailQueueChannel
{
    private readonly Channel<EmailMessage> _channel = Channel.CreateUnbounded<EmailMessage>();

    public ChannelWriter<EmailMessage> Writer => _channel.Writer;
    public ChannelReader<EmailMessage> Reader => _channel.Reader;
}
