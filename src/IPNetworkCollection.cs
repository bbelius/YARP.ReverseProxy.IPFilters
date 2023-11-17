using System.Net;

using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

namespace BBelius.Yarp.ReverseProxy.IPFilters;

/// <summary>
/// Trie based IPNetwork collection.
/// </summary>
public class IPNetworkCollection
{
	private readonly TrieNode _root = new();

	/// <summary>
	/// Contructor
	/// </summary>
	/// <param name="ipNetworks">Collection of IPNetwork</param>
	public IPNetworkCollection(IEnumerable<IPNetwork> ipNetworks)
	{
		foreach (var network in ipNetworks)
		{
			Add(network);
		}
	}

	/// <summary>
	/// Checks if the IPNetwork collection contains the specified IP address.
	/// </summary>
	/// <param name="ipAddress">IPAddress to check</param>
	/// <returns>True if Address is in one of the IPNetworks in collection</returns>
	public bool Contains(IPAddress ipAddress)
	{
		var ipAddressBytes = ipAddress.GetAddressBytes();

		TrieNode currentNode = _root;
		for (int i = 0; i < ipAddressBytes.Length * 8; i++)
		{
			int bit = GetBit(ipAddressBytes, i);
			if (currentNode.Children[bit] == null)
			{
				return false;
			}
			currentNode = currentNode.Children[bit];
			if (currentNode.IsTerminal)
			{
				return true;
			}
		}
		return false;
	}

	private void Add(IPNetwork network)
	{
		var ipAddressBytes = network.Prefix.GetAddressBytes();
		var prefixLength = network.PrefixLength;

		TrieNode currentNode = _root;
		for (int i = 0; i < prefixLength; i++)
		{
			int bit = GetBit(ipAddressBytes, i);
			if (currentNode.Children[bit] == null)
			{
				currentNode.Children[bit] = new TrieNode();
			}
			currentNode = currentNode.Children[bit];
		}
		currentNode.IsTerminal = true;
	}

	private static int GetBit(byte[] bytes, int index)
	{
		int byteIndex = index / 8;
		int bitIndex = index % 8;
		return (bytes[byteIndex] >> (7 - bitIndex)) & 1;
	}

	private class TrieNode
	{
		public TrieNode[] Children { get; } = new TrieNode[2];
		public bool IsTerminal { get; set; }
	}
}
