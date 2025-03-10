using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace DTOMaker.CSRecord.Tests
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