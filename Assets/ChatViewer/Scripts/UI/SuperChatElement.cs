using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YouTubeLive.UI {
	public class SuperChatElement : MonoBehaviour {
		[SerializeField] private RawImage _icon = null;
		public RawImage icon {
			get { return _icon; }
		}

		[SerializeField] private Text _name = null;
		public Text name {
			get { return _name; }
		}

		[SerializeField] private Text _message = null;
		public Text message {
			get { return _message; }
		}

		[SerializeField] private Text _superChatAmount = null;
		public Text superChatAmount {
			get { return _superChatAmount; }
		}

		public void SetName (string s) {
			this._name.text = s;
		}

		public void SetMessage (string s) {
			this._message.text = s;
		}

		public void SetSuperChatAmount (string s) {
			this._superChatAmount.text = s;
		}

		public void SetSuperChatAmount (int i) {
			this._superChatAmount.text = i.ToString ();
		}

		public void SetSuperChatAmount (float f) {
			this._superChatAmount.text = f.ToString ();
		}

		public void SetIcon (Texture2D tex, Rect uv) {
			this._icon.texture = tex;
			this._icon.uvRect = uv;
		}

		[ContextMenu ("testa")]
		public void AdjustHeight () {
			int lineCount = _message.cachedTextGenerator.lineCount;
			if (lineCount > 3) {
				this.GetComponent<RectTransform> ().sizeDelta += new Vector2 (0, (lineCount - 3) * 40);
			}
		}

	}
}