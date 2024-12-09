using System.Linq;

public class DiscordMessageRepository
{
    public IQueryable<DiscordMessage> GetMessages()
    {
        return _context.DiscordMessages.AsNoTracking();
    }

    // Optimize other queries as needed
}
