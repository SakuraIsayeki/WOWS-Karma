using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WowsKarma.Common.Models.DTOs;

public record PlatformBanDTO
{
	public Guid Id { get; init; }

	[Required]
	public uint UserId { get; init; }
	public virtual string Username { get; init; } = string.Empty;

	[Required]
	public uint ModId { get; init; }

	public virtual string ModUsername { get; init; } = string.Empty;

	[Required]
	public string Reason { get; set; } = string.Empty;

	public DateTime? BannedUntil { get; set; }

	public bool Reverted { get; set; }

	public DateTime CreatedAt { get; init; }
	public DateTime UpdatedAt { get; set; }
}
