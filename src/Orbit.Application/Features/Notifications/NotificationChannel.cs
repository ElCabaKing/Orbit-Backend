using System.Threading.Channels;

namespace Orbit.Application.Features.Notifications;

public class NotificationChannel
{
    public Channel<NotificationEvent> Channel { get; } = System.Threading.Channels.Channel.CreateUnbounded<NotificationEvent>();
}
