namespace Infernity.Framework.Core.Collections;

public static class ListExtensions
{
    extension<T>(List<T> sortedList)
    {
        public int FindNextSmallerOrEqual(T target)
        {
            int index = sortedList.BinarySearch(target);
        
            if (index >= 0)
            {
                // The element is found
                return index;
            }

            // Find the next larger element
            index = ~index;

            // Check if there is a smaller or equal element in the list
            if (index == 0)
            {
                // No smaller element exists
                return -1;
            }

            // Return the next smaller element
            return index - 1;
        }

        public List<T> Merge(List<T> source,Func<T,T,bool> equalsPredicate)
        {
            var result = new List<T>(sortedList);
        
            foreach (var item in source)
            {
                result.ReplaceOrAdd(item,equalsPredicate);
            }

            return result;
        }

        public void ReplaceOrAdd(T value,Func<T,T,bool> equalsPredicate)
        {
            var index = sortedList.FindIndex(t => equalsPredicate(t, value));

            if (index >= 0)
            {
                sortedList[index] = value;
            }
            else
            {
                sortedList.Add(value);
            }
        }
    }
}