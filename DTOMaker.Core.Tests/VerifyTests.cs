using System.Threading.Tasks;
using VerifyXunit;

namespace DTOMaker.Runtime.Tests
{
    public class VerifyTests
    {
        [Fact]
        public async Task RunVerifyChecks()
        {
            await VerifyChecks.Run();
        }
    }
}
