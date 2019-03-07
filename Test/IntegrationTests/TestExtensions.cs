using KellermanSoftware.CompareNetObjects;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Service.IntegrationTests
{
    public static class TestExtensions
    {
        public static void AssertEquivalentTo(this object actual, object expected)
        {
            var actualIEnum = actual as IEnumerable<object>;
            var expectedIEnum = expected as IEnumerable<object>;

            if (actualIEnum != null && expectedIEnum != null)
            {
                actual = actualIEnum.ToArray();
                expected = expectedIEnum.ToArray();
            }
            
            var compareLogic = new CompareLogic(new ComparisonConfig
            {
                MaxDifferences = 20
            });
            var compareResult = compareLogic.Compare(expected, actual);
            Assert.True(compareResult.AreEqual, compareResult.DifferencesString);
        }
    }
}
