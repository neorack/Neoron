using System.Net;

namespace Neoron.API.Tests.Helpers;

public static class TestUtils
{
    public static string GenerateRandomString(int length)
    {
        return new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", length)
            .Select(s => s[Random.Shared.Next(s.Length)])
            .ToArray());
    }

    public static async Task WaitUntilAsync(Func<Task<bool>> condition, TimeSpan timeout)
    {
        var startTime = DateTime.UtcNow;
        while (DateTime.UtcNow - startTime < timeout)
        {
            if (await condition())
            {
                return;
            }
            await Task.Delay(100);
        }
        throw new TimeoutException($"Condition not met within timeout of {timeout.TotalSeconds} seconds");
    }

    public static async Task RetryAsync(Func<Task> action, int maxAttempts = 3)
    {
        Exception? lastException = null;
        for (int i = 0; i < maxAttempts; i++)
        {
            try
            {
                await action();
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                await Task.Delay((i + 1) * 1000);
            }
        }
        throw new Exception($"Action failed after {maxAttempts} attempts", lastException);
    }

    public static async Task<T> RetryWithResultAsync<T>(Func<Task<T>> action, int maxAttempts = 3)
    {
        Exception? lastException = null;
        for (int i = 0; i < maxAttempts; i++)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                lastException = ex;
                await Task.Delay((i + 1) * 1000);
            }
        }
        throw new Exception($"Action failed after {maxAttempts} attempts", lastException);
    }

    public static async Task AssertEventuallyAsync(
        Func<Task<bool>> condition,
        string message = "Condition not met",
        int timeoutSeconds = 5,
        int pollIntervalMs = 100)
    {
        var startTime = DateTime.UtcNow;
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);

        while (DateTime.UtcNow - startTime < timeout)
        {
            if (await condition())
            {
                return;
            }
            await Task.Delay(pollIntervalMs);
        }
        throw new TimeoutException($"{message} within {timeoutSeconds} seconds");
    }

    public static string GenerateRandomEmail()
    {
        return $"test.{Guid.NewGuid()}@example.com";
    }

    public static long GenerateRandomId()
    {
        return Random.Shared.NextInt64(1, long.MaxValue);
    }

    public static async Task WithTimeout(Func<Task> action, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        try
        {
            await action().WaitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException($"Operation timed out after {timeout.TotalSeconds} seconds");
        }
    }

    public static async Task<HttpResponseMessage> WaitForSuccessfulResponse(
        Func<Task<HttpResponseMessage>> request,
        int maxAttempts = 3)
    {
        return await RetryWithResultAsync(async () =>
        {
            var response = await request();
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed with status {response.StatusCode}");
            }
            return response;
        }, maxAttempts);
    }

    public static async Task<T> AssertDbChangeAsync<T>(
        ApplicationDbContext context,
        Func<Task<T>> action,
        Func<T, Task> verification)
    {
        var result = await action();
        await verification(result);
        await context.SaveChangesAsync();
        return result;
    }

    public static async Task AssertThrowsAsync<TException>(
        Func<Task> action,
        string expectedMessage = null) where TException : Exception
    {
        var exception = await Assert.ThrowsAsync<TException>(action);
        if (expectedMessage != null)
        {
            Assert.Contains(expectedMessage, exception.Message);
        }
    }

    public static class TestDataSeeder
    {
        public static async Task SeedTestMessages(ApplicationDbContext context, int count = 10)
        {
            var messages = Enumerable.Range(1, count)
                .Select(i => new DiscordMessageBuilder()
                    .WithMessageId(GenerateRandomId())
                    .WithContent($"Test Message {i}")
                    .WithCreatedAt(DateTimeOffset.UtcNow.AddMinutes(-i))
                    .Build())
                .ToList();

            await context.DiscordMessages.AddRangeAsync(messages);
            await context.SaveChangesAsync();
        }

        public static async Task SeedTestThreads(ApplicationDbContext context, int threadCount = 3, int messagesPerThread = 5)
        {
            for (int i = 0; i < threadCount; i++)
            {
                var parentMsg = new DiscordMessageBuilder()
                    .WithMessageId(GenerateRandomId())
                    .WithContent($"Thread Parent {i}")
                    .Build();

                await context.DiscordMessages.AddAsync(parentMsg);
                await context.SaveChangesAsync();

                var threadMessages = Enumerable.Range(1, messagesPerThread)
                    .Select(j => new DiscordMessageBuilder()
                        .WithMessageId(GenerateRandomId())
                        .WithContent($"Thread Reply {j}")
                        .InThread(parentMsg.MessageId)
                        .Build())
                    .ToList();

                await context.DiscordMessages.AddRangeAsync(threadMessages);
                await context.SaveChangesAsync();
            }
        }
    }

    public static class HttpClientExtensions
    {
        public static async Task<T> GetAndDeserializeAsync<T>(
            this HttpClient client, 
            string requestUri,
            HttpStatusCode expectedStatus = HttpStatusCode.OK)
        {
            var response = await client.GetAsync(requestUri);
            response.StatusCode.Should().Be(expectedStatus);
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public static async Task<HttpResponseMessage> PostAndVerifyAsync<T>(
            this HttpClient client,
            string requestUri,
            T content,
            HttpStatusCode expectedStatus = HttpStatusCode.Created)
        {
            var response = await client.PostAsJsonAsync(requestUri, content);
            response.StatusCode.Should().Be(expectedStatus);
            return response;
        }
    }
}
