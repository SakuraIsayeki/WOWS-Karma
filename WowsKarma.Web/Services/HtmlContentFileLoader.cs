using Microsoft.AspNetCore.Components;
using System.IO;



namespace WowsKarma.Web.Services
{
	public static class HtmlContentFileLoader
	{
		public const string FileExtension = ".html";

		internal static MarkupString LoadHtmlContent(FileInfo fileInfo) => new(File.ReadAllText(fileInfo.FullName));
		public static MarkupString LoadHtmlContent(string fileName)
		{
			fileName = fileName.EndsWith(FileExtension) ? fileName : (fileName + FileExtension);
			FileInfo file = new(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", fileName));
			return new(File.ReadAllText(file.FullName));
		}
	}
}