using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouTubeLive;
using YouTubeLive.UI;

public class ApplicationManager : SingletonMonoBehaviour<ApplicationManager> {
	[SerializeField] private APIManager _apiManager = null;
	public APIManager apiManager {
		get { return _apiManager; }
	}

	[SerializeField] private UIManager _uiManager = null;
	public UIManager uIManager {
		get { return _uiManager; }
	}
}