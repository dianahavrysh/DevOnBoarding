using Common.Database;
using Common.Interfaces;
using System;
using System.Data;

namespace BusinessLogic
{
    public abstract class BaseDbManager
    {
        protected readonly Database Db;

        protected BaseDbManager(IDatabaseFactory factory)
        {
            Db = factory.CreateDatabase();
        }

        protected IDataParameter Param(string name, object? value)
        {
            return Db.CreateParameter(name, value ?? DBNull.Value);
        }
    }
}
