using System.Net;
using System.Net.Http.Json;
using Parcel.NET.Abstractions.Exceptions;
using Parcel.NET.Dhl.Internetmarke.Models;
using Shouldly;
using Xunit;

namespace Parcel.NET.Dhl.Internetmarke.Tests;

public class DhlInternetmarkeClientTests
{
    private static DhlInternetmarkeClient CreateClient(HttpResponseMessage response) =>
        new(new HttpClient(new MockHttpMessageHandler(response))
        {
            BaseAddress = new Uri("https://api-eu.dhl.com/post/de/shipping/im/v1/")
        });

    [Fact]
    public async Task GetUserInfoAsync_ReturnsUserInfo()
    {
        var responseBody = new { displayName = "Max Mustermann", email = "max@test.de", walletBalance = 5000 };
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.GetUserInfoAsync();

        result.DisplayName.ShouldBe("Max Mustermann");
        result.Email.ShouldBe("max@test.de");
        result.WalletBalanceCents.ShouldBe(5000);
    }

    [Fact]
    public async Task GetCatalogAsync_ReturnsCatalogItems()
    {
        var responseBody = new
        {
            products = new[]
            {
                new { id = "1", name = "Standardbrief", price = 85, type = "LETTER", annotation = "bis 20g", weightLimit = 20 },
                new { id = "2", name = "Kompaktbrief", price = 100, type = "LETTER", annotation = "bis 50g", weightLimit = 50 }
            }
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.GetCatalogAsync();

        result.Count.ShouldBe(2);
        result[0].Name.ShouldBe("Standardbrief");
        result[0].PriceCents.ShouldBe(85);
        result[1].WeightLimitGrams.ShouldBe(50);
    }

    [Fact]
    public async Task InitializeCartAsync_ReturnsCartId()
    {
        var responseBody = new { cartId = "CART-001", total = 170 };
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var request = new CartRequest
        {
            Items = [new CartItem { ProductId = "1", Quantity = 2 }]
        };

        var result = await client.InitializeCartAsync(request);

        result.CartId.ShouldBe("CART-001");
        result.TotalCents.ShouldBe(170);
    }

    [Fact]
    public async Task CheckoutCartAsync_ReturnsCheckoutResult()
    {
        var responseBody = new
        {
            orderId = "ORD-001",
            label = Convert.ToBase64String("fake-pdf"u8.ToArray()),
            total = 170,
            remainingBalance = 4830
        };

        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.CheckoutCartAsync("CART-001");

        result.OrderId.ShouldBe("ORD-001");
        result.LabelPdf.ShouldNotBeNull();
        result.TotalCents.ShouldBe(170);
        result.RemainingBalanceCents.ShouldBe(4830);
    }

    [Fact]
    public async Task GetWalletBalanceAsync_ReturnsBalance()
    {
        var responseBody = new { balance = 4830 };
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseBody)
        });

        var result = await client.GetWalletBalanceAsync();
        result.BalanceCents.ShouldBe(4830);
    }

    [Fact]
    public async Task GetUserInfoAsync_ApiError_ThrowsParcelException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("""{"detail":"Invalid credentials."}""", System.Text.Encoding.UTF8, "application/json")
        });

        var ex = await Should.ThrowAsync<ParcelException>(() => client.GetUserInfoAsync());
        ex.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task InitializeCartAsync_NullRequest_ThrowsArgumentNull()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));
        await Should.ThrowAsync<ArgumentNullException>(() => client.InitializeCartAsync(null!));
    }

    [Fact]
    public async Task CheckoutCartAsync_EmptyCartId_ThrowsArgumentException()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK));
        await Should.ThrowAsync<ArgumentException>(() => client.CheckoutCartAsync(""));
    }
}

internal class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;

    public MockHttpMessageHandler(HttpResponseMessage response)
    {
        _response = response;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_response);
    }
}
