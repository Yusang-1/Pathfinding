public interface IProcessor<in TIn, out TOut>
{
    TOut Process(TIn input);
}

public delegate TOut ProcessorDelegate<in TIn, out TOut>(TIn input);

public class Combined<A, B, C> : IProcessor<A, C>
{
    private readonly IProcessor<A, B> first;
    private readonly IProcessor<B, C> second;

    public Combined(IProcessor<A, B> first, IProcessor<B, C> second)
    {
        this.first = first;
        this.second = second;
    }

    public C Process(A input) => second.Process(first.Process(input));
}
