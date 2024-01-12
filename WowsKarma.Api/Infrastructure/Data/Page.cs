namespace WowsKarma.Api.Infrastructure.Data;

/// <summary>
/// Represents a paged list of items.
/// </summary>
/// <typeparam name="T">The type of the items in the list.</typeparam>
public sealed record Page<T> : PageMeta
{
	/// <summary>
	/// Builds a new <see cref="Page{T}"/> instance.
	/// </summary>
	/// <param name="items">The items in the page.</param>
	/// <param name="itemsCount">The total number of items in the list.</param>
	/// <param name="page">The page number.</param>
	/// <param name="pageSize">The page size.</param>
	public Page(IQueryable<T> items, int itemsCount, int page, int pageSize)
	{
		Items = items;
		ItemsCount = itemsCount;
		CurrentPage = page;
		PageSize = pageSize;
	}

	/// <summary>
	/// The items in the page.
	/// </summary>
	public IQueryable<T> Items { get; }
}

/// <summary>
/// Represents meta elements for a paged list of items.
/// </summary>
public record PageMeta
{
	/// <summary>
	/// The total number of items in the list.
	/// </summary>
	public int ItemsCount { get; protected init; }

	/// <summary>
	/// The current page number.
	/// </summary>
	public int CurrentPage { get; protected init; }

	/// <summary>
	/// The size of the page.
	/// </summary>
	public int PageSize { get; protected init; }
	
	/// <summary>
	/// The total number of pages.
	/// </summary>
	public int TotalPages => (int)Math.Ceiling(ItemsCount / (double)PageSize);
}