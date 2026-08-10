using System;
using System.Threading;

namespace RiceTea.Core.Threading;

partial class State
{
    private sealed class MultipleWriterImpl<T> : State<T> where T : struct
    {
        private readonly Lock _writerLock = new();

        public MultipleWriterImpl(T value) : base(value) { }

        public override T Value
        {
            get => GetValue();
            set
            {
                lock (_writerLock)
                    SetValue(value);
            }
        }

        public override void Update(Func<T, T> updateFactory)
        {
            T value = updateFactory.Invoke(GetValue(out nuint version));
            lock (_writerLock)
            {
                if (TrySetValue(value, version))
                    return;
                SetValue(updateFactory.Invoke(GetValueUnsafe()));
            }
        }

        public override void Update<TState>(Func<T, TState, T> updateFactory, TState state)
        {
            T value = updateFactory.Invoke(GetValue(out nuint version), state);
            lock (_writerLock)
            {
                if (TrySetValue(value, version))
                    return;
                SetValue(updateFactory.Invoke(GetValueUnsafe(), state));
            }
        }
    }
}
