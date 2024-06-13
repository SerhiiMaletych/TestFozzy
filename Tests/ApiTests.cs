using System.Net;
using System.Text;
using Newtonsoft.Json;
using TestFozzy.Models;
using TestFozzy.Services;

namespace TestFozzy.Tests;

[TestFixture]
public class ApiTests
{
    private HttpClient _client;

    [SetUp] 
    public void Setup()
    {
        _client = new HttpClient { BaseAddress = new System.Uri("https://test.lan") };
    }
    [TearDown]
    public void Teardown()
    {
        _client.Dispose();
    }
    private StringContent GetRequestBodyContent(object requestBody)
    {
        return new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
    }

    [Test]
    public async Task TestValidRequest()
    {
        var requestBody = new ApiRequest() { Id = 1, Name = "test" };
        var content = GetRequestBodyContent(requestBody);

        var response = await _client.PostAsync(UrlService.CLIENT_ENDPOINT, content);

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var responseObject = JsonConvert.DeserializeObject<ApiResponse>(responseBody);

        Assert.That(responseObject.Name, Is.EqualTo("test"));
        Assert.That(responseObject.Age, Is.EqualTo(1));
        Assert.That(responseObject.Adi, Is.EqualTo("addition_info"));
    }
    
    [Test]
    public async Task TestInvalidContentType()
    {
        var requestBody = new ApiRequest() { Id = 1, Name = "test" };
        var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "text/plain");

        var response = await _client.PostAsync("/client", content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnsupportedMediaType) );
    }
}