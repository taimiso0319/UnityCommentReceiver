using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using YouTubeLive;

namespace YouTubeLive.UI {

	public class RankView : ScrollView {
		private List<RankElement> elements = new List<RankElement> ();
		public string channelId = "UC_4tXjqecqox5Uc05ncxpxg";
		private Order currentOrder = Order.DESC;
		private Sort currentSort = Sort.TOTAL_CHARGE;
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
				AddElement (commentStatus, atlasInfo);
			} else {
				ele.AddCurrentCharge (commentStatus.convertedAmount);
			}
		}

		[ContextMenu ("reltest")]
		public void ReloadListenerRank () {
			foreach (var e in elements) {
				Destroy (e.gameObject);
			}
			elements.Clear ();
			LoadListenerRank ();
		}

		[ContextMenu ("test")]
		public void LoadListenerRank () {
			APIManager apiManager = APIManager.Instance;
			Observable.FromCoroutine (() => apiManager.RestoreListenerData (channelId), publishEveryYield : false).Subscribe (_ => Debug.Log ("restore complete. listing up."),
				() => {
					IReadOnlyList<ListenerData> listenerDatas = apiManager.listenersData.GetList ();
					foreach (var l in listenerDatas) {
						int charge = apiManager.databaseController.GetSuperChatsAmountByListenerAtChannel (channelId, l.channelId);
						RankElement ele = elements.Where (x => x.listenerId.Equals (l.channelId)).FirstOrDefault ();
						if (ele == null) {
							AddElement (l.channelId, l.channelTitle, charge, l.iconAtlasInfo);
						} else {
							ele.AddTotalCharge (charge);
						}
					}
				}
			);
		}

		public void SortByTotalCharge (bool desc) {
			if (!desc) {
				elements.Sort ((a, b) => a.totalCharge - b.totalCharge);
			} else {
				elements.Sort ((a, b) => b.totalCharge - a.totalCharge);
			}
			currentOrder = desc ? Order.DESC : Order.ASC;
			currentSort = Sort.TOTAL_CHARGE;
			UpdateTransformOrder ();
			UpdateTriangle ();
		}

		public void SortByCurrentCharge (bool desc) {
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

		public void SortByTotalComment (bool desc) {
			if (!desc) {
				elements.Sort ((a, b) => a.totalComment - b.totalComment);
			} else {
				elements.Sort ((a, b) => b.totalComment - a.totalComment);
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

		public override Element AddElement (CommentStatus commentStatus, AtlasManager.AtlasInfo atlasInfo) {
			if (element == null || content == null) { return null; }
			GameObject obj = Instantiate (element, content.transform);
			obj.transform.SetAsFirstSibling ();
			RankElement rankElement = obj.GetComponent<RankElement> ();
			rankElement.icon.material = iconMaterial;
			rankElement.SetListenerId (commentStatus.channelId);
			rankElement.SetName (commentStatus.displayName);
			rankElement.SetIcon (atlasInfo.packedTexture, atlasInfo.uvRect);
			rankElement.AddCurrentCharge (commentStatus.convertedAmount);
			elements.Add (rankElement);
			return rankElement;
		}

		public void AddElement (CommentStatus commentStatus, int totalCharge, AtlasManager.AtlasInfo atlasInfo) {
			if (element == null || content == null) { return; }
			RankElement rankElement = AddElement (commentStatus, atlasInfo) as RankElement;
			rankElement.SetTotalCharge (totalCharge);
		}

		public void AddElement (string listenerId, string title, int totalCharge, AtlasManager.AtlasInfo atlasInfo) {
			if (element == null || content == null) { return; }
			CommentStatus cs = new CommentStatus ();
			cs.channelId = listenerId;
			cs.displayName = title;
			cs.convertedAmount = 0;
			AddElement (cs, totalCharge, atlasInfo);
		}

		public void SwitchOrder () {
			bool desc = currentOrder == Order.DESC ? false : true;
			switch (currentSort) {
				case Sort.CHARGE:
					SortByCurrentCharge (desc);
					break;
				case Sort.TOTAL_CHARGE:
					SortByTotalCharge (desc);
					break;
				case Sort.TOTAL_COMMENT:
					SortByTotalComment (desc);
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