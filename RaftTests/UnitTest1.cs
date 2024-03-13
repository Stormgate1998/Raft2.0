using GateWay.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace RaftTests
{
    public class Tests
    {
        public List<string> nodes { get; set; }
        public Gateway gateway { get; set; }

        private static void Test3AtOnce(string one, string two, string three)
        {
            Assert.Multiple(() =>
            {
                Assert.That(one, Is.EqualTo("1"));
                Assert.That(two, Is.EqualTo("2"));
                Assert.That(three, Is.EqualTo("3"));
            });
        }

        [SetUp]
        public void Setup()
        {
            var loggerMock = new Mock<ILogger<Gateway>>();
            ILogger<Gateway> logger = loggerMock.Object;

            nodes = ["1", "2", "3"];
            gateway = new Gateway(nodes, logger, true);
        }

        [Test]
        public async Task Test1CallWorks()
        {
            string shouldBe1 = await gateway.ReturnIdOfNodeAsync("1");
            string shouldBe2 = await gateway.ReturnIdOfNodeAsync("2");
            string shouldBe3 = await gateway.ReturnIdOfNodeAsync("3");

            Test3AtOnce(shouldBe1, shouldBe2, shouldBe3);


        }

        [Test]
        public async Task Test1CallToAnotherWorks()
        {
            string shouldBe1 = await gateway.ReturnIdFromSecondNode("2", "1");
            string shouldBe2 = await gateway.ReturnIdFromSecondNode("3", "2");
            string shouldBe3 = await gateway.ReturnIdFromSecondNode("1", "3");
            Test3AtOnce(shouldBe1, shouldBe2, shouldBe3);
        }

        [Test]
        public void TestReturnList()
        {
            var list = gateway.ReturnList();
            List<string> TestList = ["1", "2", "3"];
            Assert.That(list, Is.EqualTo(TestList));
        }
    }
}
