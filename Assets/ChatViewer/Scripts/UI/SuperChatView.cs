using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouTubeLive;

namespace YouTubeLive.UI {
	public class SuperChatView : MonoBehaviour {
		[SerializeField] private GameObject element = null;
		[SerializeField] private GameObject content = null;

		public void AddElement (CommentStatus commentStatus, AtlasManager.AtlasInfo atlasInfo) {
			if (element == null || content == null) { return; }
			GameObject obj = Instantiate (element, content.transform);
			SuperChatElement superChatElement = obj.GetComponent<SuperChatElement> ();
			superChatElement.SetSuperChatAmount (commentStatus.amountDisplayString);
			superChatElement.SetMessage (commentStatus.userComment);
			superChatElement.SetName (commentStatus.displayName);
			superChatElement.SetIcon (atlasInfo.packedTexture, atlasInfo.uvRect);
			superChatElement.AdjustHeight ();
		}
	}

}