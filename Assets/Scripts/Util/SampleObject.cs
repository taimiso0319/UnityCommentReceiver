using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleObject : MonoBehaviour {

	[ContextMenu("test")]
	public void Test(){
		int i = 32;
		float f = 2.00f;
		string s = "maji";
		bool b = false;

		log(i);
		log(f);
		log(s);
		log(b);

	}

	public void log(object obj){
		Debug.Log(obj.GetType());
		Debug.Log(obj);
		Type t = obj.GetType();
		INIParser parser = new INIParser();
		parser.Open(Application.dataPath + "/test.ini");
		
	}
}
