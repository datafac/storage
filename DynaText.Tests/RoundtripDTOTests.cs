using Shouldly;
using System.Threading.Tasks;
using VerifyXunit;

namespace DynaText.Tests
{
    public class RoundtripDTOTests
    {
        [Fact]
        public async Task Roundtrip0_SimpleDTO()
        {
            SimpleDTO orig = new SimpleDTO()
            {
                Id = 123,
                Surname = "Smith",
                Nickname = null,
            };

            string buffer = orig.EmitText();

            await Verifier.Verify(buffer);

            SimpleDTO copy = new SimpleDTO();

            copy.LoadFrom(buffer);
            copy.ShouldBe(orig);

        }

    }
}