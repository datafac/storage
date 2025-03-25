using Shouldly;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace DTOMaker.Gentime.Tests
{
    public class RoundtripDTOTests
    {
        [Fact]
        public async Task Roundtrip0_SimpleDTO()
        {
            Person orig = new Person()
            {
                Id = 123,
                Surname = "Citizen",
                Nickname = null,
                YOB = 1971,
                OtherNames = ["Alan", "Beaufort"]
            };

            string buffer = orig.EmitText();

            await Verifier.Verify(buffer);

            Person copy = new Person();

            copy.LoadFrom(buffer);
            copy.ShouldBe(orig);

        }

        [Fact]
        public async Task Roundtrip1_NestedObject()
        {
            Person p1 = new Person()
            {
                Id = 123,
                Surname = "Citizen",
                Nickname = null,
                YOB = 1971,
                OtherNames = ["Alan", "Beaufort"]
            };

            Family orig = new Family()
            {
                Leader = p1,
            };

            string buffer = orig.EmitText();

            await Verifier.Verify(buffer);

            Family copy = new Family();

            copy.LoadFrom(buffer);
            copy.ShouldBe(orig);

            Person? p2 = copy.Leader;
            p2.ShouldNotBeNull();
            p2.ShouldBe(p1);

        }

        [Fact]
        public async Task Roundtrip2_NestedVector()
        {
            Person origP1 = new Person()
            {
                Id = 123,
                Surname = "Citizen",
                Nickname = null,
                YOB = 1971,
                OtherNames = ["Alan", "Beaufort"]
            };

            Person origP2 = new Person()
            {
                Id = 234,
                Surname = "Citizen",
                Nickname = null,
                YOB = 1971,
                OtherNames = ["Alice", "Belle"]
            };

            Family orig = new Family()
            {
                Leader = origP1,
                Members = [origP1, origP2]
            };

            string buffer = orig.EmitText();

            await Verifier.Verify(buffer);

            Family copy = new Family();

            copy.LoadFrom(buffer);
            copy.ShouldBe(orig);

            Person? copyP1 = copy.Leader;
            copyP1.ShouldNotBeNull();
            copyP1.ShouldBe(origP1);

        }

    }
}