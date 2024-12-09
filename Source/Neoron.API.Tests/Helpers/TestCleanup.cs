using Neoron.API.Data;

namespace Neoron.API.Tests.Helpers;

public static class TestCleanup
{
    public static async Task ClearDatabase(ApplicationDbContext context)
    {
        context.Messages.RemoveRange(context.Messages);
        await context.SaveChangesAsync();
    }
    
    public static async Task ResetTestData(ApplicationDbContext context)
    {
        await ClearDatabase(context);
        // Add baseline test data here if needed
    }
}
