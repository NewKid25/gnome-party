using System;
using System.Collections.Generic;
using System.Text;

namespace Models.TestHelperData
{
    public interface IRandomGenerator // Interface for providing random numbers, allows for easier testing and potential future extensions
    {
        double NextDouble();
    }
    public sealed class RandomNumGen : IRandomGenerator // Default implementation of IRandomProvider using System.Random
    {
        public double NextDouble()
        {
            return Random.Shared.NextDouble();
        }
    }
}
