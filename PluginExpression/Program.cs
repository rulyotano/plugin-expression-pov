// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using PluginExpression.Benchmarks;

Console.WriteLine("Running benchmark!");

var summary = BenchmarkRunner.Run<SimpleBenchmark>();