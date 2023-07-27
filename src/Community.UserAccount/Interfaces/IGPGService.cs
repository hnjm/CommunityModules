using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Community.UserAccount.Interfaces
{
    public interface IGPGService
    {
        public bool IsValidGpgPubKey(string publicKey);
        public string? ImportGpgPublicKey(string publicKey);
        public string? EncryptMessageForUser(string message, string destinationFingerprint);

    }
}
