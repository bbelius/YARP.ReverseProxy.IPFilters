using System.Net;

using Microsoft.AspNetCore.HttpOverrides;

namespace BBelius.Yarp.ReverseProxy.IPFilters;

/// <summary>
/// Policy for IPFilter. Default values: Blocks all requests.
/// </summary>
public class IPFilterPolicy
{
	private HashSet<IPAddress>? _ipAddresses = null;
	private IPNetworkCollection? _ipNetworks = null;

	/// <summary>
	/// The name of the policy. This is used to identify the policy in the route configuration.
	/// </summary>
	public string PolicyName { get; init; } = "DefaultPolicy";

	/// <summary>
	/// The filter mode of the policy. Default: AllowList
	/// </summary>
	public IPFilterPolicyMode Mode { get; init; } = IPFilterPolicyMode.AllowList;

	/// <summary>
	/// The IPAddresses that are allowed or blocked.
	/// </summary>
	public List<string> IPAddresses { get; init; } = new();

	/// <summary>
	/// The IPNetworks that are allowed or blocked.
	/// </summary>
	public List<string> IPNetworks { get; init; } = new();

	/// <summary>
	/// Returns the parsed IPAddresses as an array of IPAddress objects.
	/// The parsing is done lazily, and the result is cached for subsequent calls.
	/// </summary>
	public HashSet<IPAddress> GetIPAddresses() => _ipAddresses ??= new HashSet<IPAddress>(IPAddresses.Select(IPAddress.Parse));

	/// <summary>
	/// Returns the parsed IPNetworks as an array of IPNetwork objects.
	/// The parsing is done lazily, and the result is cached for subsequent calls.
	/// </summary>
	public IPNetworkCollection GetIPNetworks() => _ipNetworks ??= new IPNetworkCollection(IPNetworks.Select(ToIPNetwork));

	private static IPNetwork ToIPNetwork(string ipNetwork)
	{
		var parts = ipNetwork.Split('/', StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length != 2)
			throw new ArgumentException($"Invalid IPNetwork format: {ipNetwork}");
		return new IPNetwork(IPAddress.Parse(parts[0]), int.Parse(parts[1]));
	}
}
