// <copyright file="Program.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using BenchmarkDotNet.Running;

namespace Shmuelie.Zio.Windows.Benchmark
{
    public static class Program
    {
        public static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
