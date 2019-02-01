using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouTubeLive;

namespace YouTubeLive.UI {
	public class SuperChatView : ScrollView {

		public override Element AddElement (CommentStatus commentStatus, AtlasManager.AtlasInfo atlasInfo) {
			if (element == null || content == null) { return null; }
			GameObject obj = Instantiate (element, content.transform);
			obj.transform.SetAsFirstSibling ();
			SuperChatElement superChatElement = obj.GetComponent<SuperChatElement> ();
			superChatElement.icon.material = iconMaterial;
			superChatElement.SetSuperChatAmount (commentStatus.amountDisplayString);
			superChatElement.SetMessage (commentStatus.messageText);
			superChatElement.SetName (commentStatus.displayName);
			superChatElement.SetIcon (atlasInfo.packedTexture, atlasInfo.uvRect);
			superChatElement.AdjustHeight ();
			return superChatElement;
		}
	}

}