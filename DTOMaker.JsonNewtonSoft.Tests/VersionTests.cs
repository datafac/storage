using Shouldly;
using System;
using Xunit;

namespace DTOMaker.JsonNewtonSoft.Tests
{
    public class VersionTests
    {
        //[Fact]
        //public void ModelsVersionChecks()
        //{
        //    Version coreVersion = typeof(DTOMaker.Models.EntityAttribute).Assembly.GetName().Version ?? new Version(99, 99, 9999);
        //    Version thisVersion = typeof(DTOMaker.Models.JsonNewtonSoft.xxxxxxAttribute).Assembly.GetName().Version ?? new Version(0, 0, 0);

        //    thisVersion.Major.ShouldBe(coreVersion.Major);
        //    thisVersion.Minor.ShouldBeGreaterThanOrEqualTo(coreVersion.Minor);
        //}

        [Fact]
        public void SrcGenVersionChecks()
        {
            Version coreVersion = typeof(DTOMaker.Gentime.SourceGeneratorBase).Assembly.GetName().Version ?? new Version(99, 99, 9999);
            Version thisVersion = typeof(DTOMaker.JsonNewtonSoft.JsonNSSourceGenerator).Assembly.GetName().Version ?? new Version(0, 0, 0);

            thisVersion.Major.ShouldBe(coreVersion.Major);
            thisVersion.Minor.ShouldBeGreaterThanOrEqualTo(coreVersion.Minor);
        }

        [Fact]
        public void RuntimeVersionChecks()
        {
            Version coreVersion = typeof(DTOMaker.Runtime.IMutability).Assembly.GetName().Version ?? new Version(99, 99, 9999);
            Version thisVersion = typeof(DTOMaker.Runtime.JsonNewtonSoft.EntityBase).Assembly.GetName().Version ?? new Version(0, 0, 0);

            thisVersion.Major.ShouldBe(coreVersion.Major);
            thisVersion.Minor.ShouldBeGreaterThanOrEqualTo(coreVersion.Minor);
        }

    }
}