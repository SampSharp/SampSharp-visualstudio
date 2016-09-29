using System.Linq;
using System.Net;

namespace SampSharp.VisualStudio.Utils
{
	public static class HostUtils
	{
		public static IPAddress ResolveHostOrIpAddress(string hostOrIpAddress)
		{
			IPAddress result;
			if (IPAddress.TryParse(hostOrIpAddress, out result))
				return result;
			return Dns.GetHostEntry(hostOrIpAddress).AddressList.First();
		}
	}
}