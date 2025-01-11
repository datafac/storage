using FluentAssertions;
using System;
using Xunit;

namespace DTOMaker.CSPoco.Tests
{
    public class VersionTests
    {
        //[Fact]
        //public void ModelsVersionChecks()
        //{
        //    Version coreVersion = typeof(DTOMaker.Models.EntityAttribute).Assembly.GetName().Version ?? new Version(99, 99, 9999);
        //    Version thisVersion = typeof(DTOMaker.Models.CSPoco.xxxxxxAttribute).Assembly.GetName().Version ?? new Version(0, 0, 0);

        //    thisVersion.Major.Should().Be(coreVersion.Major);
        //    thisVersion.Minor.Should().BeGreaterThanOrEqualTo(coreVersion.Minor);
        //}

        [Fact]
        public void SrcGenVersionChecks()
        {
            Version coreVersion = typeof(DTOMaker.Gentime.SourceGeneratorBase).Assembly.GetName().Version ?? new Version(99, 99, 9999);
            Version thisVersion = typeof(DTOMaker.CSPoco.CSPocoSourceGenerator).Assembly.GetName().Version ?? new Version(0, 0, 0);

            thisVersion.Major.Should().Be(coreVersion.Major);
            thisVersion.Minor.Should().BeGreaterThanOrEqualTo(coreVersion.Minor);
        }

        [Fact]
        public void RuntimeVersionChecks()
        {
            Version coreVersion = typeof(DTOMaker.Runtime.IMutability).Assembly.GetName().Version ?? new Version(99, 99, 9999);
            Version thisVersion = typeof(DTOMaker.Runtime.CSPoco.EntityBase).Assembly.GetName().Version ?? new Version(0, 0, 0);

            thisVersion.Major.Should().Be(coreVersion.Major);
            thisVersion.Minor.Should().BeGreaterThanOrEqualTo(coreVersion.Minor);
        }

    }
}