﻿using UnityEngine;
using System.Collections;

public class engineSound : MonoBehaviour {
	AudioSource AudioBox;
	// Use this for initialization

	public struct engineConfig
	{
		public const float normalSpeed = 300;
		public const float pitchUpSpeed = 0.005f;
	}

	void Start () {
		AudioBox = GetComponent<AudioSource> ();
		AudioBox.pitch = AudioBox.pitch;
	}

	public float Pitch{
		set{
			float v = (value - engineConfig.normalSpeed)*engineConfig.pitchUpSpeed;
			try{
				ChangePitch(v);
			}catch{
			}
		}get{
			return AudioBox.pitch;
		}
	}

	public void ChangePitch(float v){
		if (AudioBox.pitch <= 3 && AudioBox.pitch >= 0.5f) {
			AudioBox.pitch = v + 1;
			if (AudioBox.pitch > 3) {
				AudioBox.pitch = 3;
			} else if (AudioBox.pitch < 0.5f) {
				AudioBox.pitch = 0.5f;
			}
		}
	}
}
