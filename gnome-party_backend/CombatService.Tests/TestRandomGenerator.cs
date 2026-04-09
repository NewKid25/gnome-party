using System;
using System.Collections.Generic;
using System.Text;
using Models.TestHelperData;

namespace CombatService.Tests
{
    public sealed class TestRandomGenerator : IRandomGenerator
    {
        private readonly Queue<double> values; // Queue to hold predetermined random values for testing
        public TestRandomGenerator(params double[] values) { this.values = new Queue<double>(values); } // Constructor that takes a variable number of double values and initializes the queue
        public double NextDouble() { if (values.Count == 0) { throw new InvalidOperationException("No more values in test queue"); } return values.Dequeue(); }
    } 
}
