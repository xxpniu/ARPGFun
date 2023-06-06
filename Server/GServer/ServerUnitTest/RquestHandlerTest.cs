using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Grpc.Core;

namespace ServerUnitTest
{
    public class RquestHandlerTest
    {
        public RquestHandlerTest(ITestOutputHelper oupt)
        {
            this.output = oupt;
        }
        readonly ITestOutputHelper output;


        [Theory]//InlineData
        [InlineData("127.0.0.1:1900")]
        public async Task StreamTestAsync(string server)
        {
            output.WriteLine(server);
            await Task.Delay(0);
        }
    }
}
