using System.Threading;

namespace RiceTea.Core.Threading;

partial class Swapable
{
    private sealed class OptimisticImpl<T> : ISwapable<T> where T : class
    {
        private StateTiny.SingleWriter<(T Front, T Back)> _state;
        private nuint _barrier;

        public OptimisticImpl(T front, T back)
        {
            _state = new StateTiny.SingleWriter<(T Front, T Back)>(new(front, back));
            _barrier = 0;
        }

        public T Value => _state.GetValueReferenceUnsafe().Front;

        public T Swap()
        {
            if (Atomics.Exchange(ref _barrier, Booleans.TrueNativeUnsigned) != default)
            {
                SpinWait wait = new SpinWait();
                do
                    wait.SpinOnce();
                while (Atomics.Exchange(ref _barrier, Booleans.TrueNativeUnsigned) != default);
            }
            try
            {
                (T front, T back) = _state.GetValueUnsafe();
                _state.Value = new(back, front);
                return front;
            }
            finally
            {
                Atomics.Write(ref _barrier, default);
            }
        }
    }
}
