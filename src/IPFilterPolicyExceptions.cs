namespace BBelius.Yarp.ReverseProxy.IPFilters;

/// <summary>
/// Exceptions for IPFilterPolicy.
/// </summary>
public class IPFilterPolicyException : Exception
{
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="message">Exception Message</param>
	/// <param name="innerException">Inner exception</param>
	public IPFilterPolicyException(string message, Exception? innerException = null) : base(message, innerException)
	{
	}
}

/// <summary>
/// Exception for when a policy is not found.
/// </summary>
public class IPFilterPolicyNotFoundException : IPFilterPolicyException
{
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="policyName">Name of policy that was requested</param>
	/// <param name="routeName">YARP route ID that requested the policy</param>
	public IPFilterPolicyNotFoundException(string policyName, string routeName)
		: base($"Could not find IPFilterPolicy with name {policyName}. Route: {routeName}")
	{
	}
}
