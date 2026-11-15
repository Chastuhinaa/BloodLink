using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BloodLink.IntegrationTests;

public class ApiTests : IClassFixture<BloodLinkApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly BloodLinkApiFactory _factory;

    public ApiTests(BloodLinkApiFactory factory) => _factory = factory;

    private async Task<HttpClient> AuthorizedClientAsync(string? email = null)
    {
        var client = _factory.CreateClient();
        email ??= $"donor-{Guid.NewGuid():N}@test.ua";
        var resp = await client.PostAsJsonAsync("/api/auth/register",
            new { email, password = "Passw0rd!", fullName = "Тестовий Донор" });
        resp.EnsureSuccessStatusCode();
        var auth = await resp.Content.ReadFromJsonAsync<JsonElement>(Json);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.GetProperty("token").GetString());
        return client;
    }

    [Fact]
    public async Task DemoScenario_Register_Profile_Eligibility_Centers_History()
    {
        var client = await AuthorizedClientAsync();

        var put = await client.PutAsJsonAsync("/api/profile", new
        {
            fullName = "Олена Коваль",
            sex = "Female",
            birthDate = "2003-05-10",
            bloodGroup = "A",
            rhFactor = "Negative",
            city = "Київ",
            latitude = 50.4501,
            longitude = 30.5234,
        });
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);
        var profile = await put.Content.ReadFromJsonAsync<JsonElement>(Json);
        Assert.Equal("A(II) Rh−", profile.GetProperty("bloodType").GetString());

        var elig = await client.GetFromJsonAsync<JsonElement>("/api/eligibility?type=WholeBlood", Json);
        Assert.True(elig.GetProperty("canDonateNow").GetBoolean());

        var centers = await client.GetFromJsonAsync<JsonElement>("/api/centers?lat=50.4501&lng=30.5234", Json);
        Assert.Equal(5, centers.GetArrayLength());
        var nearest = centers[0];
        Assert.True(nearest.GetProperty("distanceKm").GetDouble() <= centers[1].GetProperty("distanceKm").GetDouble());

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var post = await client.PostAsJsonAsync("/api/donations", new
        {
            date = today.ToString("yyyy-MM-dd"),
            type = "WholeBlood",
            bloodCenterId = nearest.GetProperty("id").GetInt32(),
        });
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);

        var history = await client.GetFromJsonAsync<JsonElement>("/api/donations", Json);
        Assert.Equal(1, history.GetArrayLength());

        elig = await client.GetFromJsonAsync<JsonElement>("/api/eligibility?type=WholeBlood", Json);
        Assert.False(elig.GetProperty("canDonateNow").GetBoolean());
        Assert.Equal(90, elig.GetProperty("daysLeft").GetInt32());
    }

    [Theory]
    [InlineData("/api/profile")]
    [InlineData("/api/donations")]
    [InlineData("/api/eligibility")]
    public async Task ProtectedEndpoints_WithoutToken_Return401(string url)
    {
        var resp = await _factory.CreateClient().GetAsync(url);

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithForgedToken_Returns401()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not.a.valid.jwt");

        var resp = await client.GetAsync("/api/profile");

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var email = $"dup-{Guid.NewGuid():N}@test.ua";
        await AuthorizedClientAsync(email);

        var resp = await _factory.CreateClient().PostAsJsonAsync("/api/auth/register",
            new { email, password = "Passw0rd!", fullName = "Друга" });

        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        Assert.Equal("application/problem+json", resp.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Register_ShortPassword_Returns400()
    {
        var resp = await _factory.CreateClient().PostAsJsonAsync("/api/auth/register",
            new { email = "short@test.ua", password = "123", fullName = "Донор" });

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Login_AfterRegister_Returns200_AndWrongPassword401()
    {
        var email = $"login-{Guid.NewGuid():N}@test.ua";
        await AuthorizedClientAsync(email);
        var client = _factory.CreateClient();

        var ok = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Passw0rd!" });
        var bad = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Wrong1234" });

        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, bad.StatusCode);
    }

    [Fact]
    public async Task Profile_Underage_Returns400WithErrors()
    {
        var client = await AuthorizedClientAsync();

        var resp = await client.PutAsJsonAsync("/api/profile", new { fullName = "Юний", birthDate = "2012-01-01" });

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        var problem = await resp.Content.ReadFromJsonAsync<JsonElement>(Json);
        Assert.True(problem.GetProperty("errors").GetArrayLength() > 0);
    }

    [Fact]
    public async Task Donations_FutureDate_Returns400()
    {
        var client = await AuthorizedClientAsync();
        var future = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(10);

        var resp = await client.PostAsJsonAsync("/api/donations", new { date = future.ToString("yyyy-MM-dd"), type = "Plasma" });

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Donations_AreIsolatedBetweenDonors()
    {
        var alice = await AuthorizedClientAsync();
        var bob = await AuthorizedClientAsync();
        var created = await alice.PostAsJsonAsync("/api/donations", new { date = "2026-01-10", type = "WholeBlood" });
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>(Json)).GetProperty("id").GetInt32();

        var bobList = await bob.GetFromJsonAsync<JsonElement>("/api/donations", Json);
        var bobDelete = await bob.DeleteAsync($"/api/donations/{id}");
        var aliceDelete = await alice.DeleteAsync($"/api/donations/{id}");

        Assert.Equal(0, bobList.GetArrayLength());
        Assert.Equal(HttpStatusCode.NotFound, bobDelete.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, aliceDelete.StatusCode);
    }

    [Fact]
    public async Task Centers_OnlyLatitude_Returns400()
    {
        var resp = await _factory.CreateClient().GetAsync("/api/centers?lat=50.45");

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Swagger_DocumentDescribesAllEndpoints()
    {
        var doc = await _factory.CreateClient().GetFromJsonAsync<JsonElement>("/swagger/v1/swagger.json");
        var paths = doc.GetProperty("paths");

        foreach (var p in new[] { "/api/auth/register", "/api/auth/login", "/api/profile", "/api/eligibility", "/api/centers", "/api/donations", "/api/donations/{id}" })
        {
            Assert.True(paths.TryGetProperty(p, out _), $"Немає {p} у swagger.json");
        }

        Assert.True(doc.GetProperty("components").GetProperty("securitySchemes").TryGetProperty("Bearer", out _));

        Assert.True(paths.GetProperty("/api/profile").GetProperty("get").TryGetProperty("security", out _));
        Assert.False(paths.GetProperty("/api/centers").GetProperty("get").TryGetProperty("security", out _));
    }
}
