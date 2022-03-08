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
	public virtual string Username { get; init; }

	[Required]
	public uint ModId { get; init; }
	public virtual string ModUsername { get; init; }

	[Required]
	public string Reason { get; set; }

	public Instant? BannedUntil { get; set; }

	public bool Reverted { get; set; }

	public Instant CreatedAt { get; init; }
	public Instant UpdatedAt { get; set; }
}
