using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouTubeLive.UI {
	public class UIManager : MonoBehaviour {
		[SerializeField] private SuperChatView _superChatView = null;
		public SuperChatView superChatView {
			get { return _superChatView; }
		}
	}
}