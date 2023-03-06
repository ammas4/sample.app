using AppSample.Domain.Helpers;
using AppSample.Domain.Models;

namespace AppSample.Tests;

[TestClass]
public class ScopeHelperTest
{
    [TestMethod]
    public void SpHasMessageForRequestScopeTest()
    {
        var serviceProviderScopes = new List<Scope>()
        {
            new Scope() { ScopeName = ScopesHelper.AuthzScope, Message = "Test" },
            new Scope() { ScopeName = ScopesHelper.AtpScope, Message = "Test1" },
        };
        var requestScopes = new List<string>() { ScopesHelper.AtpScope, ScopesHelper.AuthzScope, ScopesHelper.IdentityFullScope, ScopesHelper.IdentityBasicAddressScope };
        var isCustomMessageRequired = serviceProviderScopes.IsCustomMessageRequired(requestScopes, out var message);

        Assert.IsTrue(isCustomMessageRequired);
        Assert.AreEqual(message, "Test");
    }

    [TestMethod]
    public void SpHasNotMessageForRequestScopeTest()
    {
        var serviceProviderScopes = new List<Scope>()
        {
            new Scope() { ScopeName = ScopesHelper.AuthzScope, Message = "Test" },
            new Scope() { ScopeName = ScopesHelper.AtpScope, Message = "Test1" },
        };
        var requestScopes = new List<string>() { ScopesHelper.IdentityFullScope, ScopesHelper.IdentityBasicAddressScope };
        var isCustomMessageRequired = serviceProviderScopes.IsCustomMessageRequired(requestScopes, out var message);

        Assert.IsFalse(isCustomMessageRequired);
        Assert.AreEqual(message, null);
    }

    [TestMethod]
    public void CheckMessagePriorityTest()
    {
        var serviceProviderScopes = new List<Scope>()
        {
            new Scope() { ScopeName = ScopesHelper.IdentityBasicAddressScope, Message = "Test" },
            new Scope() { ScopeName = ScopesHelper.IdentityFullScope, Message = "Full" },
        };
        var requestScopes = new List<string>() { ScopesHelper.IdentityFullScope, ScopesHelper.IdentityBasicAddressScope };
        var isCustomMessageRequired = serviceProviderScopes.IsCustomMessageRequired(requestScopes, out var message);
        
        Assert.AreEqual(message, "Full");
    }
}