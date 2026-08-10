using System;

namespace RiceTea.Core.Threading;

partial class State
{
    private sealed class SingleWriterImpl<T> : State<T> where T : struct
    {
        public SingleWriterImpl(T value) : base(value) { }

        public override T Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        public override void Update(Func<T, T> updateFactory)
            => Value = updateFactory.Invoke(GetValueUnsafe());

        public override void Update<TState>(Func<T, TState, T> updateFactory, TState state)
            => Value = updateFactory.Invoke(GetValueUnsafe(), state);
    }
}
