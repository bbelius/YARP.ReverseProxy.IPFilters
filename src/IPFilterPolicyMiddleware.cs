﻿using System.Diagnostics.Contracts;
using System.Net;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BBelius.Yarp.ReverseProxy.IPFilters;

/// <summary>
/// This middleware is used to filter requests based on the IPFilterPolicy.
/// </summary>
/// <remarks>
/// Constructor
/// </remarks>
/// <param name="next">Request delegate</param>
/// <param name="logger">Logger</param>
/// <param name="policyProvider">Policy provider</param>
public class IPFilterPolicyMiddleware(RequestDelegate next, ILogger<IPFilterPolicyMiddleware> logger, IIPFilterPolicyProvider policyProvider)
{

    /// <summary>
    /// Middleware invoke method.
    /// </summary>
    /// <param name="context">HttpContext</param>
    /// <returns>Task</returns>
    /// <exception cref="IPFilterPolicyNotFoundException">Exception is thrown if policy is not found</exception>
    public async Task InvokeAsync(HttpContext context)
	{
		IPAddress remoteIP = context.Connection.RemoteIpAddress!;

		if (remoteIP.IsIPv4MappedToIPv6)
			remoteIP = remoteIP.MapToIPv4();

		var globalPolicy = policyProvider.GetGlobalPolicy();

		// If the global policy is not null, check if the remote IP is allowed.
		if (globalPolicy is not null && !IsRemoteIPAllowed(remoteIP, globalPolicy))
		{
			logger.LogWarning("Request from IP {ip} was blocked. Global Policy: {policyName}. Mode: {mode}", remoteIP, globalPolicy.PolicyName, globalPolicy.Mode);
			context.Response.StatusCode = StatusCodes.Status403Forbidden;
			return;
		}

		var route = context.GetReverseProxyFeature().Route;
		var metadata = route.Config.Metadata;
		var policyName = string.Empty;

		// If the metadata is null or the IPFilterPolicy is not found, bypass the middleware.
		if (metadata?.TryGetValue("IPFilterPolicy", out policyName) != true || string.IsNullOrEmpty(policyName))
		{
			logger.LogDebug("No IPFilterPolicy found. Bypassing middleware. Route: {routeName}", route.Config.RouteId);
			await next(context);
			return;
		}

		var policy = policyProvider.GetPolicy(policyName);

		// If the policy is null, log a critical error and throw an exception.
		if (policy is null)
		{
			logger.LogCritical("Could not find IPFilterPolicy with name {policyName}. Route: {routeName}", policyName, route.Config.RouteId);
			context.Response.StatusCode = StatusCodes.Status500InternalServerError;
			throw new IPFilterPolicyNotFoundException(policyName, route.Config.RouteId);
		}

		if (policy.Mode == IPFilterPolicyMode.Disabled)
		{
			logger.LogInformation("IPFilterPolicy {policyName} is disabled. Route: {routeName}", policyName, route.Config.RouteId);
			await next(context);
			return;
		}

		// If the IP is not allowed, block the request and shortcut middleware chain.
		if (!IsRemoteIPAllowed(remoteIP, policy))
		{
			logger.LogWarning("Request from IP {ip} was blocked. Route: {routeName}. Policy: {policyName}. Mode: {mode}", remoteIP, route.Config.RouteId, policy.PolicyName, policy.Mode);
			context.Response.StatusCode = StatusCodes.Status403Forbidden;
			return;
		}

		logger.LogInformation("Request from IP {ip} was allowed. Route: {routeName}. Policy: {policyName}. Mode: {mode}", remoteIP, route.Config.RouteId, policy.PolicyName, policy.Mode);
		await next(context);
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
