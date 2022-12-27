using System;
using System.Numerics;
using BenchmarkDotNet.Attributes;

namespace VectorRegression;

public class Analysis
{
    private static readonly Double[] heapAllocatedData = new Double[Vector<Double>.Count];
    private static readonly Vector<Double> vectorFromHeapData = new(heapAllocatedData);

    [Benchmark] // Linux: runtime=0.9431 ns | Windows: runtime=0.9026 ns
    public Double CopySpanToVector_IsNotSlow()
    {
        Span<Double> stackData = stackalloc Double[Vector<Double>.Count];
        var vectorFromStackData = new Vector<Double>(stackData);
        return vectorFromStackData[0];
    }

    [Benchmark] // Linux: runtime=0.5348 ns | Windows: runtime=0.4132 ns
    public Double ComputingSum_IsGenerallyNotSlow()
    {
        return Vector.Sum(vectorFromHeapData);
    }

    // Assuming no regression, one would expect a runtime for vector sum of
    // Linux:   0.9431 ns + 0.5348 ns = 1.4779 << 7.7402 ns !
    // Windows: 0.9026 ns + 0.4132 ns = 1.3158 << 7.3555 ns !

    // However, the regression also happens with other Vector operations, such as the dot product

    [Benchmark] // Linux: runtime=8.7099 ns | Windows: runtime=8.1482 ns
    public Double Regression_IsPresentOnOtherVectorOperations()
    {
        // Dot product using stack allocated data is also slow, comparable to using Vector.Sum
        Span<Double> stackData = stackalloc Double[Vector<Double>.Count];
        var vectorFromStackData = new Vector<Double>(stackData);
        return Vector.Dot(vectorFromStackData, Vector<Double>.Zero);
    }

    [Benchmark] // Linux: runtime=1.2128 ns << 8.7099 ns | Windows: runtime=0.6454 ns << 8.1482 ns
    public Double Regression_IsNotPresentWithHeapAllocatedArray()
    {
        // The dot product on heap allocated data is ~ 7.2 times faster than on stack allocated data.
        // Hence, the regression is not limited to summation, where a regression ratio of 7.1 is shown.
        var vectorFromHeapData = new Vector<Double>(heapAllocatedData);
        return Vector.Dot(vectorFromHeapData, Vector<Double>.Zero);
    }
}

/*** Linux | Vector<Double>.Count = 4 ***
// * Summary *

BenchmarkDotNet=v0.13.3, OS=arch 
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK=7.0.101
  [Host]     : .NET 7.0.1 (7.0.122.61501), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.1 (7.0.122.61501), X64 RyuJIT AVX2


|                                        Method |      Mean |     Error |    StdDev |
|---------------------------------------------- |----------:|----------:|----------:|
|                    CopySpanToVector_IsNotSlow | 0.9431 ns | 0.0215 ns | 0.0201 ns |
|               ComputingSum_IsGenerallyNotSlow | 0.5348 ns | 0.0065 ns | 0.0058 ns |
|   Regression_IsPresentOnOtherVectorOperations | 8.7099 ns | 0.0433 ns | 0.0405 ns |
| Regression_IsNotPresentWithHeapAllocatedArray | 1.2128 ns | 0.0088 ns | 0.0083 ns |
*/

/*** Windows | Vector<Double>.Count = 4 ***
 // * Summary *

BenchmarkDotNet=v0.13.3, OS=Windows 10 (10.0.19044.2251/21H2/November2021Update)
Intel Core i7-10610U CPU 1.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=7.0.101
  [Host]     : .NET 7.0.1 (7.0.122.56804), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.1 (7.0.122.56804), X64 RyuJIT AVX2


|                                        Method |      Mean |     Error |    StdDev |
|---------------------------------------------- |----------:|----------:|----------:|
|                    CopySpanToVector_IsNotSlow | 0.9026 ns | 0.0190 ns | 0.0186 ns |
|               ComputingSum_IsGenerallyNotSlow | 0.4132 ns | 0.0194 ns | 0.0162 ns |
|   Regression_IsPresentOnOtherVectorOperations | 8.1482 ns | 0.1840 ns | 0.1721 ns |
| Regression_IsNotPresentWithHeapAllocatedArray | 0.6454 ns | 0.0109 ns | 0.0091 ns |
*/