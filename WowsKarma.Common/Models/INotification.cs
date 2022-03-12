namespace WowsKarma.Common.Models
{
	public interface INotification
	{
		public Guid Id { get; }

		public uint AccountId { get; }

		public NotificationType Type { get; }

		public DateTime EmittedAt { get; }

		public DateTime? AcknowledgedAt { get; }
	}
}
