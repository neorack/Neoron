using Neoron.API.Data;

namespace Neoron.API.Tests.Helpers;

public static class TestCleanup
{
    public static async Task ClearDatabase(ApplicationDbContext context)
    {
        // Clear all relevant tables
        context.Messages.RemoveRange(context.Messages);
        // Add other tables as needed
        await context.SaveChangesAsync();
    }
    
    public static async Task ResetTestData(ApplicationDbContext context)
    {
        await ClearDatabase(context);
        
        // Add baseline test data
        var baselineMessage = TestDataBuilder.CreateTestMessage(
            messageId: 1,
            content: "Baseline test message",
            authorId: 1);
            
        await context.Messages.AddAsync(baselineMessage);
        await context.SaveChangesAsync();
    }

    public static async Task CleanupTestFiles(string directory)
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
