﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Threading;
using WowsKarma.Api.Data.Models.Notifications;
using WowsKarma.Api.Services;
using WowsKarma.Common.Hubs;



namespace WowsKarma.Api.Hubs;


[Authorize]
public class NotificationsHub : Hub<INotificationsHubPush>, INotificationsHubInvoke
{
	private readonly NotificationService _service;

	public NotificationsHub(NotificationService service)
	{
		_service = service;
	}

	public Task AcknowledgeNotifications(Guid[] notificationIds)
	{
		IQueryable<NotificationBase> notifications = _service.GetNotifications(notificationIds).Where(n => n.AccountId == uint.Parse(Context.UserIdentifier));
		_service.AcknowledgeNotifications(notifications);
		return Task.CompletedTask;
	}

	public async IAsyncEnumerable<(string, object)> GetPendingNotifications([EnumeratorCancellation] CancellationToken ct)
	{
		ConfiguredCancelableAsyncEnumerable<NotificationBase> notifications = _service.GetPendingNotifications(uint.Parse(Context.UserIdentifier))
			.AsNoTracking()
			.AsAsyncEnumerable()
			.WithCancellation(ct);

		await foreach (NotificationBase item in notifications)
		{
			ct.ThrowIfCancellationRequested();
			object notificationDTO = item.ToDTO();

			yield return (notificationDTO.GetType().FullName, notificationDTO);
		}
	}
}