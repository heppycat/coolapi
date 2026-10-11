using Core.Entities;

namespace Application.Interfaces;

public interface IPasswordService
{
    string Hash(User user, string password);
    bool Verify(User user, string password);
}
