using System;
using System.Collections.Generic;
using Models.TestHelperData;

namespace CombatService.Tests
{
    public sealed class TestRandomGenerator : IRandomGenerator
    {
        private readonly Queue<double> values;
        private readonly Action<string> log;

        public TestRandomGenerator(params double[] values) : this(null, values) { }

        public TestRandomGenerator(Action<string> log, params double[] values)
        {
            this.values = new Queue<double>(values);
            this.log = log;
        }

        public double NextDouble()
        {
            if (values.Count == 0)
            {
                if (log != null)
                {
                    log("TestRandomGenerator.NextDouble() -> queue empty");
                }

                throw new InvalidOperationException("No more values in test queue");
            }

            var value = values.Dequeue();

            if (log != null)
            {
                log($"TestRandomGenerator.NextDouble() -> {value}");
            }

            return value;
        }
    }
}