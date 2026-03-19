using AbbaFleet.Infrastructure.Data;
using Xunit;

namespace AbbaFleet.UnitTests.Domain;

public class ApplicationUserTests
{
    [Fact]
    public void NewUser_IsActiveByDefault()
    {
        var user = new ApplicationUser();
        Assert.True(user.IsActive);
    }
}
