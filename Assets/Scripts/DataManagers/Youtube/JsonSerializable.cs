﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace YouTubeLive.Json {

    namespace ChannelStatus {
        [Serializable]
        public class SerializedItems {
            public Items[] items;
        }

        [Serializable]
        public class Items {
            public ID id;
        }

        [Serializable]
        public class ID {
            public string videoId;
        }
    }
    namespace LiveStreamingDetails {
        [Serializable]
        public class SerializedItems {
            public Items[] items;
            public Error error;
        }

        [Serializable]
        public class Items {
            public string id;
            public Snippet snippet;
            public LiveStreamingDetails liveStreamingDetails;
        }

        [Serializable]
        public class Snippet {
            public string publishedAt;
            public string channelId;
            public string title;
            public string description;
            public Thumbnails thumbnails;
            public string channelTitle;
            public string liveBroadcastContent;
        }

        [Serializable]
        public class LiveStreamingDetails {
            public string actualStartTime;
            public string concurrentViewers;
            public string activeLiveChatId;
        }
    }

    namespace ChatDetails {
        [Serializable]
        public class SerializedItems {
            public int pollingIntervalMillis;
            public string nextPageToken;
            public Items[] items;
            public Error error;
        }

        [Serializable]
        public class Items {
            public string id;
            public Snippet snippet;
            public AuthorDetails authorDetails;
        }

        [Serializable]
        public class Snippet {
            public enum EventType {
                chatEndedEvent,
                fanFundingEvent,
                messageDeletedEvent,
                newSponsorEvent,
                sponsorOnlyModeEndedEvent,
                sponsorOnlyModeStartedEvent,
                superChatEvent,
                textMessageEvent,
                tombstone,
                userBannedEvent
            }

            public string type;
            public string publishedAt;
            public bool hasDisplayContent;
            public string displayMessage;
            public TextMessageDetails textMessageDetails;
            public SuperChatDetails superChatDetails;
        }

        [Serializable]
        public class AuthorDetails {
            public string channelId;
            public string displayName;
            public string profileImageUrl;
            public bool isVerified;
            public bool isChatOwner;
            public bool isChatSponsor;
            public bool isChatModerator;
        }

        [Serializable]
        public class TextMessageDetails {
            public string messageText;
        }

        [Serializable]
        public class SuperChatDetails {
            public string amountMicros;
            public string currency;
            public string amountDisplayString;
            public int convertedAmount;
            public string userComment;
            public int tier;
        }
    }

    namespace ChannelDetails {
        [Serializable]
        public class SerializedItems {
            public Items[] items;
            public Error error;
        }

        [Serializable]
        public class Items {
            public string id;
            public Statistics statistics;
        }

        [Serializable]
        public class Statistics {
            public string viewCount;
            public string commentCount;
            public string subscriberCount;
            public bool hiddenSubscriberCount;
            public string videoCount;
        }
    }

    namespace Channel {
        [Serializable]
        public class SerializedItems {
            public Items[] items;
        }

        [Serializable]
        public class Items {
            public Snippet snippet;
        }

        [Serializable]
        public class Snippet {
            public string title;
            public Thumbnails thumbnails;
        }

    }

    [Serializable]
    public class Thumbnails {
        public enum ThumbnailsType {
            DEFAULT = 0,
            MEDIUM,
            HIGH,
            STANDARD,
            MAXRES
        }

        public ThumbnailsDetails @default;
        public ThumbnailsDetails medium;
        public ThumbnailsDetails high;
        public ThumbnailsDetails standard;
        public ThumbnailsDetails maxres;
    }

    [Serializable]
    public class ThumbnailsDetails {
        public string url;
        public int width;
        public int hight;
    }

    [Serializable]
    public class Error {
        public Errors[] errors;
        public string code;
        public string message;
    }

    [Serializable]
    public class Errors {
        public string domain;
        public string reason;
        public string message;
    }
}