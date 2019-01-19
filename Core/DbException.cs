using System;

namespace Core
{
    public class DbException : Exception
    {
        public DbException(Exception e, string query) : base(e.Message, e)
        {
            Query = query;
        }

        public string Query { get; }
    }
}
