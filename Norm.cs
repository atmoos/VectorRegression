using System;
using System.Numerics;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;

namespace VectorRegression;

public class Norm
{
    private Double[] array;
    private Double[] arrayOffByOne;

    [Params(1, 3, 12, 21)]
    public Int32 Chunks { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var fullLength = Vector<Double>.Count * this.Chunks;
        this.array = new Double[fullLength];
        this.arrayOffByOne = new Double[fullLength - 1];
    }

    [Benchmark(Baseline = true)]
    public Double TrivialNorm() => TrivialTwoNorm(this.array);

    [Benchmark]
    public Double AcceleratedTwoNorm() => TwoNorm(this.array);

    [Benchmark]
    public Double AcceleratedTwoNorm_OffByOne() => TwoNorm(this.arrayOffByOne);

    [Benchmark]
    public Double AcceleratedTwoNorm_OffByOne_UsingTail() => TwoNormWithTail(this.arrayOffByOne);

    private static T TrivialTwoNorm<T>(T[] array)
    where T : unmanaged, IRootFunctions<T>
    {
        T sum = T.Zero;
        for (Int32 index = 0; index < array.Length; ++index) {
            T element = array[index];
            sum += element * element;
        }
        return T.Sqrt(sum);
    }
    private static T TwoNorm<T>(T[] array)
        where T : unmanaged, IRootFunctions<T>
    {
        var sum = Init(array);
        sum *= sum;
        var chunks = MemoryMarshal.Cast<T, Vector<T>>(array);
        for (Int32 index = 0; index < chunks.Length; ++index) {
            var chunk = chunks[index];
            sum += chunk * chunk;
        }
        return T.Sqrt(Vector.Sum(sum));
    }

    private static T TwoNormWithTail<T>(T[] array)
    where T : unmanaged, IRootFunctions<T>, IFloatingPointIeee754<T>
    {
        var chunks = MemoryMarshal.Cast<T, Vector<T>>(array);
        var vectorSum = chunks.Length > 0 ? chunks[0] * chunks[0] : Vector<T>.Zero;
        for (Int32 index = 1; index < chunks.Length; ++index) {
            var chunk = chunks[index];
            vectorSum += chunk * chunk;
        }

        Int32 tail = array.Length % Vector<T>.Count;
        T sum = Vector.Sum(vectorSum);
        for (Int32 index = array.Length - tail; index < array.Length; ++index) {
            var element = array[index];
            sum = T.FusedMultiplyAdd(element, element, sum);
        }
        return T.Sqrt(sum);
    }

    private static Vector<T> Init<T>(T[] array)
        where T : unmanaged
    {
        Int32 remainder = array.Length % Vector<T>.Count;
        if (remainder == 0) {
            return Vector<T>.Zero;
        }
        Span<T> tail = ((Span<T>)array)[^remainder..];
        Span<T> start = stackalloc T[Vector<Double>.Count];
        tail.CopyTo(start);
        return new Vector<T>(start);
    }
}

/*
// * Summary *

BenchmarkDotNet=v0.13.3, OS=arch 
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK=7.0.102
  [Host]     : .NET 7.0.2 (7.0.223.6001), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.2 (7.0.223.6001), X64 RyuJIT AVX2


|                                Method | Chunks |      Mean |     Error |    StdDev | Ratio | RatioSD |
|-------------------------------------- |------- |----------:|----------:|----------:|------:|--------:|
|                           TrivialNorm |      1 |  2.244 ns | 0.0357 ns | 0.0334 ns |  1.00 |    0.00 |
|                    AcceleratedTwoNorm |      1 |  6.648 ns | 0.1276 ns | 0.1194 ns |  2.96 |    0.04 |
|           AcceleratedTwoNorm_OffByOne |      1 | 18.395 ns | 0.0286 ns | 0.0239 ns |  8.17 |    0.11 |
| AcceleratedTwoNorm_OffByOne_UsingTail |      1 |  3.636 ns | 0.0584 ns | 0.0546 ns |  1.62 |    0.02 |
|                                       |        |           |           |           |       |         |
|                           TrivialNorm |      3 |  5.798 ns | 0.0216 ns | 0.0180 ns |  1.00 |    0.00 |
|                    AcceleratedTwoNorm |      3 |  8.815 ns | 0.2007 ns | 0.1971 ns |  1.53 |    0.04 |
|           AcceleratedTwoNorm_OffByOne |      3 | 23.622 ns | 0.0646 ns | 0.0504 ns |  4.07 |    0.02 |
| AcceleratedTwoNorm_OffByOne_UsingTail |      3 |  4.447 ns | 0.0322 ns | 0.0269 ns |  0.77 |    0.00 |
|                                       |        |           |           |           |       |         |
|                           TrivialNorm |     12 | 29.927 ns | 0.1704 ns | 0.1510 ns |  1.00 |    0.00 |
|                    AcceleratedTwoNorm |     12 | 22.524 ns | 0.1968 ns | 0.1840 ns |  0.75 |    0.01 |
|           AcceleratedTwoNorm_OffByOne |     12 | 45.130 ns | 0.1589 ns | 0.1327 ns |  1.51 |    0.01 |
| AcceleratedTwoNorm_OffByOne_UsingTail |     12 | 10.061 ns | 0.0181 ns | 0.0160 ns |  0.34 |    0.00 |
|                                       |        |           |           |           |       |         |
|                           TrivialNorm |     21 | 56.011 ns | 0.2706 ns | 0.2259 ns |  1.00 |    0.00 |
|                    AcceleratedTwoNorm |     21 | 35.591 ns | 0.2934 ns | 0.2450 ns |  0.64 |    0.01 |
|           AcceleratedTwoNorm_OffByOne |     21 | 71.750 ns | 0.1691 ns | 0.1412 ns |  1.28 |    0.01 |
| AcceleratedTwoNorm_OffByOne_UsingTail |     21 | 17.197 ns | 0.1119 ns | 0.1047 ns |  0.31 |    0.00 |
*/