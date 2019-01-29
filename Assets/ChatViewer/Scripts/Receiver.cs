using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
namespace YouTubeLive {
	public class Receiver : MonoBehaviour {
		public string api;
		[Tooltip ("videoでもchannelのURLでもOK")]
		public string url;
		public APIManager apiManager;
		[SerializeField] private bool isRecieving = false;
		[SerializeField] private float showCommentSec = 0.07f;

		[ContextMenu ("StartCommentReceiving")]
		public void StartCommentReceiving () {
			apiManager.ClearSettings ();
			APIData.apiKey = api;
			StartCoroutine (TryStartReceiving ());
		}

		public void StopCommentReceiveing () {
			apiManager.StopReceiveComments ();
			StopAllCoroutines ();
		}

		private IEnumerator TryStartReceiving () {
			if (apiManager.StartReceiveComments (url)) {
				yield return new WaitForSeconds (2.0f);
				StartCoroutine (ShowComment ());
			}
		}

		private IEnumerator ShowComment () {
			if (apiManager.commentData.commentQueue.Count == 0) {
				yield return new WaitForSeconds (showCommentSec);
				StartCoroutine (WaitComment ());
			} else {
				CommentStatus c = apiManager.commentData.commentQueue.Dequeue ();
#if UNITY_EDITOR
				//Debug.Log (c.displayName + ": " + c.displayMessage);
#endif
				if (c.type == Json.ChatDetails.Snippet.EventType.superChatEvent.ToString ()) {
					AtlasManager.AtlasInfo atlasInfo = apiManager.listenersData.GetListenerData (c.channelId).iconAtlasInfo;
					ApplicationManager.Instance.uIManager.superChatView.AddElement (c, atlasInfo);
				}
				var d = Regex.Replace (c.publishedAt, @"\..*", "Z");
				StartCoroutine (WaitComment ());
			}
		}

		private IEnumerator WaitComment () {
			StopCoroutine (ShowComment ());
			yield return new WaitForSeconds (showCommentSec);
			StartCoroutine (ShowComment ());
		}

	}
}