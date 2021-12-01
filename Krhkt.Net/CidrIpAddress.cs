using System;
using System.Net;
using System.Net.Sockets;

namespace Krhkt.Net
{
    public class CidrIpAddress
    {
        const char CIDR_SEPARATOR = '/';
        const byte IPV6_BIT_LENGTH = 128;
        const byte IPV4_BIT_LENGTH = 32;
        const byte BYTE_MASK = 0b11111111;

        const string FormatExceptionMessage = "cidrAddressString is not a valid CIDR address";

        public IPAddress IpAddress { get; protected set; }
        public IPAddress SubnetIp { get; protected set; }
        private byte[] SubnetIpMask { get; set; }
        private int SubnetBitLengthMask { get; set; }

        #region [ constructors ]
        /// <summary>
        /// Initialize a new instance by Ip and subnet mask
        /// </summary>
        public CidrIpAddress(IPAddress ip, int subnetBitLengthMask)
        {
            IpAddress = ip;
            SubnetBitLengthMask = subnetBitLengthMask;

            CalculateSubnetIpAddress();
        }

        /// <summary>
        /// Initialize a new instance by CidrIpAddress and subnet mask.
        /// Useful for changing subnet bit length mask from an existing CidrIpAddress
        /// </summary>
        public CidrIpAddress(CidrIpAddress cidr, int subnetBitLengthMask)
        {
            IpAddress = cidr.IpAddress;
            SubnetBitLengthMask = subnetBitLengthMask;

            CalculateSubnetIpAddress();
        }

        private CidrIpAddress() { }

        /// <summary>
        /// Parse a cidr ip string and returns an instance of the CidrIpAddress
        /// </summary>
        /// <param name="cidrAddressString">Cidr formatted string using / to split ip and subnet. If no subnet is specified, the subnet mask will be full</param>
        public static CidrIpAddress Parse(string cidrAddressString)
        {
            if (cidrAddressString == null) throw new ArgumentNullException("cidrAddress");

            var pieces = cidrAddressString.Trim().Split(CIDR_SEPARATOR);
            if (pieces.Length > 2) throw new FormatException(FormatExceptionMessage);

            var ip = pieces[0];
            var subnetMask = pieces[1];

            var cidr = new CidrIpAddress
            {
                //throws FormatException if string is not an IP
                IpAddress = IPAddress.Parse(ip)
            };

            if (pieces.Length == 2)
            {
                cidr.SubnetBitLengthMask = Convert.ToInt32(subnetMask);

                //checking subnet ranges
                if (cidr.SubnetBitLengthMask < 0) throw new FormatException(FormatExceptionMessage);

                if (cidr.IpAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (cidr.SubnetBitLengthMask > IPV4_BIT_LENGTH) throw new FormatException(FormatExceptionMessage);
                }
                else if (cidr.SubnetBitLengthMask > IPV6_BIT_LENGTH) throw new FormatException(FormatExceptionMessage);
            }
            else
            {
                cidr.SubnetBitLengthMask = (cidr.IpAddress.AddressFamily == AddressFamily.InterNetwork)
                    ? IPV4_BIT_LENGTH : IPV6_BIT_LENGTH;
            }

            cidr.SubnetIpMask = new byte[((cidr.IpAddress.AddressFamily == AddressFamily.InterNetwork)
                    ? IPV4_BIT_LENGTH : IPV6_BIT_LENGTH) / 8];

            cidr.CalculateSubnetIpAddress();

            return cidr;
        }
        #endregion


        #region [ interface ]
        /// <summary>
        /// Check if the ip belongs to the subnet of this instance
        /// </summary>
        public bool IsIpFromSameSubnet(IPAddress ip)
        {
            if (ip == null) return false;

            if (SubnetIp.AddressFamily == AddressFamily.InterNetworkV6)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ip = ip.MapToIPv6();
                }
            }
            else
            {
                //will not map IPv6 to IPv4 in this case
                if (ip.AddressFamily != AddressFamily.InterNetwork) return false;
            }

            var maskedIp = ApplyMask(ip.GetAddressBytes());

