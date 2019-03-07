using Core.Models;

namespace Core
{
    public interface IConnectionFactory
    {
        IUserConnection MakeConnection(UserConfiguration user);
    }
}