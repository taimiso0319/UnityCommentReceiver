using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouTubeLive {

	public static class APIData {
		public static string apiKey = "";
		public static string channelId = "";
		public static string chatId = "";
		public static string videoId = "";
		public static string nextPageToken = null;

		private static string baseURI = "https://www.googleapis.com/youtube/v3/";
		private static string apiKeyBase = "&key=";
		private static string videos = "videos?";
		private static string search = "search?";
		private static string channels = "channels?";
		private static string liveChatMessages = "liveChat/messages?";

		private static string partId = "part=id";
		private static string partSnippet = "part=snippet";
		private static string partLiveDetails = "part=liveStreamingDetails";
		private static string partSnippetAuthorDetails = "part=snippet,authorDetails";
		private static string partStatistics = "part=statistics";

		private static string byId = "&id=";
		private static string byChannelId = "&channelId=";
		private static string byLiveChatId = "&liveChatId=";

		private static string searchType = "&eventType=live&type=video";
		private static string pageToken = "&pageToken=";

		/// <summary>
		/// ChannelId VideoId ChatId NextPageTokenを削除します。
		/// </summary>
		public static void InitializeData () {
			channelId = "";
			chatId = "";
			videoId = "";
			nextPageToken = null;
		}

		public static void SetAPIKey (string key) {
			apiKey = key;
		}

		public static string VideoDetailsURI () {
			if (apiKey.Equals ("") || videoId.Equals ("")) { return null; }
			return baseURI + videos + partSnippet + byId + videoId + searchType + apiKeyBase + apiKey;
		}

		public static string ChannelURI (string id) {
			if (apiKey.Equals ("")) { return null; }
			return baseURI + channels + partSnippet + byId + id + apiKeyBase + apiKey;
		}

		public static string ChannelDetailsURI () {
			if (apiKey.Equals ("") || channelId.Equals ("")) { return null; }
			return baseURI + channels + partStatistics + byId + channelId + apiKeyBase + apiKey;
		}

		public static string SearchVideoURI () {
			if (apiKey.Equals ("") || channelId.Equals ("")) { return null; }
			return baseURI + search + partId + byChannelId + channelId + searchType + apiKeyBase + apiKey;
		}

		public static string SearchChatURI () {
			if (apiKey.Equals ("") || videoId.Equals ("")) { return null; }
			return baseURI + videos + partLiveDetails + byId + videoId + apiKeyBase + apiKey;
		}

		public static string ChatURI () {
			if (apiKey.Equals ("") || chatId.Equals ("")) { return null; }
			return baseURI + liveChatMessages + partSnippetAuthorDetails + byLiveChatId + chatId + pageToken + nextPageToken + apiKeyBase + apiKey;
		}

		public static void DebugShowData () {
			Debug.Log (apiKey);
			Debug.Log (channelId);
			Debug.Log (chatId);
			Debug.Log (videoId);
		}
	}
}