            var subnetIp = SubnetIp.GetAddressBytes();

            for (var i = 0; i < subnetIp.Length; i += 1)
            {
                if (subnetIp[i] != maskedIp[i]) return false;
            }

            //if IPV6, the scopeId should be the same
            //to be considered of the same subnet
            if ((SubnetIp.AddressFamily == AddressFamily.InterNetworkV6)
                && (SubnetIp.ScopeId != ip.ScopeId))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if the cidrIp belongs to the subnet of this instance
        /// </summary>
        public bool IsIpFromSameSubnet(CidrIpAddress cidrIp)
        {
            if (cidrIp == null) return false;

            return IsIpFromSameSubnet(cidrIp.IpAddress);
        }

        /// <summary>
        /// Checks if the ip belongs to the subnet of this instance
        /// </summary>
        public bool IsIpFromSameSubnet(string ip)
        {
            if (!IPAddress.TryParse(ip, out var parsedIp)) return false;

            return IsIpFromSameSubnet(parsedIp);
        }

        /// <summary>
        /// Return a string formatted in CIDR notation
        /// </summary>
        public override string ToString()
        {
            return IpAddress.ToString() + CIDR_SEPARATOR + SubnetBitLengthMask;
        }

        /// <summary>
        /// Return a 3 line string containing the IP in bits, the mask in bits, and the subnet IP in bits.
        /// Useful for debugging
        /// </summary>
        public string ToBinaryString()
        {
            string binaryIp = "";
            var binaryIpBytes = IpAddress.GetAddressBytes();
            for (var i = 0; i < SubnetIpMask.Length; i += 1)
            {
                binaryIp += Convert.ToString(binaryIpBytes[i], 2).PadLeft(8, '0')
                    + (((i + 1) == binaryIpBytes.Length) ? "" : ".");
            }

            string binaryMask = "";
            for (var i = 0; i < SubnetIpMask.Length; i += 1)
            {
                binaryMask += Convert.ToString(SubnetIpMask[i], 2).PadLeft(8, '0')
                    + (((i + 1) == SubnetIpMask.Length) ? "" : ".");
            }

            string binarySubnetIp = "";
            var subnetIpBytes = SubnetIp.GetAddressBytes();
            for (var i = 0; i < SubnetIpMask.Length; i += 1)
            {
                binarySubnetIp += Convert.ToString(subnetIpBytes[i], 2).PadLeft(8, '0')
                    + (((i + 1) == subnetIpBytes.Length) ? "" : ".");
            }

            return string.Format("ip:{0}\nmk:{1}\nsi:{2}", binaryIp, binaryMask, binarySubnetIp);
        }
        #endregion


        private void CalculateSubnetIpAddress()
        {
            var ipBytes = IpAddress.GetAddressBytes();
            var subnetIp = new byte[ipBytes.Length];
            SubnetIpMask = new byte[ipBytes.Length];

            var bytesMask = SubnetBitLengthMask / 8;
            var remainingBits = SubnetBitLengthMask % 8;

            for (var i = 0; i < bytesMask; i += 1)
            {
                SubnetIpMask[i] = BYTE_MASK;
                subnetIp[i] = (byte)(BYTE_MASK & ipBytes[i]);
            }

            if (remainingBits > 0)
            {
                var bitMask = (byte)(BYTE_MASK ^ (BYTE_MASK >> remainingBits));
                SubnetIpMask[bytesMask] = bitMask;
                subnetIp[bytesMask] = (byte)(bitMask & ipBytes[bytesMask]);
            }

            for (var i = bytesMask + 1; i < ipBytes.Length; i += 1)
            {
                SubnetIpMask[i] = 0;
                subnetIp[i] = 0;
            }

            SubnetIp = new IPAddress(subnetIp);
        }

        private byte[] ApplyMask(byte[] ip)
        {
            var maskedIp = new byte[ip.Length];

            for (var i = 0; i < ip.Length; i += 1)
            {
                maskedIp[i] = (byte)(ip[i] & SubnetIpMask[i]);
            }

            return maskedIp;
        }
    }
}
