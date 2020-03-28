using NUnit.Framework;

namespace NUnitTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            Evaluation.LogEvent = log => TestContext.WriteLine(log);
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}