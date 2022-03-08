namespace WowsKarma.Common.Models
{
	public interface INotification
	{
		public Guid Id { get; }

		public uint AccountId { get; }

		public NotificationType Type { get; }

		public DateTimeOffset EmittedAt { get; }

		public DateTimeOffset? AcknowledgedAt { get; }
	}
}
