using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Tests.CharacterAITests.DifficultEnemyPoolAITests
{
    public class CaveBatAITests
    {
        [Theory]
        [InlineData(0.01, 6, 30, "Blood Peck")]
        [InlineData(0.5, 6, 30, "Blood Peck")]
        [InlineData(0.75, 9, 30, "Blood Peck")]
        public void BloodPeckOnLowHealthTargetAndCorrectRoll(double roll, int health, int maxHealth, string expectedAction)
        {

        }

        [Theory]
        [InlineData(0.90, 6, 30, "Sonic Squeal")]
        [InlineData(0.76, 9, 30, "Sonic Squeal")]
        [InlineData(0.99, 9, 30, "Sonic Squeal")]
        public void SonicSquealLowHealthTargetButFailedRoll(double rool, int health, int maxHealth, string expectedAction)
        {

        }
    }
}
