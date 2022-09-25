using System.Runtime.CompilerServices;
using WowsKarma.Api.Infrastructure.Data;

namespace WowsKarma.Api.Utilities;

/// <summary>
/// Various LINQ-related extensions and utilities.
/// </summary>
public static class LinqExtensions
{
	/// <summary>
	/// Breaks a sequence into chunks/pages of a given size, and returns the chunk at the given index.
	/// </summary>
	/// <typeparam name="T">The type of the elements in the sequence.</typeparam>
	/// <param name="source">The sequence to chunk/page.</param>
	/// <param name="pageSize">The size of each chunk/page.</param>
	/// <param name="index">The index of the chunk/page to return.</param>
	/// <returns>The chunk/page at the given index.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="pageSize"/> or <paramref name="index"/> are out of range.</exception>
	public static Page<T> Page<T>(this IQueryable<T> source, int pageSize, int index)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(index), "Index must be greater than or equal to 0.");
		}

		if (pageSize < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than or equal to 0.");
		}

		// Get the total number of items in the sequence.
		int total = source.Count();
		
		// Calculate the total number of pages.
		int totalPages = (int)Math.Ceiling(total / (double)pageSize);
		
		// If the index is out of range, throw an exception.
		if (index > totalPages)
		{
			throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
		}
		
		// Skip the first (index * pageSize) items, and take the next pageSize items.
		IQueryable<T> page = source.Skip(index * pageSize).Take(pageSize);
		
		// Return the page.
		return new(page, total, index, pageSize);
	}
} 