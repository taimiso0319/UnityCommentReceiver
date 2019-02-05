using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

namespace YouTubeLive {
	public class ListenersData {

		private List<ListenerData> listenerDataList;

		public ListenersData () {
			listenerDataList = new List<ListenerData> ();
		}

		public List<ListenerData> GetList () {
			return listenerDataList;
		}

		public void UpdateListenerDataRoyal (string channelId) {
			if (listenerDataList.Count == 0) {
				return;
			}
			listenerDataList.Where (d => d.channelId == channelId).First ().isRoyal = true;
		}

		public bool IsListenerDataRoyal (string channelId) {
			if (listenerDataList.Count == 0) {
				return false;
			}
			return listenerDataList.Where (d => d.channelId == channelId).First ().isRoyal;
		}

		public bool IsListenerDataExists (string channelId) {
			if (listenerDataList.Count == 0) {
				return false;
			}
			return listenerDataList.Exists (d => d.channelId == channelId);
		}

		public bool IsListenerDataExists (string channelId, string imageUrl) {
			if (listenerDataList.Count == 0) {
				return false;
			}
			if (IsListenerDataExists (channelId)) {
				if (imageUrl.Equals (listenerDataList.Single (d => d.channelId == channelId).profileImageUrl)) {
					return true;
				}
			}
			return false;
		}

		public string GetImageUrl (string channelId) {
			return listenerDataList.Single (d => d.channelId == channelId).profileImageUrl;
		}

		public void UpdateProfileImage (string channelId, string imageUrl, AtlasManager.AtlasInfo atlasInfo) {
			listenerDataList.Where (d => d.channelId == channelId).ToList ().ForEach (x => {
				x.profileImageUrl = imageUrl;
				x.iconAtlasInfo = atlasInfo;
			});
		}

		public ListenerData GetListenerData (string channelId) {
			return listenerDataList.Single (d => d.channelId == channelId);
		}

		public void AddListenerData (ListenerData listenerData) {
			listenerDataList.Add (listenerData);
		}
	}

	public class ListenerData : IEquatable<ListenerData> {
		public string channelId;
		public string channelTitle;
		public string profileImageUrl;
		public bool isRoyal;
		public AtlasManager.AtlasInfo iconAtlasInfo;

		public ListenerData (string id, string title, string url, bool isRoyal, AtlasManager.AtlasInfo atlasInfo) {
			this.channelId = id;
			this.channelTitle = title;
			this.profileImageUrl = url;
			this.isRoyal = isRoyal;
			this.iconAtlasInfo = atlasInfo;
		}

		public override bool Equals (object other) {
			if (!(other is ListenerData)) return false;
			return Equals ((ListenerData) other);
		}

		public bool Equals (ListenerData other) {
			return channelId.Equals (other.channelId) && profileImageUrl.Equals (other.profileImageUrl) && isRoyal.Equals (other.isRoyal) && iconAtlasInfo.Equals (other.iconAtlasInfo);
		}

		public override int GetHashCode () {
			int hashCode = channelId.GetHashCode ();
			hashCode = (hashCode * 397) ^ profileImageUrl.GetHashCode ();
			hashCode = hashCode ^ isRoyal.GetHashCode ();
			return hashCode ^ iconAtlasInfo.GetHashCode ();
		}
	}
}