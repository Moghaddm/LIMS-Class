using LIMS.Domain.Entities;

namespace LIMS.Domain.Entity;

public sealed class User : BaseEntity
{
    public string FullName { get; private set; }
    public string Alias { get; private set; }
    public UserRole Role { get; private set; }
    public IEnumerable<Meeting> Sessions { get; }

    public User(string fullName, string alias, UserRole role) =>
        (FullName, Alias, Role) = (fullName, alias, role);

    private User() { }
}
