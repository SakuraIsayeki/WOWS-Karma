using System;
using System.ComponentModel.DataAnnotations;
using WowsKarma.Common.Models;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Web.Models
{
	public record PlayerPostViewModel
	{
		public Guid? Id { get; set; }

		[Required]
		public uint PlayerId { get; set; }

		[Required]
		public uint AuthorId { get; set; }

		public PostFlairs Flairs { get; set; }

		[Required, StringLength(60, MinimumLength = 5)]
		public string Title { get; set; }
		[Required, StringLength(2000, MinimumLength = 50)]
		public string Content { get; set; }



		public static implicit operator PlayerPostViewModel(PlayerPostDTO value) => new()
		{
			Id = value.Id,
			PlayerId = value.PlayerId,
			AuthorId = value.AuthorId,
			Flairs = value.Flairs,
			Title = value.Title,
			Content = value.Content
		};

		public static PlayerPostDTO ToDTO(PlayerPostViewModel value) => new()
		{
			Id = value.Id,
			PlayerId = value.PlayerId,
			AuthorId = value.AuthorId,
			Flairs = value.Flairs,
			Title = value.Title,
			Content = value.Content,
		};
	}
}
