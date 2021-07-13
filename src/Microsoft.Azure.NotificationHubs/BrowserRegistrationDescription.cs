using Microsoft.Azure.NotificationHubs.Messaging;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.Azure.NotificationHubs
{
    [DataContract(Name = ManagementStrings.BrowserRegistrationDescription, Namespace = ManagementStrings.Namespace)]
    public class BrowserRegistrationDescription : RegistrationDescription
    {
        private const string EndpointProperty = "endpoint";
        private const string P256DHProperty = "p256dh";
        private const string AuthProperty = "auth";

        public BrowserRegistrationDescription(string endpoint, string p256dh, string auth)
            : this(string.Empty, endpoint, p256dh, auth, null)
        {
            // Intentionally Left Blank
        }

        public BrowserRegistrationDescription(string endpoint, string p256dh, string auth, IEnumerable<string> tags)
            : this(string.Empty, endpoint, p256dh, auth, tags)
        {
            // Intentionally Left Blank
        }

        internal BrowserRegistrationDescription(string notificationHubPath, string endpoint, string p256dh, string auth, IEnumerable<string> tags)
            : base(notificationHubPath)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            if (string.IsNullOrWhiteSpace(p256dh))
            {
                throw new ArgumentNullException(nameof(p256dh));
            }

            if (string.IsNullOrWhiteSpace(auth))
            {
                throw new ArgumentNullException(nameof(auth));
            }

            Endpoint = endpoint;
            P256DH = p256dh;
            Auth = auth;
            if (tags != null)
            {
                Tags = new HashSet<string>(tags);
            }
        }

        public BrowserRegistrationDescription(BrowserRegistrationDescription sourceRegistration)
    : base(sourceRegistration)
        {
            Endpoint = sourceRegistration.Endpoint;
            P256DH = sourceRegistration.P256DH;
            Auth = sourceRegistration.Auth;
        }

        [DataMember(Name = ManagementStrings.BrowserEndpoint, Order = 2001, IsRequired = true)]
        public string Endpoint { get; set; }

        [DataMember(Name = ManagementStrings.BrowserP256DH, Order = 2002, IsRequired = true)]
        public string P256DH { get; set; }

        [DataMember(Name = ManagementStrings.BrowserAuth, Order = 2003, IsRequired = true)]
        public string Auth { get; set; }

        internal override string AppPlatForm => BrowserCredential.AppPlatformName;

        internal override string RegistrationType => BrowserCredential.AppPlatformName;

        internal override string PlatformType => BrowserCredential.AppPlatformName;

        internal override void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(Endpoint))
            {
                throw new InvalidDataContractException(SRClient.BrowserEndpointInvalid);
            }

            if (string.IsNullOrWhiteSpace(P256DH))
            {
                throw new InvalidDataContractException(SRClient.BrowserP256DHInvalid);
            }

            if (string.IsNullOrWhiteSpace(Auth))
            {
                throw new InvalidDataContractException(SRClient.BrowserPushAuthInvalid);
            }

            base.OnValidate();
        }

        internal override RegistrationDescription Clone()
        {
            return new BrowserRegistrationDescription(this);
        }

        internal override string GetPnsHandle()
        {
            using (var buffer = new MemoryStream())
            using (var base64Encoder = new CryptoStream(buffer, new ToBase64Transform(), CryptoStreamMode.Write))
            using (var writer = new StreamWriter(base64Encoder))
            using (var jsonWriter = new JsonTextWriter(writer) { Indentation = 0 })
            {
                // Why doesn't this use `JsonSerializer.Serialize`? In a word: determinism
                // By controlling the order in which these properties are written ourselves, we avoid a potentially
                // breaking change should .NET change the order the Json Serializer spits out properties.
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(EndpointProperty);
                jsonWriter.WriteValue(Endpoint);
                jsonWriter.WritePropertyName(P256DHProperty);
                jsonWriter.WriteValue(P256DH);
                jsonWriter.WritePropertyName(AuthProperty);
                jsonWriter.WriteValue(Auth);
                jsonWriter.WriteEndObject();
                jsonWriter.Flush();
                base64Encoder.FlushFinalBlock();

                buffer.Seek(0, SeekOrigin.Begin);
                return Encoding.UTF8.GetString(buffer.ToArray());
            }
        }

        internal override void SetPnsHandle(string pnsHandle)
        {
            var jsonBytes = Convert.FromBase64String(pnsHandle);
            var jsonString = Encoding.UTF8.GetString(jsonBytes);
            var result = JsonConvert.DeserializeObject<BrowserRegistrationDescription>(jsonString);
            if (result == null)
            {
                throw new ArgumentException(paramName: nameof(pnsHandle), message: "expected a base64 encoded UTF-8 JSON object");
            }

            Auth = result.Auth;
            Endpoint = result.Endpoint;
            P256DH = result.P256DH;
        }
    }
}
