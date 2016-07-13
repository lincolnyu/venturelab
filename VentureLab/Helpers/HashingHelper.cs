using System.Linq;

public static class HashingHelper
{
    /// <summary>
    ///  Returns a hashcode for a collection of objects based of their individiual hashcode
    /// </summary>
    /// <param name="items">The array of objects</param>
    /// <returns>The hashcode for the objects</returns>
    public static int GetHashCodeForItems(params object[] items)
    {
        return GetHashCodeForItems(items.Select(x => x.GetHashCode()).ToArray());
    }

    /// <summary>
    /// Returns a hashcode for a collection of hashcodes
    /// </summary>
    /// <param name="items">an array of hashcodes</param>
    /// <returns>The hashcode for the collection</returns>
    /// <remarks>
    /// http://stackoverflow.com/questions/892618/create-a-hashcode-of-two-numbers
    /// </remarks>
    public static int GetHashCodeForItems(params int[] items)
    {
        return items.Aggregate(23, (current, item) => current * 31 + item);
    }
}