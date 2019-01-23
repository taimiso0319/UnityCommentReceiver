using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouTubeLive.Json.LiveStreamingDetails;
namespace YouTubeLive {
	public class LiveStatus : MonoBehaviour {
		[Header ("ライブ情報")]
		public string videoId;
		public string liveTitle;
		public string liveDescription;
		public string liveConcurrentViewers;
		public string livePublishedAt;
		public string liveBroadcastContent = "none";
		public ThumbnailsDetails[] liveThumbnails = new ThumbnailsDetails[5];
		public string liveActualStartedAt;

		[Header ("チャンネル情報")]
		public string channelId;
		public string channelTitle;
		public bool hiddenSubscriberCount;
		public string subscriberCount;
		public string viewCount;
		public string videoCount;

		public void SetLiveStreamingStatus (Json.LiveStreamingDetails.SerializedItems serializedItems) {
			Json.LiveStreamingDetails.Items items = serializedItems.items[0];
			videoId = items.id;
			channelTitle = items.snippet.channelTitle;
			liveTitle = items.snippet.title;
			liveDescription = items.snippet.description;
			livePublishedAt = items.snippet.publishedAt;
			liveThumbnails[(int) Thumbnails.ThumbnailsType.DEFAULT] = items.snippet.thumbnails.@default;
			liveThumbnails[(int) Thumbnails.ThumbnailsType.MEDIUM] = items.snippet.thumbnails.medium;
			liveThumbnails[(int) Thumbnails.ThumbnailsType.HIGH] = items.snippet.thumbnails.high;
			liveThumbnails[(int) Thumbnails.ThumbnailsType.MAXRES] = items.snippet.thumbnails.maxres;
			liveThumbnails[(int) Thumbnails.ThumbnailsType.STANDARD] = items.snippet.thumbnails.standard;
			liveBroadcastContent = items.snippet.liveBroadcastContent;
		}

		public void SetLiveStreamingDetails (Json.LiveStreamingDetails.SerializedItems serializedItems) {
			liveActualStartedAt = serializedItems.items[0].liveStreamingDetails.actualStartTime;
			liveConcurrentViewers = serializedItems.items[0].liveStreamingDetails.concurrentViewers;
		}

		public void SetChannelDetails (Json.ChannelDetails.SerializedItems serializedItems) {
			channelId = serializedItems.items[0].id;
			hiddenSubscriberCount = serializedItems.items[0].statistics.hiddenSubscriberCount;
			subscriberCount = serializedItems.items[0].statistics.subscriberCount;
			viewCount = serializedItems.items[0].statistics.viewCount;
			videoCount = serializedItems.items[0].statistics.videoCount;
		}
	}
}