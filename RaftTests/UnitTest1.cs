using GateWay.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace RaftTests
{
    public class Tests
    {
        public Dictionary<int, string> nodes { get; set; }
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

            string list = "1=http://josh-test-node-1:8080;2=http://josh-test-node-2:8080;3=http://josh-test-node-3:8080";

            string[] pairs = list.Split(';');

            // Dictionary to store key-value pairs
            Dictionary<int, string> keyValuePairs = new Dictionary<int, string>();

            foreach (string pair in pairs)
            {
                // Split each pair by equal sign to separate key and value
                string[] parts = pair.Split('=');
                if (parts.Length == 2)
                {
                    // Parse key and add to dictionary
                    int key;
                    if (int.TryParse(parts[0], out key))
                    {
                        // Add key-value pair to dictionary
                        keyValuePairs.Add(key, parts[1]);
                    }
                }
            }
            nodes = keyValuePairs;
            gateway = new Gateway(nodes, logger);
        }

        [Test]
        public async Task Test1CallWorks()
        {
            string shouldBe1 = await gateway.ReturnIdOfNodeAsync("1");
            string shouldBe2 = await gateway.ReturnIdOfNodeAsync("2");
            string shouldBe3 = await gateway.ReturnIdOfNodeAsync("3");

            Test3AtOnce(shouldBe1, shouldBe2, shouldBe3);


        }

        // [Test]
        // public async Task Test1CallToAnotherWorks()
        // {
        //     string shouldBe1 = await gateway.ReturnIdFromSecondNode("2", "1");
        //     string shouldBe2 = await gateway.ReturnIdFromSecondNode("3", "2");
        //     string shouldBe3 = await gateway.ReturnIdFromSecondNode("1", "3");
        //     Console.WriteLine(shouldBe1);
        //     Console.WriteLine(shouldBe2);
        //     Console.WriteLine(shouldBe3);
        //     Test3AtOnce(shouldBe1, shouldBe2, shouldBe3);
        // }

        // [Test]
        // public void TestReturnList()
        // {
        //     var list = gateway.ReturnList();
        //     var TestList = nodes;
        //     Assert.That(list, Is.EqualTo(TestList));
        // }
    }
}
