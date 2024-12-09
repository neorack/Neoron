using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Neoron.API.Tests.Helpers;

public static class HttpTestUtils
{
    public static async Task<T?> GetJsonAsync<T>(this HttpClient client, string url, HttpStatusCode expectedStatus = HttpStatusCode.OK)
    {
        var response = await client.GetAsync(url);
        response.StatusCode.Should().Be(expectedStatus);
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public static async Task<HttpResponseMessage> PostJsonAsync<T>(this HttpClient client, string url, T data, HttpStatusCode expectedStatus = HttpStatusCode.Created)
    {
        var response = await client.PostAsJsonAsync(url, data);
        response.StatusCode.Should().Be(expectedStatus);
        return response;
    }

    public static async Task<HttpResponseMessage> PutJsonAsync<T>(this HttpClient client, string url, T data, HttpStatusCode expectedStatus = HttpStatusCode.OK)
    {
        var response = await client.PutAsJsonAsync(url, data);
        response.StatusCode.Should().Be(expectedStatus);
        return response;
    }

    public static async Task<HttpResponseMessage> DeleteAsync(this HttpClient client, string url, HttpStatusCode expectedStatus = HttpStatusCode.NoContent)
    {
        var response = await client.DeleteAsync(url);
        response.StatusCode.Should().Be(expectedStatus);
        return response;
    }

    public static void AddTestAuthHeader(this HttpClient client, string userId)
    {
        client.DefaultRequestHeaders.Add("X-User-Id", userId);
    }

    public static void RemoveTestAuthHeader(this HttpClient client)
    {
        client.DefaultRequestHeaders.Remove("X-User-Id");
    }
}
