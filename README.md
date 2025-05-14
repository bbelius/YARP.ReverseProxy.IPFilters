# YARP IP Filter Middleware

A customizable IP filtering middleware for Microsoft's [YARP (Yet Another Reverse Proxy)](https://microsoft.github.io/reverse-proxy/)
that provides fine-grained control over allowed or blocked IP addresses globally and per-route, 
ensuring secure and flexible access management.

## Features

- Global IP filtering policies
- Route-specific IP filtering policies
- Easy integration with YARP
- Blocklist and allowlist support
- Can dynamically reload configuration changes

## Installation

Install the package via [NuGet](https://www.nuget.org/packages/BBelius.Yarp.ReverseProxy.IPFilters/):

```
dotnet add package BBelius.Yarp.ReverseProxy.IPFilters
```

## Usage

1. Add the IP filter policy provider service to the DI services bootstrapping code:

```csharp
using BBelius.Yarp.ReverseProxy.IPFilters;
[...]
builder.Services.AddIPFilterPolicies(configuration);
[...]
```

2. Add the IP filter middleware to the YARP middleware pipeline:

```csharp
[...]
// Register the reverse proxy routes
app.MapReverseProxy(proxyPipeline =>
{
   proxyPipeline.UseIPFilterPolicies();
   proxyPipeline.UseLoadBalancing();
   [...]
});
[...]
```

Recommendation: Add it before any other YARP middleware.
**Important**: Do not add the middleware to the regular ASP.NET Core pipeline, as it requires access to the YARP `HttpContext` object.
More information about the YARP middleware pipeline can be found [here](https://microsoft.github.io/reverse-proxy/articles/middleware.html#adding-middleware).

3. Configure the IP filter policy provider in your `appsettings.json`:

```json
{
  "IPFilterConfiguration": {
    "EnableGlobalPolicy": true, // Defaults to false
    "GlobalPolicyName": "Global", // Optional, defaults to "Global"
    "Policies": [
      {
      // Block remote IPs matching the list
        "PolicyName": "Global",
        "Mode": "BlockList",
        "IPAddresses": ["192.168.0.3", "192.168.0.4"]
      },
      {
      // Block remote IPs not within the networks
        "PolicyName": "Intranet", 
        "Mode": "AllowList",
        "IPNetworks": ["192.186.0.0/24"]
      },
      {
        "PolicyName": "TestPolicy",
        "Mode": "Disabled",
        "IPAddresses": ["192.168.0.3", "192.168.0.4"],
        "IPNetworks": ["192.186.0.0/24"]
      }
    ]
  }
}
```

You can use both, IPNetworks and IPAddresses, within the same policy. If at least one address matches the filter will be applied.

4. Configure the YARP routes to use the policies:

```json
{
  "ReverseProxy": {
    "Routes": [
      {
        "RouteId": "route1",
        "ClusterId": "cluster1",
        "Match": {
          "Path": "/{**catch-all}"
        },
        "Metadata": {
          "IPFilterPolicy": "Intranet"
        }
      }
    ]
  }
}
```

## Tracing
Activity source name is `BBelius.Yarp.ReverseProxy.IPFilters`.

## License
This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing
Contributions are welcome! Please open an issue or submit a pull request.
