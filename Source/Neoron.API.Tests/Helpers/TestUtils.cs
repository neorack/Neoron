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
}
