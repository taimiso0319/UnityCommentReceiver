using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using YouTubeLive.Util;
namespace YouTubeLive {
	public class Receiver : MonoBehaviour {
		public string api;
		[Tooltip ("videoでもchannelのURLでもOK")]
		public string url;
		public APIManager apiManager;
		[SerializeField] private bool isRecieving = false;
		[SerializeField] private float showCommentSec = 0.07f;
		private CurrencyExchanger currencyExchanger;

		[ContextMenu ("SetAPI")]
		public void SetAPI () {
			APIData.apiKey = api;
		}

		[ContextMenu ("StartCommentReceiving")]
		public void StartCommentReceiving () {
			if (currencyExchanger == null) { currencyExchanger = new CurrencyExchanger (); }
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
				yield return new WaitForSeconds (3.0f);
				StartCoroutine (ShowComment ());
			}
		}

		private IEnumerator ShowComment () {
			if (apiManager.commentData.commentQueue.Count < 5) {
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
					ApplicationManager.Instance.uIManager.rankView.UpdateElement (c, atlasInfo);
				} else if (apiManager.listenersData.IsListenerDataRoyal (c.channelId)) {
					ApplicationManager.Instance.uIManager.rankView.UpdateElement (c, new AtlasManager.AtlasInfo ());
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