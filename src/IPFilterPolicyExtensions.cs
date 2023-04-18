using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BBelius.Yarp.ReverseProxy.IPFilters;

/// <summary>
/// Extension methods registering IPFilterPolicyProvider and IPFilterPolicyMiddleware.
/// </summary>
public static class IPFilterPolicyExtensions
{
	/// <summary>
	/// Registers IIPFilterPolicyProvider and configures IPFilterPoliciesConfiguration from provided configuration.
	/// </summary>
	/// <param name="services">DI service collection</param>
	/// <param name="configuration">Configuration</param>
	/// <param name="configSectionName">Optional section name of configuration</param>
	/// <returns>DI service collection</returns>
	public static IServiceCollection AddIPFilterPolicies(this IServiceCollection services, IConfiguration configuration, string configSectionName = "IPFilterConfiguration")
	{
		services.AddOptions();
		services.Configure<IPFilterPoliciesConfiguration>(configuration.GetSection(configSectionName));
		services.AddSingleton<IIPFilterPolicyProvider, IPFilterPolicyProvider>();

		return services;
	}

	/// <summary>
	/// Registers IIPFilterPolicyProvider and uses the given configurePolicies action to register configuration.
	/// </summary>
	/// <param name="services">DI service collection</param>
	/// <param name="configurePolicies">Action that bootstraps configuration</param>
	/// <returns>DI service collection</returns>
	public static IServiceCollection AddIPFilterPolicies(this IServiceCollection services, Action<List<IPFilterPolicy>> configurePolicies)
	{
		services.AddOptions();
		services.Configure(configurePolicies);
		services.AddSingleton<IIPFilterPolicyProvider, IPFilterPolicyProvider>();

		return services;
	}

	/// <summary>
	/// Registers IPFilterPolicyMiddleware.
	/// IMPORTANT: Use in YARP's middleware pipeline.
	/// </summary>
	/// <param name="builder">YARP's middleware pipeline builder</param>
	/// <returns>YARP's middleware pipeline builder</returns>
	public static IReverseProxyApplicationBuilder UseIPFilterPolicies(this IReverseProxyApplicationBuilder builder)
	{
		builder.UseMiddleware<IPFilterPolicyMiddleware>();
		return builder;
	}
}
