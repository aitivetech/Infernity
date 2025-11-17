namespace Infernity.Framework.Core.Collections;

public static class CollectionExtensions
{
    extension<T>(ICollection<T> target)
    {
        public void AddAll(IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                target.Add(item);
            }
        }
    }
}