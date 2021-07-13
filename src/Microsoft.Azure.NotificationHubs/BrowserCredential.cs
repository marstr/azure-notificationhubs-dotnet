using Microsoft.Azure.NotificationHubs.Messaging;
using System.Runtime.Serialization;

namespace Microsoft.Azure.NotificationHubs
{
    [DataContract(Name = ManagementStrings.BrowserCredential, Namespace = ManagementStrings.Namespace)]

    public class BrowserCredential : PnsCredential
    {
        internal const string AppPlatformName = "browser";

        internal override string AppPlatform
        {
            get
            {
                return AppPlatformName;
            }
        }

        public string Subject
        {
            get { return base[nameof(Subject)]; }
            set { base[nameof(Subject)] = value; }
        }

        public string VapidPublicKey
        {
            get { return base[nameof(VapidPublicKey)]; }
            set { base[nameof(VapidPublicKey)] = value; }
        }

        public string VapidPrivateKey
        {
            get { return base[nameof(VapidPrivateKey)]; }
            set { base[nameof(VapidPrivateKey)] = value; }
        }

        public long JwtTtl
        {
            get
            {
                var expiration = base[nameof(JwtTtl)];
                if (long.TryParse(expiration, out var ttl))
                {
                    return ttl;
                }
                return -1;
            }
            set { base[nameof(JwtTtl)] = value.ToString(); }
        }

        protected override void OnValidate(bool allowLocalMockPns)
        {

        }

        public override bool Equals(object other)
        {
            var browserCredential = other as BrowserCredential;
            if (browserCredential == null)
            {
                return false;
            }
            return Subject.Equals(browserCredential.Subject)
                && VapidPrivateKey.Equals(browserCredential.VapidPrivateKey)
                && VapidPublicKey.Equals(browserCredential.VapidPublicKey)
                && JwtTtl == browserCredential.JwtTtl;
        }

        public override int GetHashCode()
        {
            return Subject.GetHashCode() ^ VapidPrivateKey.GetHashCode() ^ VapidPublicKey.GetHashCode() ^ JwtTtl.GetHashCode();
        }
    }
}
