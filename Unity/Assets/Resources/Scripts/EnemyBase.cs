﻿using UnityEngine;
using System.Collections;

public class EnemyBase : MonoBehaviour {
	public static byte restChildren = 0;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}
	void OnTriggerEnter(Collider col){
		Debug.Log ("col");
		Destroy(restChildren <= 0 ? gameObject : null);
	}
}