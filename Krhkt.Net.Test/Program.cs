using System;
using System.Net;

namespace Krhkt.Net.Test
{
    public class Program
    {
        static void Main(string[] args)
        {
            TestIPV4CidrParser();
            TestIPV6CidrParser();
            TestSubnetIp();
            
            Examples();

            Console.ReadKey();
        }

        #region [ TEST AREA ]
        static void TestIPV4CidrParser()
        {
            PrintTestHeader("Valid CIDR IPv4 should NOT throw on parse");
            try
            {
                var cidrIpv4 = CidrIpAddress.Parse("255.255.255.255/32");

                PrintPass();
            }
            catch (Exception e)
            {
                PrintFail("Unexpected exception: " + e.Message);
            }

            PrintTestFooter();
        }

        static void TestIPV6CidrParser()
        {
            PrintTestHeader("Valid CIDR IPv6 should NOT throw on parse");
            try
            {
                var cidrIpv6 = CidrIpAddress.Parse("::/128");

                PrintPass();
            }
            catch (Exception e)
            {
                PrintFail("Unexpected exception: " + e.Message);
            }

            PrintTestFooter();
        }

        static void TestSubnetIp()
        {
            PrintTestHeader("Full bit mask should check all IP parts in order to be considered of the same subnet");
            var cidrIpv4Full = CidrIpAddress.Parse("186.177.128.128/32");

            if (cidrIpv4Full.IsIpFromSameSubnet("186.177.128.129")) PrintFail("Ip expected NOT to be on the same subnet");
            else PrintPass();
            PrintTestFooter();
        }

        static void Examples()
        {
            Console.WriteLine("EXAMPLES:\n");
            var emptyIpV6 = IPAddress.Parse("::");
            var emptyCidr = new CidrIpAddress(emptyIpV6, 0);
            Console.WriteLine("empty IPV6 formatted: {0}\n", emptyCidr.ToString()); 

            var cidrIpAll = CidrIpAddress.Parse("0.0.0.0/0");
            var ipV4 = IPAddress.Parse("128.192.128.65");
            Console.WriteLine("[CIDR IP all]: {0}\n[         IP]: {1}\n  same subnet: {2}\n",
                cidrIpAll, ipV4,
                cidrIpAll.IsIpFromSameSubnet(ipV4));

            var cidrIp = CidrIpAddress.Parse("128.192.164.32/17");
            Console.WriteLine("[CIDR IP all]: {0}\n[    CIDR IP]: {1}\n  same subnet: {2}\n",
                cidrIpAll, cidrIp,
                cidrIpAll.IsIpFromSameSubnet(cidrIp));

            Console.WriteLine(cidrIp.ToBinaryString());

            var cidrIpSingle = CidrIpAddress.Parse("186.177.128.128/32"); //even the last bit is important when checking
            var ipSingle = IPAddress.Parse("186.177.128.129");
            Console.WriteLine("[    CIDR IP]: {0}\n[    CIDR IP]: {1}\n  same subnet: {2}\n",
                cidrIpSingle, ipSingle,
                cidrIpSingle.IsIpFromSameSubnet(ipSingle));

            Console.WriteLine("[    CIDR IP]: {0}\n[         IP]: {1}\n  same subnet: {2}\n",
                cidrIp, ipV4,
                cidrIp.IsIpFromSameSubnet(ipV4));

            var cidrIpV6 = CidrIpAddress.Parse("2001:0db8:0000:0042:0000:8a2e:0370:7334/64");
            var ipV6 = IPAddress.Parse("2001:0db8:0000:0042:0000:8a2e:0370:7334");
            Console.WriteLine("[  CIDR IPV6]: {0}\n[       IPV6]: {1}\n  same subnet: {2}\n",
                cidrIpV6, ipV6,
                cidrIpV6.IsIpFromSameSubnet(ipV6));

            Console.WriteLine("[    CIDR IP]: {0}\n[       IPV6]: {1}\n  same subnet: {2}\n",
                cidrIp, ipV6,
                cidrIp.IsIpFromSameSubnet(ipV6));
            Console.WriteLine("--------");

            var cidrIp2 = CidrIpAddress.Parse("128.192.128.65/20");
            Console.WriteLine("Binary check v4:\n{0}", cidrIp2);
            Console.WriteLine(cidrIp2.ToBinaryString());

            
            Console.WriteLine("\nBinary check v6:\n{0}", cidrIpV6);
            Console.WriteLine(cidrIpV6.ToBinaryString());
            Console.WriteLine("--------");
        }
        #endregion

        #region [ TEST OUTPUTS ]
        static void PrintTestHeader(string name)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("Test: " + name);
        }

        static void PrintTestFooter()
        {
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("\n");
        }

        static void PrintFail(string message)
        {
            var oldBgColor = Console.BackgroundColor;
            var oldFgColor = Console.ForegroundColor;

            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.ForegroundColor = ConsoleColor.White;

            Console.Write("[FAIL]");
            Console.BackgroundColor = oldBgColor;
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine(string.Format(" {0}", message));

            Console.ForegroundColor = oldFgColor;
        }

        static void PrintPass()
        {
            var oldFgColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Green;

            Console.Write("[passed]");

            Console.ForegroundColor = oldFgColor;
        }
        #endregion
    }
}
