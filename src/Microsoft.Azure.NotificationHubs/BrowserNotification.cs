using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Azure.NotificationHubs
{
    /// <summary>
    /// Represents a Notification to be pushed to a browser. Structurally, these notifications follow RFC 8030
    /// (Generic Event Delivery Using HTTP Push).
    /// </summary>
    public class BrowserNotification : Notification, INativeNotification
    {
        public const string UrgencyVeryLow = "very-low";
        public const string UrgencyLow = "low";
        public const string UrgencyNormal = "normal";
        public const string UrgencyHigh = "high";

        public const string TtlHeader= "TTL";
        public const string UrgencyHeader = "Urgency";
        public const string TopicHeader = "Topic";

        public string Urgency { get; set; } = UrgencyNormal;

        public TimeSpan Ttl { get; set; } = TimeSpan.FromDays(28);
        
        public string Topic { get; set; }

        public BrowserNotification(IDictionary<string, string> additionalHeaders, string tag, string contentType) : 
            base(additionalHeaders, tag, contentType)
        {
            // TODO: replace this constructor with a more sensible one. 
        }

        protected override void OnValidateAndPopulateHeaders()
        {
            if (Urgency != null)
            {
                // TODO: if Urgency is null, indicate that this Notification is invalid.
                this.AddOrUpdateHeader(UrgencyHeader, Urgency);   
            }

            var ttlInSeconds = (ulong) Ttl.TotalSeconds; // TODO: If this would overflow, cap to ulong.Max. It's clients responsibility to deal with capping at lower number if necessary.
            this.AddOrUpdateHeader(TtlHeader, ttlInSeconds.ToString());

            if (Topic != null)
            {
                this.AddOrUpdateHeader(TopicHeader, Topic);
            }
        }

        protected override string PlatformType => BrowserCredential.AppPlatformName;
    }
}
