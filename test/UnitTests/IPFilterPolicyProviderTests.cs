namespace UnitTests;

public class IPFilterPolicyProviderTests
{
	[Fact]
	public void GetPolicy_ReturnsCorrectPolicy()
	{
		// Arrange
		var policy = new IPFilterPolicy { PolicyName = "TestPolicy" };
		var policiesConfiguration = new IPFilterPoliciesConfiguration
		{
			Policies = [policy]
		};

		var optionsMonitorMock = new Mock<IOptionsMonitor<IPFilterPoliciesConfiguration>>();
		optionsMonitorMock.Setup(m => m.CurrentValue).Returns(policiesConfiguration);

		var provider = new IPFilterPolicyProvider(optionsMonitorMock.Object);

		// Act
		var returnedPolicy = provider.GetPolicy("TestPolicy");

		// Assert
		Assert.NotNull(returnedPolicy);
		Assert.Equal("TestPolicy", returnedPolicy.PolicyName);
	}

	[Fact]
	public void GetPolicy_ReturnsNullForUnknownPolicy()
	{
		// Arrange
		var policy = new IPFilterPolicy { PolicyName = "TestPolicy" };
		var policiesConfiguration = new IPFilterPoliciesConfiguration
		{
			Policies = [policy]
		};

		var optionsMonitorMock = new Mock<IOptionsMonitor<IPFilterPoliciesConfiguration>>();
		optionsMonitorMock.Setup(m => m.CurrentValue).Returns(policiesConfiguration);

		var provider = new IPFilterPolicyProvider(optionsMonitorMock.Object);

		// Act
		var returnedPolicy = provider.GetPolicy("UnknownPolicy");

		// Assert
		Assert.Null(returnedPolicy);
	}
}