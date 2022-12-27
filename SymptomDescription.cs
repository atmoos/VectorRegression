using System;
using System.Numerics;
using BenchmarkDotNet.Attributes;

namespace VectorRegression;

public class SymptomDescription
{
    // The actual values contained in 'data' are irrelevant.
    private static readonly Double[] heapAllocatedData = new Double[Vector<Double>.Count];

    [Benchmark(Baseline = true)]
    public Double ExpectedUpperBound()
    {
        var vectorFromHeapData = new Vector<Double>(heapAllocatedData);
        return Vector.Sum(vectorFromHeapData);
    }

    [Benchmark] // Linux: ratio=7.07 | Windows: ratio=10.90
    public Double ActualRuntime()
    {
        // The ratio should be approximately 1.88, but it's 7.07, approx 3.8 times slower than expected!
        Span<Double> stackData = stackalloc Double[Vector<Double>.Count];
        var vectorFromStackData = new Vector<Double>(stackData);
        return Vector.Sum(vectorFromStackData);
    }

    [Benchmark] // Linux: ratio=0.88 | Windows: ratio=1.43
    public Double CostOfStackAllocation()
    {
        // The actual runtime above should be no longer than expected plus the duration of this benchmark.
        Span<Double> stackData = stackalloc Double[Vector<Double>.Count];
        return stackData.Length;
    }
}

/*** Linux | Vector<Double>.Count = 4 ***
// * Summary *

BenchmarkDotNet=v0.13.3, OS=arch 
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK=7.0.101
  [Host]     : .NET 7.0.1 (7.0.122.61501), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.1 (7.0.122.61501), X64 RyuJIT AVX2


|                Method |      Mean |     Error |    StdDev | Ratio | RatioSD |
|---------------------- |----------:|----------:|----------:|------:|--------:|
|    ExpectedUpperBound | 1.0949 ns | 0.0471 ns | 0.0441 ns |  1.00 |    0.00 |
|         ActualRuntime | 7.7402 ns | 0.0183 ns | 0.0162 ns |  7.07 |    0.29 |
| CostOfStackAllocation | 0.9660 ns | 0.0172 ns | 0.0161 ns |  0.88 |    0.03 |
*/

/*** Windows | Vector<Double>.Count = 4 ***
// * Summary *

BenchmarkDotNet=v0.13.3, OS=Windows 10 (10.0.19044.2251/21H2/November2021Update)
Intel Core i7-10610U CPU 1.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=7.0.101
  [Host]     : .NET 7.0.1 (7.0.122.56804), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.1 (7.0.122.56804), X64 RyuJIT AVX2


|                Method |      Mean |     Error |    StdDev | Ratio | RatioSD |
|---------------------- |----------:|----------:|----------:|------:|--------:|
|    ExpectedUpperBound | 0.6974 ns | 0.0431 ns | 0.0560 ns |  1.00 |    0.00 |
|         ActualRuntime | 7.3555 ns | 0.1648 ns | 0.1542 ns | 10.90 |    1.08 |
| CostOfStackAllocation | 0.9481 ns | 0.0119 ns | 0.0093 ns |  1.43 |    0.16 |
*/