using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Model;

namespace UnitTests;

public class IPFilterPolicyMiddlewareTests
{
	private readonly DefaultHttpContext _context;
	private readonly Mock<ILogger<IPFilterPolicyMiddleware>> _loggerMock;
	private readonly RouteModel _route;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

    public IPFilterPolicyMiddlewareTests()
	{
		_context = new DefaultHttpContext();

		_route = new RouteModel(new RouteConfig()
		{
			RouteId = "TestRoute",
			Metadata = new Dictionary<string, string>
				{
					{ "IPFilterPolicy", "TestPolicy" }
				}
		}, null, HttpTransformer.Default);

		var reverseProxyFeatureMock = new Mock<IReverseProxyFeature>();
		reverseProxyFeatureMock.Setup(m => m.Route).Returns(_route);

		_context.Features.Set<IReverseProxyFeature>(reverseProxyFeatureMock.Object);
		_context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");

		_loggerMock = new Mock<ILogger<IPFilterPolicyMiddleware>>();
	}

	[Fact]
	public async Task InvokeAsync_AllowsRequest_WhenNoPolicy()
	{
		// Arrange
		var context = new DefaultHttpContext();

		var route = new RouteModel(new RouteConfig()
		{
			RouteId = "TestRoute",
			Metadata = new Dictionary<string, string> {}
		}, null, HttpTransformer.Default);

		var reverseProxyFeatureMock = new Mock<IReverseProxyFeature>();
		reverseProxyFeatureMock.Setup(m => m.Route).Returns(route);

		context.Features.Set<IReverseProxyFeature>(reverseProxyFeatureMock.Object);
		context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");

		var policyProviderMock = new Mock<IIPFilterPolicyProvider>();
		var loggerMock = new Mock<ILogger<IPFilterPolicyMiddleware>>();
		policyProviderMock.Setup(m => m.GetGlobalPolicy()).Returns(null as IPFilterPolicy);


        var middleware = new IPFilterPolicyMiddleware(async _ => { }, loggerMock.Object, policyProviderMock.Object);

        // Act
        await middleware.InvokeAsync(context);

		// Assert
		Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
	}

	[Fact]
	public async Task InvokeAsync_BlocksRequest_WhenGlobalPolicyBlocksIPAddress()
	{
		// Arrange
		var globalPolicy = new IPFilterPolicy
		{
			Mode = IPFilterPolicyMode.BlockList,
			IPAddresses = ["127.0.0.1"]
		};
		var testPolicy = new IPFilterPolicy
		{
			Mode = IPFilterPolicyMode.Disabled
		};

		var policyProviderMock = new Mock<IIPFilterPolicyProvider>();
		policyProviderMock.Setup(m => m.GetGlobalPolicy()).Returns(globalPolicy);
		policyProviderMock.Setup(m => m.GetPolicy("TestPolicy")).Returns(testPolicy);

		var loggerMock = new Mock<ILogger<IPFilterPolicyMiddleware>>();

		var middleware = new IPFilterPolicyMiddleware(async _ => { }, loggerMock.Object, policyProviderMock.Object);

		// Act
		await middleware.InvokeAsync(_context);

		// Assert
		Assert.Equal(StatusCodes.Status403Forbidden, _context.Response.StatusCode);
	}

	[Fact]
	public async Task InvokeAsync_BlocksRequest_WhenRoutePolicyBlocksIPAddress()
	{
		// Arrange
		var routePolicy = new IPFilterPolicy
		{
			Mode = IPFilterPolicyMode.BlockList,
			IPAddresses = ["127.0.0.1"]
		};

		var policyProviderMock = new Mock<IIPFilterPolicyProvider>();
		policyProviderMock.Setup(m => m.GetPolicy("TestPolicy")).Returns(routePolicy);

		var loggerMock = new Mock<ILogger<IPFilterPolicyMiddleware>>();

		var middleware = new IPFilterPolicyMiddleware(async _ => { }, loggerMock.Object, policyProviderMock.Object);

		// Act
		await middleware.InvokeAsync(_context);

		// Assert
		Assert.Equal(StatusCodes.Status403Forbidden, _context.Response.StatusCode);
	}

