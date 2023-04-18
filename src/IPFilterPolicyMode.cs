namespace BBelius.Yarp.ReverseProxy.IPFilters;

/// <summary>
/// Specifies the mode of the IP filter.
/// </summary>
public enum IPFilterPolicyMode
{
	/// <summary>
	/// Disables filtering. All requests are allowed.
	/// </summary>
	Disabled,
	/// <summary>
	/// Blocks all requests except those that match the IP addresses or networks in the list.
	/// </summary>
	AllowList,
	/// <summary>
	/// Allows all requests except those that match the IP addresses or networks in the list.
	/// </summary>
	BlockList
}
