using Microsoft.Extensions.Options;

namespace BBelius.Yarp.ReverseProxy.IPFilters;

/// <summary>
/// IP filter policy provider.
/// </summary>
public class IPFilterPolicyProvider : IIPFilterPolicyProvider
{
	private IDictionary<string, IPFilterPolicy> _policies = new Dictionary<string, IPFilterPolicy>();
	private IPFilterPolicy? _globalPolicy = null;

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="policiesConfigurationMonitor">Options monitor</param>
	public IPFilterPolicyProvider(IOptionsMonitor<IPFilterPoliciesConfiguration> policiesConfigurationMonitor)
	{
		policiesConfigurationMonitor.OnChange(UpdatePolicies);
		UpdatePolicies(policiesConfigurationMonitor.CurrentValue);
	}

	private void UpdatePolicies(IPFilterPoliciesConfiguration policies)
	{
		_policies = policies.Policies.ToDictionary(p => p.PolicyName);

		if (policies.EnableGlobalPolicy)
		{
			if (_policies.TryGetValue(policies.GlobalPolicyName, out var globalPolicy) && globalPolicy is not null)
				_globalPolicy = globalPolicy;
			else
				throw new Exception("Could not find Global Policy. Double check configuration or disable the global policy feature.");
		}
		else
			_globalPolicy = null;
	}

	/// <summary>
	/// Returns the policy with the given name.
	/// </summary>
	/// <param name="policyName">Policy name</param>
	/// <returns>Policy</returns>
	public IPFilterPolicy? GetPolicy(string policyName)
	=> _policies.TryGetValue(policyName, out var policy) ? policy : null;

	/// <summary>
	/// Returns the global policy or null if it is not enabled.
	/// </summary>
	/// <returns>Global policy</returns>
	public IPFilterPolicy? GetGlobalPolicy()
		=> _globalPolicy;
}
