using System;
using System.Numerics;
using BenchmarkDotNet.Attributes;

namespace VectorRegression;

public class DirectHeapVsStackComparison
{

    [Benchmark(Baseline = true)]
    public Double ExpectedStrictUpperBound()
    {
        // Expect this to be a strict upper bound, due to heap allocated data.
        Span<Double> heapAllocatedData = new Double[Vector<Double>.Count];
        var vectorFromHeapData = new Vector<Double>(heapAllocatedData);
        return Vector.Sum(vectorFromHeapData);
    }

    [Benchmark] // Linux: ratio=1.15 | Windows: ratio=1.61
    public Double ActualRuntime()
    {
        // We'd expect the ratio to be strictly smaller than 1, but it's 1.15.
        // Indicating that using vectorization on stack allocated data is approx 15% slower
        // than operating on heap allocated data. This is the opposite of what would 
        // be expected.
        Span<Double> stackAllocatedData = stackalloc Double[Vector<Double>.Count];
        var vectorFromStackData = new Vector<Double>(stackAllocatedData);
        return Vector.Sum(vectorFromStackData);
    }
}

/*** Linux | Vector<Double>.Count = 4 ***
// * Summary *

BenchmarkDotNet=v0.13.3, OS=arch 
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK=7.0.101
  [Host]     : .NET 7.0.1 (7.0.122.61501), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.1 (7.0.122.61501), X64 RyuJIT AVX2


|                   Method |     Mean |     Error |    StdDev | Ratio |
|------------------------- |---------:|----------:|----------:|------:|
| ExpectedStrictUpperBound | 6.918 ns | 0.0518 ns | 0.0432 ns |  1.00 |
|            ActualRuntime | 7.966 ns | 0.0217 ns | 0.0203 ns |  1.15 |
*/

/*** Windows | Vector<Double>.Count = 4 ***
 // * Summary *

BenchmarkDotNet=v0.13.3, OS=Windows 10 (10.0.19044.2251/21H2/November2021Update)
Intel Core i7-10610U CPU 1.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=7.0.101
  [Host]     : .NET 7.0.1 (7.0.122.56804), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.1 (7.0.122.56804), X64 RyuJIT AVX2


|                   Method |     Mean |     Error |    StdDev | Ratio | RatioSD |
|------------------------- |---------:|----------:|----------:|------:|--------:|
| ExpectedStrictUpperBound | 4.613 ns | 0.0563 ns | 0.0470 ns |  1.00 |    0.00 |
|            ActualRuntime | 7.415 ns | 0.1762 ns | 0.1809 ns |  1.61 |    0.05 |
*/
