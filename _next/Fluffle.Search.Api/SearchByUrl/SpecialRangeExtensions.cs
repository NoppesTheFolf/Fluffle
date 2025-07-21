using System.Net;
using System.Net.Sockets;

namespace Fluffle.Search.Api.SearchByUrl;

public static class SpecialRangeExtensions
{
    // Based on https://github.com/whitequark/ipaddr.js/blob/08c2cd41e2cb3400683cbd503f60421bfdf66921/lib/ipaddr.js#L1021

    public static bool IsSpecialRangeDefault(this IPAddress ipAddress)
    {
        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            return ipAddress.GetIpv4SpecialRange() == Ipv4SpecialRange.Default;
        }

        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            return ipAddress.GetIpv6SpecialRange() == Ipv6SpecialRange.Default;
        }

        throw new ArgumentException("IP address must be IPv4 or IPv6.", nameof(ipAddress));
    }

    public static Ipv6SpecialRange GetIpv6SpecialRange(this IPAddress ipAddress)
    {
        if (ipAddress.AddressFamily != AddressFamily.InterNetworkV6)
        {
            throw new ArgumentException("IP address must be IPv6.", nameof(ipAddress));
        }

        (Ipv6SpecialRange, IPNetwork[])[] ranges = [
            // RFC4291, here and after
            (Ipv6SpecialRange.Unspecified, [new IPNetwork(new IPAddress([0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]), 128)]),
            (Ipv6SpecialRange.LinkLocal,[new IPNetwork(new IPAddress([0xfe, 0x80, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]), 10)]),
            (Ipv6SpecialRange.Multicast, [new IPNetwork(new IPAddress([0xff, 0x00, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]), 8)]),
            (Ipv6SpecialRange.Loopback, [new IPNetwork(new IPAddress([0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1]), 128)]),
            (Ipv6SpecialRange.UniqueLocal, [new IPNetwork(new IPAddress([0xfc, 0x00, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]), 7)]),
            (Ipv6SpecialRange.Ipv4Mapped, [new IPNetwork(new IPAddress([0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xff, 0xff, 0, 0, 0, 0]), 96)]),
            // RFC6666
            (Ipv6SpecialRange.Discard, [new IPNetwork(new IPAddress([0x01, 0x00, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]), 64)]),
            // RFC6145
            (Ipv6SpecialRange.Rfc6145, [new IPNetwork(new IPAddress([0, 0, 0, 0, 0, 0, 0, 0, 0xff, 0xff, 0, 0, 0, 0, 0, 0]), 96)]),
            // RFC6052
            (Ipv6SpecialRange.Rfc6052, [new IPNetwork(new IPAddress([0x00, 0x64, 0xff, 0x9b, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]), 96)]),
            // RFC3056
            (Ipv6SpecialRange.SixToFour, [new IPNetwork(new IPAddress([0x20, 0x02, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]), 16)]),
            // RFC6052, RFC6146
            (Ipv6SpecialRange.Teredo, [new IPNetwork(new IPAddress([0x20, 0x01, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]), 32)]),
            // RFC5180
            (Ipv6SpecialRange.Benchmarking, [new IPNetwork(new IPAddress([0x20, 0x01, 0x00, 0x02, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]), 48)]),
            // RFC7450
            (Ipv6SpecialRange.Amt, [new IPNetwork(new IPAddress([0x20, 0x01, 0x00, 0x03, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]), 32)]),
            (Ipv6SpecialRange.As112V6, [
                new IPNetwork(new IPAddress([0x20, 0x01, 0x00, 0x04, 0x01, 0x12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]), 48),
                new IPNetwork(new IPAddress([0x26, 0x20, 0x00, 0x4f, 0x80, 0x00, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]), 48),
            ]),
            (Ipv6SpecialRange.Deprecated, [new IPNetwork(new IPAddress([0x20, 0x01, 0x00, 0x10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]), 28)]),
            (Ipv6SpecialRange.Orchid2, [new IPNetwork(new IPAddress([0x20, 0x01, 0x00, 0x20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]), 28)]),
            (Ipv6SpecialRange.DroneRemoteIdProtocolEntityTags, [new IPNetwork(new IPAddress([0x20, 0x01, 0x00, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]), 28)]),
            (Ipv6SpecialRange.Reserved, [
                // RFC3849
                new IPNetwork(new IPAddress([0x20, 0x01, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]), 23),
                // RFC2928
                new IPNetwork(new IPAddress([0x20, 0x01, 0xd, 0xb8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]), 32),
            ])
        ];

        foreach (var (specialRange, ipNetworks) in ranges)
        {
            foreach (var ipNetwork in ipNetworks)
            {
                if (ipNetwork.Contains(ipAddress))
                {
                    return specialRange;
                }
            }
        }

        return Ipv6SpecialRange.Default;
    }

    public static Ipv4SpecialRange GetIpv4SpecialRange(this IPAddress ipAddress)
    {
        if (ipAddress.AddressFamily != AddressFamily.InterNetwork)
        {
            throw new ArgumentException("IP address must be IPv4.", nameof(ipAddress));
        }

        (Ipv4SpecialRange, IPNetwork[])[] ranges =
        [
            (Ipv4SpecialRange.Unspecified, [new IPNetwork(new IPAddress([0, 0, 0, 0]), 8)]),
            (Ipv4SpecialRange.Broadcast, [new IPNetwork(new IPAddress([255, 255, 255, 255]), 32)]),
            // RFC3171
            (Ipv4SpecialRange.Multicast, [new IPNetwork(new IPAddress([224, 0, 0, 0]), 4)]),
            // RFC3927
            (Ipv4SpecialRange.LinkLocal, [new IPNetwork(new IPAddress([169, 254, 0, 0]), 16)]),
            // RFC5735
            (Ipv4SpecialRange.Loopback, [new IPNetwork(new IPAddress([127, 0, 0, 0]), 8)]),
            // RFC6598
            (Ipv4SpecialRange.CarrierGradeNat, [new IPNetwork(new IPAddress([100, 64, 0, 0]), 10)]),
            // RFC1918
            (Ipv4SpecialRange.Private, [
                new IPNetwork(new IPAddress([10, 0, 0, 0]), 8),
                new IPNetwork(new IPAddress([172, 16, 0, 0]), 12),
                new IPNetwork(new IPAddress([192, 168, 0, 0]), 16)
            ]),
            // Reserved and testing-only ranges; RFCs 5735, 5737, 2544, 1700
            (Ipv4SpecialRange.Reserved, [
                new IPNetwork(new IPAddress([192, 0, 0, 0]), 24),
                new IPNetwork(new IPAddress([192, 0, 2, 0]), 24),
                new IPNetwork(new IPAddress([192, 88, 99, 0]), 24),
                new IPNetwork(new IPAddress([198, 18, 0, 0]), 15),
                new IPNetwork(new IPAddress([198, 51, 100, 0]), 24),
                new IPNetwork(new IPAddress([203, 0, 113, 0]), 24),
                new IPNetwork(new IPAddress([240, 0, 0, 0]), 4)
            ]),
            // RFC7534, RFC7535
            (Ipv4SpecialRange.As112, [
                new IPNetwork(new IPAddress([192, 175, 48, 0]), 24),
                new IPNetwork(new IPAddress([192, 31, 196, 0]), 24),
            ]),
            // RFC7450
            (Ipv4SpecialRange.Amt, [
                new IPNetwork(new IPAddress([192, 52, 193, 0]), 24),
            ])
        ];

        foreach (var (specialRange, ipNetworks) in ranges)
        {
            foreach (var ipNetwork in ipNetworks)
            {
                if (ipNetwork.Contains(ipAddress))
                {
                    return specialRange;
                }
            }
        }

        return Ipv4SpecialRange.Default;
    }
}
