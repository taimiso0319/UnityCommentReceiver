using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouTubeLive;

public abstract class ScrollView : MonoBehaviour {
	[SerializeField] protected Material iconMaterial = null;
	[SerializeField] protected GameObject element = null;
	[SerializeField] protected GameObject content = null;

	public abstract Element AddElement (CommentStatus commentStatus, AtlasManager.AtlasInfo atlasInfo);
}