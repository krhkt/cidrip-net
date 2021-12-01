*[last update: 2018-12-09]*
# CIDR IP Notation Handler
 
C# implementation that wraps the IPAddress to support CIDR notation ([Classless Inter-Domain Routing](https://en.wikipedia.org/wiki/Classless_Inter-Domain_Routing)), variable-length subnet masking and IP calculation.

### Examples
```c#
    //creating from string
    var cidrIpv4 = CidrIpAddress.Parse("255.255.255.255/32");
    var cidrIpv6 = CidrIpAddress.Parse("::/128");

    //testing subnet
    var cidrIp = CidrIpAddress.Parse("128.192.164.32/17");
    var ip = IPAddress.Parse("128.192.128.65");
    //compatible with the standard IPAddress .NET class
    cidrIp.IsIpFromSameSubnet(ip);
    // return: true

    var cidrIp2 = CidrIpAddress.Parse("128.192.128.65/20");
    cidrIp.IsIpFromSameSubnet(cidrIp2);
    // return: true

    var ip2 = IPAddress.Parse("128.129.128.65");
    cidrIp.IsIpFromSameSubnet(ip3);
    // return: false


    //to CIDR notation
    cidrIp.ToString();
    // return: 128.192.164.32/17

    //debugging/logging
    cidrIp.ToBinaryString();
    // return: ip:10000000.11000000.10100100.00100000 (IP)
    //         mk:11111111.11111111.10000000.00000000 (MASK)
    //         si:10000000.11000000.10000000.00000000 (SUBNET IP)

```