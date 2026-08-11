using System.Threading;

namespace RiceTea.Core.Threading;

partial class Swapable
{
    private sealed class PessimisticImpl<T> : ISwapable<T> where T : class
    {
        private readonly Lock _syncLock = new Lock();

        private T _front, _back;

        public PessimisticImpl(T front, T back)
        {
            _front = front;
            _back = back;
        }

        public T Value
        {
            get
            {
                lock (_syncLock)
                    return _front;
            }
        }

        public T Swap()
        {
            T result;
            lock (_syncLock)
            {
                result = _front;
                _front = _back;
                _back = result;
            }
            return result;
        }
    }
}
