using AbbaFleet.Infrastructure.Data;
using Xunit;

namespace AbbaFleet.Unit.Tests.Domain;

public class ApplicationUserTests
{
    [Fact]
    public void NewUser_IsActiveByDefault()
    {
        var user = new ApplicationUser();
        Assert.True(user.IsActive);
    }
}
