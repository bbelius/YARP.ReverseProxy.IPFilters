using Microsoft.AspNetCore.HttpOverrides;

namespace UnitTests;

public  class IPNetworkCollectionTests
{
	[Fact]
	public void Contains_SingleAddress()
	{
		var ipAddress = IPAddress.Parse("192.168.0.1");
		var collection = new IPNetworkCollection(new[] { new IPNetwork(ipAddress, 32) });

		Assert.True(collection.Contains(ipAddress));
		Assert.False(collection.Contains(IPAddress.Parse("192.168.0.2")));
	}

	[Fact]
	public void Contains_AddressRange()
	{
		var ipAddress = IPAddress.Parse("192.168.0.0");
		var collection = new IPNetworkCollection(new[] { new IPNetwork(ipAddress, 24) });

		Assert.True(collection.Contains(IPAddress.Parse("192.168.0.1")));
		Assert.True(collection.Contains(IPAddress.Parse("192.168.0.255")));
		Assert.False(collection.Contains(IPAddress.Parse("192.167.255.255")));
		Assert.False(collection.Contains(IPAddress.Parse("192.169.0.0")));
	}

	[Fact]
	public void Contains_MultipleRanges()
	{
		var ipAddress1 = IPAddress.Parse("192.168.0.0");
		var ipAddress2 = IPAddress.Parse("10.0.0.0");
		var collection = new IPNetworkCollection(new[]
		{
				new IPNetwork(ipAddress1, 24),
				new IPNetwork(ipAddress2, 8)
			});

		Assert.True(collection.Contains(IPAddress.Parse("192.168.0.1")));
		Assert.True(collection.Contains(IPAddress.Parse("192.168.0.255")));
		Assert.True(collection.Contains(IPAddress.Parse("10.0.0.1")));
		Assert.True(collection.Contains(IPAddress.Parse("10.255.255.255")));

		Assert.False(collection.Contains(IPAddress.Parse("192.167.255.255")));
		Assert.False(collection.Contains(IPAddress.Parse("192.169.0.0")));
		Assert.False(collection.Contains(IPAddress.Parse("11.0.0.0")));
	}
}
