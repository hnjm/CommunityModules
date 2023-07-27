using Community.UserAccount.Interfaces;
using Libgpgme;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Community.UserAccount.Services
{
    public class GPGService : IGPGService
    {
        private const string BeginPgpPublicKeyString = "-----BEGIN PGP PUBLIC KEY BLOCK-----";
        private const string EndPgpPublicKeyString = "-----END PGP PUBLIC KEY BLOCK-----";

        public bool IsValidGpgPubKey(string publicKey)
        {
            if (string.IsNullOrEmpty(publicKey)) return false;

            publicKey = publicKey.Trim();

            return publicKey.StartsWith(BeginPgpPublicKeyString)
                   && publicKey.EndsWith(EndPgpPublicKeyString);
        }

        public string? ImportGpgPublicKey(string publicKey)
        {
            if (!IsValidGpgPubKey(publicKey)) return null;

            var encoder = new UTF8Encoding();
            var keyData = encoder.GetBytes(publicKey);
            using var memoryStream = new MemoryStream(keyData);
            using var keyStream = new GpgmeStreamData(memoryStream);
            using var ctx = new Context();
            var result = ctx.KeyStore.Import(keyStream);
            return result.Imports?.FirstOrDefault()?.Fpr;
        }

        public string? EncryptMessageForUser(string message, string destinationFingerprint)
        {
            var ctx = new Context();
            ctx.PinentryMode = PinentryMode.Loopback;

            if (ctx.Protocol != Protocol.OpenPGP)
                ctx.SetEngineInfo(Protocol.OpenPGP, null, null);
            var keyring = ctx.KeyStore;
            var key = keyring.GetKeyList(destinationFingerprint, false).FirstOrDefault();
            if (key == null) return null;

            var pgpKey = (PgpKey)key;
            if (pgpKey.Uid == null || pgpKey.Fingerprint == null)
                throw new InvalidKeyException();

            var utf8 = new UTF8Encoding();
            var plain = new GpgmeMemoryData();

            var binaryWriter = new BinaryWriter(plain, utf8);
            binaryWriter.Write(message.ToCharArray());
            binaryWriter.Flush();
            binaryWriter.Seek(0, SeekOrigin.Begin);

            /////// ENCRYPT DATA ///////
            ctx.Armor = true;
            var cipher = new GpgmeMemoryData();
            ctx.Encrypt(
                new Key[] { pgpKey },
                EncryptFlags.AlwaysTrust,
                plain,
                cipher);
            cipher.Seek(0, SeekOrigin.Begin);

            var binaryReader = new BinaryReader(cipher, utf8);
            string encryptedMessage = "";
            while (cipher.CanRead)
            {
                try
                {
                    char[] buf = binaryReader.ReadChars(255);
                    if (buf.Length == 0)
                        break;
                    encryptedMessage += new string(buf);
                }
                catch (EndOfStreamException)
                {
                    break;
                }
            }

            return encryptedMessage;
        }
    }
}
