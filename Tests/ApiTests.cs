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
        _client = new HttpClient { BaseAddress = new Uri(UrlService.BASE_URL) };
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
    public async Task TestValidRequestReturns200AndCorrectResponse()
    {
        var requestBody = new ApiRequest { Id = 1, Name = "test" };
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
    public async Task TestInvalidContentTypeHeaderReturnsUnsupportedMediaType()
    {
        var requestBody = new ApiRequest { Id = 1, Name = "test" };
        var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "text/plain");

        var response = await _client.PostAsync(UrlService.CLIENT_ENDPOINT, content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnsupportedMediaType) );
    }
    [Test]
    public async Task TestMissingRequiredFieldsInBodyReturnsBadRequest()
    {
        var requestBody = new ApiRequest { Name = "test" }; 
        var content = GetRequestBodyContent(requestBody);

        var response = await _client.PostAsync(UrlService.CLIENT_ENDPOINT, content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest) );
    }
    [Test]
    public async Task TestIncorrectDataTypeForFieldsReturnsBadRequest()
    {
        var requestBody = new  { Id = "one", Name = 123 };
        var content = GetRequestBodyContent(requestBody);

        var response = await _client.PostAsync(UrlService.CLIENT_ENDPOINT, content);

        Assert.That( response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
    [Test]
    public async Task TestEmptyRequestBodyReturnsBadRequest()
    {
        var requestBody = new ApiRequest { };
        var content = GetRequestBodyContent(requestBody);

        var response = await _client.PostAsync(UrlService.CLIENT_ENDPOINT, content);

        Assert.That( response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
    [Test]
    public async Task TestLargeNameReturnsRequestIstooLarge()
    {
        var requestBody = new ApiRequest { Id = 1, Name = new string('a', 1000000) };
        var content = GetRequestBodyContent(requestBody);

        var response = await _client.PostAsync(UrlService.CLIENT_ENDPOINT, content);

        Assert.That( response.StatusCode, Is.EqualTo(HttpStatusCode.RequestHeaderFieldsTooLarge));
    }
    [Test]
    public async Task TestIdOutOfBoundsReturnsBadRequest()
    {
        var requestBody = new { Id = 123456789012345678, Name = "test" }; 
        var content = GetRequestBodyContent(requestBody);

        var response = await _client.PostAsync(UrlService.CLIENT_ENDPOINT, content);

        Assert.That( response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}