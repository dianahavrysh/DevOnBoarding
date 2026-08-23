using Common;

namespace Common.Interfaces
{
    /// <summary>
    /// Factory interface to create Database instances.
    /// </summary>
    public interface IDatabaseFactory
    {
        Database CreateDatabase();
    }
}
