namespace BBelius.Yarp.ReverseProxy.IPFilters;

/// <summary>
/// Exceptions for IPFilterPolicy.
/// </summary>
/// <remarks>
/// Constructor
/// </remarks>
/// <param name="message">Exception Message</param>
/// <param name="innerException">Inner exception</param>
public class IPFilterPolicyException(string message, Exception? innerException = null) : Exception(message, innerException)
{
}

/// <summary>
/// Exception for when a policy is not found.
/// </summary>
/// <remarks>
/// Constructor
/// </remarks>
/// <param name="policyName">Name of policy that was requested</param>
/// <param name="routeName">YARP route ID that requested the policy</param>
public class IPFilterPolicyNotFoundException(string policyName, string routeName) : IPFilterPolicyException($"Could not find IPFilterPolicy with name {policyName}. Route: {routeName}")
{
}
