using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WowsKarma.Common.Models.DTOs.Notifications
{
	public record NotificationBaseDTO : INotification
	{
		public Guid Id { get; init; }

		public uint AccountId { get; init; }

		public NotificationType Type { get; init; }

		public DateTimeOffset EmittedAt { get; init; }

		public DateTimeOffset? AcknowledgedAt { get; init; }


	}
}
