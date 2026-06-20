using Med_Map.DTO.NotificationDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Med_Map.Hubs;

[Authorize]
public class NotificationHub : Hub<INotificationClient>
{
}
