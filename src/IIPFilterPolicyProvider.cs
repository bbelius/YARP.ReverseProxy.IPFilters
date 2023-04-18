namespace BBelius.Yarp.ReverseProxy.IPFilters;

/// <summary>
/// Interface for IPFilterPolicyProvider.
/// </summary>
public interface IIPFilterPolicyProvider
{
	/// <summary>
	/// Returns the policy with the given name.
	/// </summary>
	/// <param name="policyName">The policy name.</param>
	/// <returns>The IPFilterPolicy or null if not found.</returns>
	IPFilterPolicy? GetPolicy(string policyName);

	/// <summary>
	/// Returns the global policy.
	/// </summary>
	/// <returns>The IPFilterPoliicy or null if none is defined.</returns>
	IPFilterPolicy? GetGlobalPolicy();

}

