using System.Collections.Generic;

namespace RiceTea.Core.Threading;

public static partial class Swapable
{
    public static ISwapable<T> Create<T>(bool optimistic = false) where T : class, new()
        => Create(new T(), new T(), optimistic);

    public static ISwapable<T> Create<T>(T front, T back, bool optimistic = false) where T : class
        => optimistic ? new OptimisticImpl<T>(front, back) : new PessimisticImpl<T>(front, back);

    public static ISwapable<List<T>> CreateList<T>(bool optimistic = false)
        => Create(new List<T>(), new List<T>(), optimistic);

    public static ISwapable<List<T>> CreateList<T>(int capacity, bool optimistic = false)
    {
        List<T> front = new List<T>(capacity);
        List<T> back = new List<T>();
        return Create(front, back, optimistic);
    }

    public static ISwapable<List<T>> CreateList<T>(IEnumerable<T> collection, bool optimistic = false)
    {
        List<T> front = new List<T>(collection);
        List<T> back = new List<T>();
        return Create(front, back, optimistic);
    }

    public static ISwapable<Queue<T>> CreateQueue<T>(bool optimistic = false)
        => Create(new Queue<T>(), new Queue<T>(), optimistic);

    public static ISwapable<Queue<T>> CreateQueue<T>(int capacity, bool optimistic = false)
    {
        Queue<T> front = new Queue<T>(capacity);
        Queue<T> back = new Queue<T>();
        return Create(front, back, optimistic);
    }

    public static ISwapable<Queue<T>> CreateQueue<T>(IEnumerable<T> collection, bool optimistic = false)
    {
        Queue<T> front = new Queue<T>(collection);
        Queue<T> back = new Queue<T>();
        return Create(front, back, optimistic);
    }

    public static ISwapable<Stack<T>> CreateStack<T>(bool optimistic = false)
        => Create(new Stack<T>(), new Stack<T>(), optimistic);

    public static ISwapable<Stack<T>> CreateStack<T>(int capacity, bool optimistic = false)
    {
        Stack<T> front = new Stack<T>(capacity);
        Stack<T> back = new Stack<T>();
        return Create(front, back, optimistic);
    }

    public static ISwapable<Stack<T>> CreateStack<T>(IEnumerable<T> collection, bool optimistic = false)
    {
        Stack<T> front = new Stack<T>(collection);
        Stack<T> back = new Stack<T>();
        return Create(front, back, optimistic);
    }
}

public interface ISwapable<T> where T : class
{
    T Value { get; }

    T Swap();
}
