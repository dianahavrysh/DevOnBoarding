using Common.Database;

namespace Common.Interfaces
{
    /// <summary>
    /// Factory interface to create Database instances.
    /// </summary>
    public interface IDatabaseFactory
    {
        Common.Database.Database CreateDatabase();
    }
}
