using System;
using LibroLib;
using LibroLib.FileSystem;
using LibroLib.Misc;
using Moq;
using NUnit.Framework;
using Syborg.Policies;

namespace Syborg.Tests.PoliciesTests
{
    public class HstsPolicyTests
    {
        [Test]
        public void DoesNothingIfHstsIsNotUsed()
        {
            config.UseHsts = false;

            ApplyPolicy();

            AssertNoHstsHeader();
        }

        [Test]
        public void ReplacesExistingHstsHeaderIfHstsIsUsed()
        {
            config.UseHsts = true;
            context.AddHeader(
                HttpConsts.HeaderStrictTransportSecurity,
                "asdasdas");

            ApplyPolicy();

            AssertHstsMaxAgeIsSet();
        }

        [Test]
        public void SetsSpecifiedMaxAgeWhenHstsIsUsed()
        {
            config.UseHsts = true;
            const int MaxAgeSeconds = 100;
            hstsPolicy = new HstsPolicy(TimeSpan.FromSeconds(MaxAgeSeconds));
            ApplyPolicy();
            AssertHstsMaxAgeIsSet(MaxAgeSeconds);
        }

        [SetUp]
        public void Setup()
        {
            config = new WebServerConfiguration();
            context = new FakeWebContext(
                null,
                null,
                Mock.Of<IFileSystem>(),
                new TimeMachine(),
                config);
            hstsPolicy = new HstsPolicy();
        }

        private void ApplyPolicy()
        {
            hstsPolicy.Apply(context);
        }

        private void AssertNoHstsHeader()
        {
            Assert.That(
                context.ResponseHeaders.Keys,
                Does.Not.Contain(HttpConsts.HeaderStrictTransportSecurity));
        }

        private void AssertHstsMaxAgeIsSet(int maxAgeSeconds = 31536000)
        {
            Assert.That(
                context.ResponseHeaders[HttpConsts.HeaderStrictTransportSecurity],
                Is.EqualTo("max-age={0}".Fmt(maxAgeSeconds)));
        }

        private FakeWebContext context;
        private WebServerConfiguration config;
        private HstsPolicy hstsPolicy;
    }
}