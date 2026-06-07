using System.Net;
using Microsoft.AspNetCore.Http;
using YangOne.Log;
using YangOne.Web.Service;

namespace YangOne.Web.Middleware;

public class AdminIPAccessMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;
    private readonly IRestrictionService _restrictionService;

    public AdminIPAccessMiddleware(
        RequestDelegate next,
        ILogger logger,
        IRestrictionService restrictionService)
    {
        _next = next;
        _logger = logger;
        _restrictionService = restrictionService;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp == null)
        {
            await _next(context);
            return;
        }

        var userRoles = GetUserRoles(context);
        if (userRoles.Count == 0)
        {
            await _next(context);
            return;
        }

        var rules = await _restrictionService.GetCachedAdminIPAccessListAsync();
        var matchingRules = rules.Where(r => userRoles.Contains(r.RoleId)).ToList();

        if (matchingRules.Count == 0)
        {
            await _next(context);
            return;
        }

        var isAllowed = matchingRules.Any(r => IsIpAllowed(remoteIp, r));

        if (!isAllowed)
        {
            _logger.Log(LogType.Info, () =>
                $"Blocked admin IP access: {remoteIp} for user {context.User.Identity.Name}");
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                "{\"Code\":403,\"Message\":\"Access denied: Your IP is not authorized for admin access.\"}");
            return;
        }

        await _next(context);
    }

    private static List<long> GetUserRoles(HttpContext context)
    {
        var roles = new List<long>();
        var claimsPrincipal = context.User;
        var roleClaim = claimsPrincipal.FindFirst("role")?.Value
                        ?? claimsPrincipal.FindFirst(claimsPrincipal.Identity?.AuthenticationType + "/role")?.Value;

        if (!string.IsNullOrEmpty(roleClaim) && long.TryParse(roleClaim, out var singleRoleId))
            roles.Add(singleRoleId);

        var roleClaims = claimsPrincipal.FindAll("role").Select(c => c.Value).ToList();
        foreach (var rc in roleClaims)
        {
            if (long.TryParse(rc, out var rid) && !roles.Contains(rid))
                roles.Add(rid);
        }

        return roles;
    }

    private static bool IsIpAllowed(IPAddress remoteIp, Model.AdministrativeIPAccess rule)
    {
        if (remoteIp.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && rule.AllowIPV4)
            return IsIpInRange(remoteIp, rule.IPV4Range);

        if (remoteIp.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 && rule.AllowIPV6)
            return IsIpInRange(remoteIp, rule.IPV6Range);

        return false;
    }

    private static bool IsIpInRange(IPAddress ip, string range)
    {
        if (string.IsNullOrWhiteSpace(range))
            return false;

        range = range.Trim();

        if (range.Contains('/'))
        {
            if (IPNetwork.TryParse(range, out var network))
                return network.Contains(ip);
            return false;
        }

        if (range.Contains('-'))
        {
            var parts = range.Split('-', 2);
            if (parts.Length == 2 &&
                IPAddress.TryParse(parts[0].Trim(), out var start) &&
                IPAddress.TryParse(parts[1].Trim(), out var end))
            {
                var ipBytes = ip.GetAddressBytes();
                var startBytes = start.GetAddressBytes();
                var endBytes = end.GetAddressBytes();

                if (ipBytes.Length != startBytes.Length || ipBytes.Length != endBytes.Length)
                    return false;

                for (int i = 0; i < ipBytes.Length; i++)
                {
                    if (ipBytes[i] < startBytes[i] || ipBytes[i] > endBytes[i])
                        return false;
                }
                return true;
            }
            return false;
        }

        return IPAddress.TryParse(range, out var parsed) && ip.Equals(parsed);
    }
}

internal static class IPNetwork
{
    public static bool TryParse(string value, out (IPAddress Network, int PrefixLength) network)
    {
        network = (null, 0);
        var parts = value.Split('/');
        if (parts.Length != 2) return false;

        if (!IPAddress.TryParse(parts[0], out var ip)) return false;
        if (!int.TryParse(parts[1], out var prefixLength)) return false;

        network = (ip, prefixLength);
        return true;
    }

    public static bool Contains(this (IPAddress Network, int PrefixLength) network, IPAddress ip)
    {
        if (network.Network.AddressFamily != ip.AddressFamily)
            return false;

        var networkBytes = network.Network.GetAddressBytes();
        var ipBytes = ip.GetAddressBytes();
        int bits = network.PrefixLength;
        int bytes = bits / 8;
        int remainingBits = bits % 8;

        for (int i = 0; i < bytes; i++)
        {
            if (networkBytes[i] != ipBytes[i])
                return false;
        }

        if (remainingBits > 0)
        {
            int mask = (byte)(0xFF << (8 - remainingBits));
            if ((networkBytes[bytes] & mask) != (ipBytes[bytes] & mask))
                return false;
        }

        return true;
    }
}
