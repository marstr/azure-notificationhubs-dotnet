//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved. 
// Licensed under the MIT License. See License.txt in the project root for 
// license information.
//----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Azure.NotificationHubs
{
    /// <summary>
    /// Represents a Notification to be pushed to a browser. Structurally, these notifications follow RFC 8030
    /// (Generic Event Delivery Using HTTP Push).
    /// </summary>
    public sealed class BrowserNotification : Notification, INativeNotification
    {
        public const string UrgencyVeryLow = "very-low";
        public const string UrgencyLow = "low";
        public const string UrgencyNormal = "normal";
        public const string UrgencyHigh = "high";

        public const string TtlHeader= "TTL";
        public const string UrgencyHeader = "Urgency";
        public const string TopicHeader = "Topic";

        private static readonly ISet<char> ValidTopicCharacters;
        private static readonly TimeSpan LargestAcceptableTimespan = TimeSpan.FromSeconds(ulong.MaxValue);
        private static readonly ISet<string> ValidUrgency;

        /// <summary>
        /// Indicates how time sensitive a notification is.
        /// </summary>
        /// <remarks>
        /// The intention is to allow batching or delay of notifications when the target device is trying to preserve
        /// battery.
        ///
        /// This field is corresponds to IETF RFC8030 Section 5.3
        /// </remarks>
        public string Urgency { get; set; } = UrgencyNormal;
        
        /// <summary>
        /// The amount of time that this notification is relevant, before it should be abandoned by push delivery
        /// services.
        /// </summary>
        /// <remarks>
        /// This field corresponds to IETF RFC8030 Section 5.2
        /// </remarks>
        public TimeSpan Ttl { get; set; } = TimeSpan.FromDays(28);
        
        /// <summary>
        /// Optional: A handle that will allow for the replacement of previously sent notifications.
        /// </summary>
        /// <remarks>
        /// This field corresponds to IETF RFC8030 Section 5.4
        /// </remarks>
        public string Topic { get; set; }

        static BrowserNotification()
        {
            const string validCharacters = @"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_=";
            ValidTopicCharacters = new HashSet<char>();
            foreach (var c in validCharacters)
            {
                ValidTopicCharacters.Add(c);
            }

            ValidUrgency = new SortedSet<string>
            {
                UrgencyVeryLow, 
                UrgencyLow, 
                UrgencyNormal, 
                UrgencyHigh
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserNotification"/> class with no body.
        /// </summary>
        public BrowserNotification()
            : this(null, null)
        {
            // Intentionally Left Blank
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserNotification"/> class with the specified body, and the
        /// name of the web encoding it uses. 
        /// </summary>
        /// <param name="body">The payload of the notification.</param>
        /// <param name="contentType">The format of the provided <paramref name="body"/>, e.g. "application/json" or "text/plain".</param>
        public BrowserNotification(string body, string contentType)
            : this(body, contentType, null)
        {
            // Intentionally Left Blank
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserNotification"/> class with the specified body, and the
        /// name of the web encoding it uses. 
        /// </summary>
        /// <param name="body">The payload of the notification.</param>
        /// <param name="contentType">The format of the provided <paramref name="body"/>, e.g. "application/json" or "text/plain".</param>
        /// <param name="additionalHeaders"></param>
        public BrowserNotification(string body, string contentType, IDictionary<string, string> additionalHeaders)
            : base(additionalHeaders, null, contentType)
        {
            Body = body;
            if (!contentType.Contains("charset="))
            {
                ContentType += $";charset={Encoding.UTF8.WebName}";
            }
        }

        /// <summary>
        /// Validate and populate headers.
        /// </summary>
        protected override void OnValidateAndPopulateHeaders()
        {
            ValidateAndPopulateUrgency();
            ValidateAndPopulateTTl();
            ValidateAndPopulateTopic();
        }

        private void ValidateAndPopulateUrgency()
        {
            if (Urgency != null)
            {
                if (!ValidUrgency.Contains(Urgency))
                {
                    throw new InvalidDataContractException(SRClient.BrowserUrgencyUnrecognized);
                }
                AddOrUpdateHeader(UrgencyHeader, Urgency);
            }
        }

        protected override string PlatformType => BrowserCredential.AppPlatformName;

        private void ValidateAndPopulateTTl()
        {
            if ( Ttl < TimeSpan.Zero)
            {
                throw new InvalidDataContractException(SRClient.BrowserTtlDeserializationError);
            }

            if (Ttl > LargestAcceptableTimespan)
            {
                Ttl = LargestAcceptableTimespan;
            }

            var ttlInSeconds = (ulong) Ttl.TotalSeconds;
            AddOrUpdateHeader(TtlHeader, ttlInSeconds.ToString());
        }

        private void ValidateAndPopulateTopic()
        {
            if (!string.IsNullOrEmpty(Topic))
            {
                if (Topic.Length > 32)
                {
                    throw new InvalidDataContractException(SRClient.BrowserTopicTooLong);
                }

                foreach (var c in Topic)
                {
                    if (!ValidTopicCharacters.Contains(c))
                    {
                        throw new InvalidDataContractException(string.Format(SRClient.BrowserTopicInvalidCharacter, c));
                    }
                }

                AddOrUpdateHeader(TopicHeader, Topic);
            }
        }
    }
}
