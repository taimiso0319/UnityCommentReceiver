using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using YouTubeLive;

namespace YouTubeLive.UI {

	public class RankView : ScrollView {
		private List<RankElement> elements = new List<RankElement> ();
		[SerializeField] private InputField channelURLInputField = null;
		public string channelId = "UC_4tXjqecqox5Uc05ncxpxg";
		private Order currentOrder = Order.DESC;
		private Sort currentSort = Sort.TOTAL_CHARGE;
		private APIManager apiManager;
		[SerializeField] private Button triangle = null;

		enum Order {
			ASC = 0,
			DESC
		}

		enum Sort {
			CHARGE,
			TOTAL_CHARGE,
			TOTAL_COMMENT
		}

		public void UpdateElement (CommentStatus commentStatus, AtlasManager.AtlasInfo atlasInfo) {
			RankElement ele = elements.Where (x => x.listenerId.Equals (commentStatus.channelId)).FirstOrDefault ();
			if (ele == null) {
				if (apiManager == null) { apiManager = APIManager.Instance; }
				int charge = apiManager.databaseController.GetSuperChatsAmountByListenerAtChannel (APIData.channelId, commentStatus.channelId);
				int comments = apiManager.databaseController.GetCommentTotalByListenerAtChannel (APIData.channelId, commentStatus.channelId);
				ele = AddElement (commentStatus, charge, comments, atlasInfo) as RankElement;
				ele.AddCharge (charge);
			} else {
				ele.AddCommentTotal (1);
				ele.AddCharge (commentStatus.convertedAmount, true);
			}
		}

		[ContextMenu ("clear")]
		public void ClearElements () {
			foreach (var e in elements) {
				Destroy (e.gameObject);
			}
			elements.Clear ();
		}

		[ContextMenu ("test")]
		public void LoadListenerRank () {
			if (apiManager == null) { apiManager = APIManager.Instance; }
			if (elements.Count > 0) { ClearElements (); }
			if (apiManager.receiveChat) {
				if (channelURLInputField.text == "") {
					channelId = APIData.channelId;
				}
			}
			if (channelId == "" || channelId == null) { Debug.LogWarning ("channelIdが入力されていません。"); return; }
			Observable.FromCoroutine (() => apiManager.RestoreRoyalListenerData (channelId), publishEveryYield : false)
				.Subscribe (
					_ => Debug.Log ("restore complete. listing up."),
					() => {
						IReadOnlyList<ListenerData> listenerDatas = apiManager.listenersData.GetList ();
						foreach (var l in listenerDatas) {
							if (!l.isRoyal) { continue; }
							int chargeTotal = apiManager.databaseController.GetSuperChatsAmountByListenerAtChannel (channelId, l.channelId);
							int comments = apiManager.databaseController.GetCommentTotalByListenerAtChannel (channelId, l.channelId);
							var ele = AddElement (l.channelId, l.channelTitle, chargeTotal, comments, l.iconAtlasInfo) as RankElement;
							if (apiManager.receiveChat) {
								int charge = apiManager.databaseController.GetSuperChatsAmountByListenerInVideo (l.channelId, APIData.videoId);
								ele.AddCharge (charge);
							}
						}
					});
		}

		public void SortByChargeTotal (bool desc) {
			if (!desc) {
				elements.Sort ((a, b) => a.chargeTotal - b.chargeTotal);
			} else {
				elements.Sort ((a, b) => b.chargeTotal - a.chargeTotal);
			}
			currentOrder = desc ? Order.DESC : Order.ASC;
			currentSort = Sort.TOTAL_CHARGE;
			UpdateTransformOrder ();
			UpdateTriangle ();
		}

		public void SortByCharge (bool desc) {
			if (!desc) {
				elements.Sort ((a, b) => a.currentCharge - b.currentCharge);
			} else {
				elements.Sort ((a, b) => b.currentCharge - a.currentCharge);
			}
			currentOrder = desc ? Order.DESC : Order.ASC;
			currentSort = Sort.CHARGE;
			UpdateTransformOrder ();
			UpdateTriangle ();
		}

		public void SortByCommentTotal (bool desc) {
			if (!desc) {
				elements.Sort ((a, b) => a.commentTotal - b.commentTotal);
			} else {
				elements.Sort ((a, b) => b.commentTotal - a.commentTotal);
			}
			currentOrder = desc ? Order.DESC : Order.ASC;
			currentSort = Sort.TOTAL_COMMENT;
			UpdateTransformOrder ();
			UpdateTriangle ();
		}

		private void UpdateTransformOrder () {
			foreach (var e in elements) {
				e.transform.SetAsFirstSibling ();
			}
		}

		public void ExtractChannelId () {
			string url = channelURLInputField.text;
			if (!url.Contains ("youtu") && !url.Contains ("channel")) {
				Debug.LogError ("urlが正しくない恐れがあります。");
				return;
			}
			channelId = Regex.Replace (url, "(https?:\\/\\/)?(www\\.)?youtu((\\.be)|(be\\..{2,5}))\\/((user)|(channel))\\/", "");
		}

		public override Element AddElement (CommentStatus commentStatus, AtlasManager.AtlasInfo atlasInfo) {
			if (element == null || content == null) { return null; }
			GameObject obj = Instantiate (element, content.transform);
			obj.transform.SetAsFirstSibling ();
			RankElement rankElement = obj.GetComponent<RankElement> ();
			rankElement.icon.material = iconMaterial;
			rankElement.SetListenerId (commentStatus.channelId);
			rankElement.SetName (commentStatus.displayName);
			rankElement.SetIcon (atlasInfo.packedTexture, atlasInfo.uvRect);
			elements.Add (rankElement);
			return rankElement;
		}

		public Element AddElement (CommentStatus commentStatus, int chargeTotal, int commentTotal, AtlasManager.AtlasInfo atlasInfo) {
			if (element == null || content == null) { return null; }
			RankElement rankElement = AddElement (commentStatus, atlasInfo) as RankElement;
			rankElement.SetChargeTotal (chargeTotal);
			rankElement.SetCommentTotal (commentTotal);
			return rankElement;
		}

		public Element AddElement (string listenerId, string title, int chargeTotal, int commentTotal, AtlasManager.AtlasInfo atlasInfo) {
			if (element == null || content == null) { return null; }
			CommentStatus cs = new CommentStatus ();
			cs.channelId = listenerId;
			cs.displayName = title;
			cs.convertedAmount = 0;
			return AddElement (cs, chargeTotal, commentTotal, atlasInfo);
		}

		public void SwitchOrder () {
			bool desc = currentOrder == Order.DESC ? false : true;
			switch (currentSort) {
				case Sort.CHARGE:
					SortByCharge (desc);
					break;
				case Sort.TOTAL_CHARGE:
					SortByChargeTotal (desc);
					break;
				case Sort.TOTAL_COMMENT:
					SortByCommentTotal (desc);
					break;
			}
		}

		private void UpdateTriangle () {
			if (currentOrder == Order.DESC) {
				triangle.transform.rotation = Quaternion.Euler (0, 0, 180);
			} else {
				triangle.transform.rotation = Quaternion.Euler (0, 0, 0);
			}
		}
	}
}