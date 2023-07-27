using Community.UserAccount.Interfaces;
using Community.UserAccount.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Community.UserAccount.Tests
{
    public class GPGServiceTests
    {
        private IGPGService? _gpgService;
        private readonly string _invalidGpg = "This gpg key is invalid";

        private readonly string _validGpg = @"
-----BEGIN PGP PUBLIC KEY BLOCK-----

mQENBGLumHABCADSGgfKNl3KAeWfHyz1iQ+XiPeGcqB/XAhkQAkl6zwwVGNwyn/X
vXkITk9wIwkQDi3tE6xaMzTEPrjzhVUfLiIJbrIyQHAYLeYmgePDymU4mBwjqCug
kld4YQQdP1JOze6Vv/4CWDfNvAh0A5BKdH6zyiRv0Qz9GFn7YfFxrT7ylhLYkygy
SC4E4Hkh5qIgL8Tgq+lgbWsRyukv2WEbabNKDh5m6M6/gMGnVpPbI2iWWWfYbi45
cRMhPQNOEq7pc6ekYepyPSjyZsCBLgsn6mbIR0M5ju+ZUYZ6SPEmHyVMTztdl8fk
wBl2A/dDIXaS+4CXC0eIT9vaEdPhhbc6oLTrABEBAAG0FFRlc3RlciA8dGVzdGVy
QGhvbWU+iQFOBBMBCgA4FiEEEEJ8N/WEfzqN4ZPaO3akEIuPThcFAmLumHACGwMF
CwkIBwIGFQoJCAsCBBYCAwECHgECF4AACgkQO3akEIuPTheLxggApn7+ocxlvgDV
mmfjNuQ38z52/B3jW54DWfdlWNfvDvdtONrgCd91L6A1ld26PEQJe01hGrSAoWlz
OjEQIuH/TFpotgEq+ziYbs2B86eGPgFv5mIgJxQ38XACytEZY/MyDtenbSq3RXmL
YD4wYTbLEvP51Gxm/v9LJV0FrnjLFA3M0uHmjTtETNl8IXTDmPNut24YaKBeLOsI
lVbAjMd6v/scx8r5YTrTO1vFXdcL7ysFBL3nRnaoB3vzJzBMAcPNQJkvDLLxYawy
8x1Lld7fJ99ReFbIezV8VMclGhFSc8gbZ0v2zWU/AOOTQw/GPaj9Usxrdj+fw+4e
KrHjr+3RB7kBDQRi7phwAQgApGMY0mKz136Wsg+Sc8n77bicrar2M75VSOiNy2Gb
o3WJ3JP2vZ5HJuHh62a7Vd6hL32nj/RD/dKMA+C4COmxzG0LqlNQx8gDIZKbLaD1
CLpwPjkq7gbJEKD69F7RidHqUbvpKYnV+nwKgRf/OiMPjgCsDT4+jxBVNpEjvcEU
SQ2Z3FXMmLozeppNDP4C9bVvB1v2xCbaDHbu8U8S+Rtny9xqJECy4W3C/IOpoAGW
kNZv/vkR2NzhD5RjyjuuyJUqfm8Vls9hvV6luH8p+qtixa0DKLafM+jc+jL/r0Gk
fdIUdTWbtslngueuoZU3Pkzbh6qeFuYwSLMl/5Gtyyk2OwARAQABiQE2BBgBCgAg
FiEEEEJ8N/WEfzqN4ZPaO3akEIuPThcFAmLumHACGwwACgkQO3akEIuPThdJMwgA
rDXul5nrUrlxLJodn5ICAZVpxAlPtz+xqjL3g6ePKAL+zCC8shE0XLU2U2SN/Llc
oARYp63yXhSUFgEJb/pi423O7rfMaDizeX/H/dGpZHJTMt80Wzfky8HQF2zkUadh
VE20Rxf9OSI1GLLf9G8MTP2d3lbRNjK8n2c95MhzGKnw5DrzIZBVzPY1RNVTO8ku
Rpbmu/V4qwkMxvXQOWRmFYEzkj30YGU5jANEskX6YLpseUhoI9slQRxGGgk8YaH3
qtgf67q7GyO9ZqEC9xIHOYTV0WZNSykc9EKQgabUcRUhxqWbl2/sI5uZBkX5kZmp
RadEGo+1QNShb4Zr5B4dcA==
=Ivv8
-----END PGP PUBLIC KEY BLOCK-----
";

        public GPGServiceTests()
        {
            _gpgService = new GPGService();
        }

        [Fact]
        public void InvalidStringReturnsFalse()
        {
            var result = _gpgService!.IsValidGpgPubKey(_invalidGpg);
            Assert.False(result);
        }

        [Fact]
        public void ValidStringReturnsTrue()
        {
            var result = _gpgService!.IsValidGpgPubKey(_validGpg);
            Assert.True(result);
        }

        [Fact]
        public void ValidPgpImportReturnsFingerprint()
        {
            var result = _gpgService!.ImportGpgPublicKey(_validGpg);
            Assert.False(string.IsNullOrEmpty(result));
        }

        [Fact]
        public void InvalidPgpImportReturnsNull()
        {
            var result = _gpgService!.ImportGpgPublicKey(_invalidGpg);
            Assert.True(string.IsNullOrEmpty(result));
        }

        [Fact]
        public void ValidPgpKeyCanEncryptMessage()
        {
            var result = _gpgService!.EncryptMessageForUser("Hello user", "10427C37F5847F3A8DE193DA3B76A4108B8F4E17");
            Assert.NotNull(result);
        }

        [Fact]
        public void CannotEncryptIfNoKeyFound()
        {
            var result = _gpgService!.EncryptMessageForUser("Hello user", "invalid destination");
            Assert.Null(result);
        }
    }
}
