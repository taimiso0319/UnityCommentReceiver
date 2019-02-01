using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Element : MonoBehaviour {
	[SerializeField] private RawImage _icon = null;
	public RawImage icon {
		get { return _icon; }
	}

	[SerializeField] private Text _name = null;
	public Text name {
		get { return _name; }
	}

	public void SetName (string s) {
		this._name.text = s + " さん";
	}

	public void SetIcon (Texture2D tex, Rect uv) {
		this._icon.texture = tex;
		this._icon.uvRect = uv;
	}
}