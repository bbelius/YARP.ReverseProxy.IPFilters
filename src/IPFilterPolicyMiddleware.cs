using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Net;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BBelius.Yarp.ReverseProxy.IPFilters;

/// <summary>
/// This middleware is used to filter requests based on the IPFilterPolicy.
/// </summary>
public sealed class IPFilterPolicyMiddleware
{
	private static readonly ActivitySource _activitySource = new("BBelius.Yarp.ReverseProxy.IPFilters");
    private readonly RequestDelegate _next;
    private readonly ILogger<IPFilterPolicyMiddleware> _logger;
	private readonly IIPFilterPolicyProvider _policyProvider;

    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="next">Request delegate</param>
    /// <param name="logger">Logger</param>
    /// <param name="policyProvider">Policy provider</param>
    public IPFilterPolicyMiddleware(RequestDelegate next, ILogger<IPFilterPolicyMiddleware> logger, IIPFilterPolicyProvider policyProvider)
	{
		_next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _policyProvider = policyProvider ?? throw new ArgumentNullException(nameof(policyProvider));
    }

    private static class Log
    {
        private static readonly Action<ILogger, IPAddress, string, IPFilterPolicyMode, Exception?> _requestBlockedGlobal = LoggerMessage.Define<IPAddress, string, IPFilterPolicyMode>(LogLevel.Warning, new EventId(1001, "RequestBlockedGlobal"), "Request from IP {ip} was blocked. Global Policy: {policyName}. Mode: {mode}");
        private static readonly Action<ILogger, string, Exception?> _noIPPolicyFoundDebug = LoggerMessage.Define<string>(LogLevel.Debug, new EventId(1002, "NoIPPolicyFound"), "No IPFilterPolicy found. Bypassing middleware. Route: {routeName}");
		private static readonly Action<ILogger, string, string, Exception?> _policyIsNullException = LoggerMessage.Define<string, string>(LogLevel.Critical, new EventId(1003, "PolicyIsNull"), "Could not find IPFilterPolicy with name {policyName}. Route: {routeName}");
		private static readonly Action<ILogger, string, string, Exception?> _policyIsDisabled = LoggerMessage.Define<string, string>(LogLevel.Information, new EventId(1004, "PolicyIsDisabled"), "IPFilterPolicy {policyName} is disabled. Route: {routeName}");
		private static readonly Action<ILogger, IPAddress, string, string, IPFilterPolicyMode, Exception?> _requestBlocked = LoggerMessage.Define<IPAddress, string, string, IPFilterPolicyMode>(LogLevel.Warning, new EventId(1005, "RequestBlocked"), "Request from IP {ip} was blocked. Route: {routeName}. Policy: {policyName}. Mode: {mode}");
		private static readonly Action<ILogger, IPAddress, string, string, IPFilterPolicyMode, Exception?> _requestAllowed = LoggerMessage.Define<IPAddress, string, string, IPFilterPolicyMode>(LogLevel.Information, new EventId(1006, "RequestAllowed"), "Request from IP {ip} was allowed. Route: {routeName}. Policy: {policyName}. Mode: {mode}");

        public static void RequestBlockedGlobal(ILogger logger, IPAddress ip, string policyName, IPFilterPolicyMode mode)
			=> _requestBlockedGlobal(logger, ip, policyName, mode, null);

        public static void NoIPPolicyFoundDebug(ILogger logger, string routeId)
            => _noIPPolicyFoundDebug(logger, routeId, null);

		public static void PolicyIsNullException(ILogger logger, string policyName, string routeId)
			=> _policyIsNullException(logger, policyName, routeId, null);

		public static void PolicyIsDisabled(ILogger logger, string policyName, string routeId)
            => _policyIsDisabled(logger, policyName, routeId, null);

        public static void RequestBlocked(ILogger logger, IPAddress ip, string routeId, string policyName, IPFilterPolicyMode mode)
            => _requestBlocked(logger, ip, routeId, policyName, mode, null);

		public static void RequestAllowed(ILogger logger, IPAddress ip, string routeId, string policyName, IPFilterPolicyMode mode)
            => _requestAllowed(logger, ip, routeId, policyName, mode, null);
    }

    /// <summary>
    /// Middleware invoke method.
    /// </summary>
    /// <param name="context">HttpContext</param>
    /// <returns>Task</returns>
    /// <exception cref="IPFilterPolicyNotFoundException">Exception is thrown if policy is not found</exception>
    public async Task InvokeAsync(HttpContext context)
	{
        using var activity = _activitySource.StartActivity("IPFilterEvaluation");

        IPAddress remoteIP = context.Connection.RemoteIpAddress!;

		if (remoteIP.IsIPv4MappedToIPv6)
			remoteIP = remoteIP.MapToIPv4();

		var globalPolicy = _policyProvider.GetGlobalPolicy();

		// If the global policy is not null, check if the remote IP is allowed.
		if (globalPolicy is not null && !IsRemoteIPAllowed(remoteIP, globalPolicy))
		{
			Log.RequestBlockedGlobal(_logger, remoteIP, globalPolicy.PolicyName, globalPolicy.Mode);
            activity?.SetTag("blocked", true);
            activity?.SetTag("policy", "global");
            activity?.SetTag("ip", remoteIP.ToString());
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
			return;
		}

		var route = context.GetReverseProxyFeature().Route;
		var metadata = route.Config.Metadata;
		var policyName = string.Empty;

		// If the metadata is null or the IPFilterPolicy is not found, bypass the middleware.
		if (metadata?.TryGetValue("IPFilterPolicy", out policyName) != true || string.IsNullOrEmpty(policyName))
		{
			Log.NoIPPolicyFoundDebug(_logger, route.Config.RouteId);
			await _next(context);
			return;
		}

		var policy = _policyProvider.GetPolicy(policyName);

		// If the policy is null, log a critical error and throw an exception.
		if (policy is null)
		{
			Log.PolicyIsNullException(_logger, policyName, route.Config.RouteId);
            activity?.SetTag("error", "policy_not_found");
            activity?.SetStatus(ActivityStatusCode.Error);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            throw new IPFilterPolicyNotFoundException(policyName, route.Config.RouteId);
		}

		if (policy.Mode == IPFilterPolicyMode.Disabled)
		{
			Log.PolicyIsDisabled(_logger, policyName, route.Config.RouteId);
			await _next(context);
			return;
		}

		// If the IP is not allowed, block the request and shortcut middleware chain.
		if (!IsRemoteIPAllowed(remoteIP, policy))
		{
			Log.RequestBlocked(_logger, remoteIP, route.Config.RouteId, policy.PolicyName, policy.Mode);
            activity?.SetTag("blocked", true);
            activity?.SetTag("policy", policy.PolicyName);
            activity?.SetTag("ip", remoteIP.ToString());
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
			return;
		}

		Log.RequestAllowed(_logger, remoteIP, route.Config.RouteId, policy.PolicyName, policy.Mode);
		await _next(context);
	}

	[Pure]
	private static bool IsRemoteIPAllowed(IPAddress remoteIP, IPFilterPolicy policy)
		=> policy.Mode switch
		{
			IPFilterPolicyMode.AllowList => policy.GetIPAddresses().Contains(remoteIP) || policy.GetIPNetworks().Contains(remoteIP),
			IPFilterPolicyMode.BlockList => !(policy.GetIPAddresses().Contains(remoteIP) || policy.GetIPNetworks().Contains(remoteIP)),
			_ => false
		};
}
