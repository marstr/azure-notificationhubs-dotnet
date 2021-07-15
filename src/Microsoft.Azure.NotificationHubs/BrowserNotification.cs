//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved. 
// Licensed under the MIT License. See License.txt in the project root for 
// license information.
//----------------------------------------------------------------

using System;
using System.Text;

namespace Microsoft.Azure.NotificationHubs
{
    /// <summary>
    /// Represents a Notification to be pushed to a browser. Structurally, these notifications follow RFC 8030
    /// (Generic Event Delivery Using HTTP Push).
    /// </summary>
    public sealed class BrowserNotification : Notification, INativeNotification
    {
        static string contentType = $"application/json;charset={Encoding.UTF8.WebName}";

        public const string UrgencyVeryLow = "very-low";
        public const string UrgencyLow = "low";
        public const string UrgencyNormal = "normal";
        public const string UrgencyHigh = "high";

        public const string TtlHeader = "TTL";
        public const string UrgencyHeader = "Urgency";
        public const string TopicHeader = "Topic";

        public string Urgency { get; set; } = UrgencyNormal;

        public TimeSpan Ttl { get; set; } = TimeSpan.FromDays(28);

        public string Topic { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Azure.NotificationHubs.BrowserNotification"/> class.
        /// </summary>
        /// <param name="jsonPayload">The JSON payload.</param>
        public BrowserNotification(string jsonPayload) :
            base(null, null, contentType)
        {
            if (string.IsNullOrWhiteSpace(jsonPayload))
            {
                throw new ArgumentNullException("jsonPayload");
            }

            this.Body = jsonPayload;
        }

        /// <summary>
        /// Validate and populates the headers.
        /// </summary>
        protected override void OnValidateAndPopulateHeaders()
        {
            if (Urgency != null)
            {
                this.AddOrUpdateHeader(UrgencyHeader, Urgency);
            }
            else
            {
                throw new ArgumentException("Urgency cannot be null. This notification is invalid.");
            }

            ulong ttlInSeconds;
            try
            {
                ttlInSeconds = checked((ulong)Ttl.TotalSeconds);
            }
            catch (System.OverflowException e)
            {
                ttlInSeconds = ulong.MaxValue;

            }

            this.AddOrUpdateHeader(TtlHeader, ttlInSeconds.ToString());

            if (Topic != null)
            {
                this.AddOrUpdateHeader(TopicHeader, Topic);
            }
        }

        /// <summary>
        /// Gets the type of the platform.
        /// </summary>
        /// <value>
        /// The type of the platform.
        /// </value>
        protected override string PlatformType => BrowserCredential.AppPlatformName;
    }
}
