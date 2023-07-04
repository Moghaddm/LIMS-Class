using LIMS.Domain.Entities;

namespace LIMS.Domain.Entity;

public class MemberShip : BaseEntity
{
    public Meeting Meeting { get; private set; }
    public User User { get; private set; }
    public DateTime JoinedDateTime { get; private set; }
    public bool UserRejected { get; private set; }
    public bool UserExited { get; private set; }
    public async Task BanUser()
        => Task.FromResult(UserRejected = true);

    public MemberShip(Meeting meeting, User user) => (Meeting, User) = (meeting, user);
}
