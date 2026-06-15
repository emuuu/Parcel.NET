using Parcel.NET.Dhl;
using Shouldly;
using Xunit;

namespace Parcel.NET.Dhl.EPost.Tests;

public class DhlOptionsEPostTests
{
    [Fact]
    public void EPostBaseUrl_DefaultsToDocuguideHostWithTrailingSlash()
    {
        new DhlOptions { ApiKey = "x" }.EPostBaseUrl
            .ShouldBe("https://api.epost.docuguide.com/");
    }

    [Theory]
    [InlineData("https://host/epost", "https://host/epost/")]
    [InlineData("https://host/epost/", "https://host/epost/")]
    public void EPostBaseUrl_EnforcesTrailingSlash(string custom, string expected)
    {
        new DhlOptions { ApiKey = "x", CustomEPostBaseUrl = custom }.EPostBaseUrl
            .ShouldBe(expected);
    }
}
