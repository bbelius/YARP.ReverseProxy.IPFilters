namespace BBelius.Yarp.ReverseProxy.IPFilters;

/// <summary>
/// IP filter policies configuration.
/// </summary>
public class IPFilterPoliciesConfiguration
{
	/// <summary>
	/// List of IP filter policies.
	/// </summary>
	public IList<IPFilterPolicy> Policies { get; set; } = new List<IPFilterPolicy>();

	/// <summary>
	/// Name of the global policy.
	/// </summary>
	public string GlobalPolicyName { get; set; } = "Global";

	/// <summary>
	/// Enables the global policy module. If set to true, the global policy will be checked before route specific policies.
	/// </summary>
	public bool EnableGlobalPolicy { get; set; } = false;
}
