using System.Collections.Generic;

namespace Freecon.Core
{
    /// <summary>
    /// //How the fuck does this still not exist in System.Collections? 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CyclicalIterator<T>
    {
        IEnumerable<T> _items;

        IEnumerator<T> _iterator;

        /// <summary>
        /// Does not move the iterator.
        /// </summary>
        public T Current { get { return _iterator.Current; } }

        public CyclicalIterator(IEnumerable<T> collection)
        {
            _items = collection;
            _iterator = collection.GetEnumerator();
            _iterator.MoveNext();
        }

        /// <summary>
        /// Gets Current, then moves the iterator.
        /// </summary>
        /// <returns></returns>
        public T GetCurrentMoveNext()
        {
            T cur = Current;
            MoveNext();
            
            return cur;
        }

        public void MoveNext()
        {
            if(!_iterator.MoveNext())
            {
                reset();
            }
        }


        void reset()
        {
            _iterator = _items.GetEnumerator();//Apparently .Reset() is not guaranteed to work with all collections?
            _iterator.MoveNext();
        }
    }
}