	[Fact]
	public async Task InvokeAsync_AllowsRequest_WhenGlobalPolicyAllowsIPAddress()
	{
		// Arrange
		var globalPolicy = new IPFilterPolicy
		{
			Mode = IPFilterPolicyMode.AllowList,
			IPAddresses = ["127.0.0.1"]
		};
		var testPolicy = new IPFilterPolicy
		{
			Mode = IPFilterPolicyMode.Disabled
		};

		var policyProviderMock = new Mock<IIPFilterPolicyProvider>();
		policyProviderMock.Setup(m => m.GetGlobalPolicy()).Returns(globalPolicy);
		policyProviderMock.Setup(m => m.GetPolicy("TestPolicy")).Returns(testPolicy);

		var loggerMock = new Mock<ILogger<IPFilterPolicyMiddleware>>();

		var middleware = new IPFilterPolicyMiddleware(async _ => { }, loggerMock.Object, policyProviderMock.Object);

		// Act
		await middleware.InvokeAsync(_context);

		// Assert
		Assert.Equal(StatusCodes.Status200OK, _context.Response.StatusCode);
	}

	[Fact]
	public async Task InvokeAsync_AllowsRequest_WhenRoutePolicyAllowsIPAddress()
	{
		// Arrange
		_context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");

		var routePolicy = new IPFilterPolicy
		{
			Mode = IPFilterPolicyMode.AllowList,
			IPAddresses = ["127.0.0.1"]
		};

		var policyProviderMock = new Mock<IIPFilterPolicyProvider>();
		policyProviderMock.Setup(m => m.GetPolicy("TestPolicy")).Returns(new IPFilterPolicy() { PolicyName = "TestPolicy", Mode = IPFilterPolicyMode.Disabled });

		var middleware = new IPFilterPolicyMiddleware(async _ => { }, _loggerMock.Object, policyProviderMock.Object);

		// Act
		await middleware.InvokeAsync(_context);

		// Assert
		Assert.Equal(StatusCodes.Status200OK, _context.Response.StatusCode);
	}

	[Fact]
	public async Task InvokeAsync_BlocksRequest_WhenGlobalPolicyBlocksIPNetwork()
	{
		// Arrange
		var globalPolicy = new IPFilterPolicy
		{
			Mode = IPFilterPolicyMode.BlockList,
			IPNetworks = ["127.0.0.0/24"]
		};

		var policyProviderMock = new Mock<IIPFilterPolicyProvider>();
		policyProviderMock.Setup(m => m.GetGlobalPolicy()).Returns(globalPolicy);

		var middleware = new IPFilterPolicyMiddleware(async _ => { }, _loggerMock.Object, policyProviderMock.Object);

		// Act
		await middleware.InvokeAsync(_context);

		// Assert
		Assert.Equal(StatusCodes.Status403Forbidden, _context.Response.StatusCode);
	}

	[Fact]
	public async Task InvokeAsync_BlocksRequest_WhenRoutePolicyBlocksIPNetwork()
	{
		// Arrange
		var routePolicy = new IPFilterPolicy
		{
			Mode = IPFilterPolicyMode.BlockList,
			IPNetworks = ["127.0.0.0/24"]
		};

		var policyProviderMock = new Mock<IIPFilterPolicyProvider>();
		policyProviderMock.Setup(m => m.GetPolicy("TestPolicy")).Returns(routePolicy);

		var middleware = new IPFilterPolicyMiddleware(async _ => { }, _loggerMock.Object, policyProviderMock.Object);

		// Act
		await middleware.InvokeAsync(_context);

		// Assert
		Assert.Equal(StatusCodes.Status403Forbidden, _context.Response.StatusCode);
	}

	[Fact]
	public async Task InvokeAsync_BlocksRequest_WhenRoutePolicyBlocksIPAddressAndIPNetwork()
	{
		// Arrange
		var routePolicy = new IPFilterPolicy
		{
			Mode = IPFilterPolicyMode.BlockList,
			IPAddresses = ["192.168.0.1"],
			IPNetworks = ["127.0.0.0/24"]
		};

		var policyProviderMock = new Mock<IIPFilterPolicyProvider>();
		policyProviderMock.Setup(m => m.GetPolicy("TestPolicy")).Returns(routePolicy);

		var middleware = new IPFilterPolicyMiddleware(async _ => { }, _loggerMock.Object, policyProviderMock.Object);

		// Act
		await middleware.InvokeAsync(_context);

		// Assert
		Assert.Equal(StatusCodes.Status403Forbidden, _context.Response.StatusCode);
	}

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}