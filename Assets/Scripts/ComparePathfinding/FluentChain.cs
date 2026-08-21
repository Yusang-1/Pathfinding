using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.Scripts.Pathfinding;

public class FluentChain<TIn, TOut, TDerived> where TDerived : FluentChain<TIn, TOut, TDerived>
{
    // TDerived tell the base calss to return the derived type
    // fluent chain stays prefectly type-safe, type-aware at every step

    // if TDerived is removed C# would refuse to let any derived class claim that it could return a more specific type than a base
    // the compiler would force every then method call to return just FluentChain, and you'd lose your exact derived type after the first step

    public IProcessor<TIn, TOut> processor;

    protected FluentChain(IProcessor<TIn, TOut> processor)
    {
        this.processor = processor ?? throw new ArgumentNullException(nameof(processor));
    }

    protected TNextSelf Then<TNext, TNextSelf, TProcessor>(TProcessor nextProcessor, ChainFactory<TIn, TNext, TNextSelf> factory)
        where TNextSelf : FluentChain<TIn, TNext, TNextSelf>
        where TProcessor : class, IProcessor<TOut, TNext>
    {
        if (nextProcessor == null) throw new ArgumentNullException(nameof(nextProcessor));
        if (factory == null) throw new ArgumentNullException(nameof(factory));

        return factory(new Combined<TIn, TOut, TNext>(processor, nextProcessor));
    }

    public TOut Run(TIn input)
    {
        if (processor == null) throw new InvalidOperationException();

        return processor.Process(input);
    }

    public ProcessorDelegate<TIn, TOut> Compile()
    {
        if (processor == null) throw new InvalidOperationException();

        return processor.Process;
    }
}

// behind the scenes factory that lets each stage of the chain create the next one without any restirctions on how the concrete chain classes are built
public delegate TChain ChainFactory<out TIn, in TOut, out TChain>(IProcessor<TIn, TOut> processor) where TChain : FluentChain<TIn, TOut, TChain>;

public class FindAStarPathChain : FluentChain<(Vector3 from, Vector3 to), List<Vector3>, FindAStarPathChain>
{
    public FindAStarPathChain(IProcessor<(Vector3 from, Vector3 to), List<Vector3>> processor) : base(processor) { }
}

public class FindClusterPathChain : FluentChain<ClusterResultWrapper, ClusterResultWrapper, FindClusterPathChain>
{
    public FindClusterPathChain(IProcessor<ClusterResultWrapper, ClusterResultWrapper> processor) : base(processor) { }

    public FindClusterPathChain SmoothHPAPath(ClusterPathSmoother clusterPathSmoother)
    {
        processor = new Combined<ClusterResultWrapper, ClusterResultWrapper, ClusterResultWrapper>(processor, new ProcessorSmoothClusterPath(clusterPathSmoother));
        return new FindClusterPathChain(processor);
    }

    static FindAStarPathWithClusterChain CreateFindPathChain(IProcessor<ClusterResultWrapper, List<Vector3>> processor)
    {
        return new FindAStarPathWithClusterChain(processor);
    }
    
    public FindAStarPathWithClusterChain Then<TProcessor>(TProcessor processor) where TProcessor : class, IProcessor<ClusterResultWrapper, List<Vector3>>
        => base.Then<List<Vector3>, FindAStarPathWithClusterChain, TProcessor>(processor, CreateFindPathChain);
}

public class FindAStarPathWithClusterChain : FluentChain<ClusterResultWrapper, List<Vector3>, FindAStarPathWithClusterChain>
{
    public FindAStarPathWithClusterChain(IProcessor<ClusterResultWrapper, List<Vector3>> processor) : base(processor) { }
}

public class FindThetaPathWithClusterChain : FluentChain<(Vector3 from, Vector3 to), List<Vector3>, FindThetaPathWithClusterChain>
{
    public FindThetaPathWithClusterChain(IProcessor<(Vector3 from, Vector3 to), List<Vector3>> processor) : base(processor) { }
}
