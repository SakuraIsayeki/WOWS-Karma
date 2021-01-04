using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using WowsKarma.Common.Models;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Api.Data.Models
{
	public record Post : IDataModel<Guid>, ITimestamped
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Id { get; init; }

		[Required]
		public uint PlayerId { get; init; }
		[Required]
		public Player Player { get; init; }
		[Required]
		public uint AuthorId { get; init; }
		[Required]
		public Player Author { get; init; }

		public PostFlairs Flairs { get; set; }
		public PostFlairsParsed ParsedFlairs => Flairs.ParseFlairsEnum();
	
		public bool NegativeKarmaAble { get; init; }

		public string Title { get; set; }
		public string Content { get; set; }

		// Computed by DB Engine (hopefully)
		public DateTime CreatedAt { get; init; }
		public DateTime UpdatedAt { get; set; }


		public static implicit operator PlayerPostDTO(Post value) => new()
		{
			Id = value.Id,
			PlayerId = value.PlayerId,
			AuthorId = value.AuthorId,
			Title = value.Title,
			Content = value.Content,
			Flairs = value.Flairs,
			PostedAt = value.CreatedAt,
			UpdatedAt = value.UpdatedAt
		};
	}


}
