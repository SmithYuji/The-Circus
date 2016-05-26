﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Missile : MonoBehaviour
{
	[SerializeField]
	private AudioClip AudioClip1;
	[SerializeField]
	private AudioClip AudioClip2;

	private AudioSource AudioS;

	private float Speed;//700
	private float LifeTime = 40;

	private static Airframe AirFrame;
	private Vector3 StartPos;
	private Quaternion StartRot;


	void Awake(){
		AirFrame = GameObject.Find ("eurofighter").GetComponent<Airframe> ();
		AudioS = gameObject.GetComponent<AudioSource> ();
	}

	void Start ()
	{
		if (gameObject.layer == 12) {
			ReticleSystem.AddMissiles.Add (gameObject);
			Speed = 700;//700
			MissileRader.AddOutRangeMissile.Add (gameObject.transform);
		} else {
			Speed = 850;
		}
		AudioS.clip = AudioClip1;
		StartPos = transform.localPosition;
		StartRot = transform.localRotation;
	}

	public IEnumerator StraightToTgt (Transform tgt,bool Player)
	{
		if (Player) {
			ShootReady ();
		} else {
			ShootReady_E ();
		}
		transform.LookAt (tgt);

		while (true) {
			StartCoroutine(MoveForward ());
			yield return null;
		}
	}

	public IEnumerator StraightToTgt (bool Player)
	{
		if (Player) {
			ShootReady ();
		} else {
			ShootReady_E ();
		}
		while (true) {
			StartCoroutine(MoveForward ());
			yield return null;
		}
	}

	public IEnumerator TrackingPlayer (Transform tgt)
	{
		ShootReady ();
		while (true) {
			StartCoroutine (GetAimingPlayer (tgt));
			StartCoroutine(MoveForward ());
			yield return null;
		}
	}

	public IEnumerator TrackingEnemy (Transform tgt)
	{
		ShootReady_E ();
		while (true) {
			Vector3 RandomError = new Vector3 (Random.Range(-10,10),Random.Range(-10,10),Random.Range(-10,10));
			while(Distance(tgt) >= 40){
				ErrorTracking (tgt,RandomError);
				yield return null;
			}
			if(Distance(tgt) < 40){
				RefreshSelfBreak ();
				while (true) {
					StartCoroutine (MoveForward ());
					yield return null;
				}
			}
			yield return null;
		}
	}

	private void ErrorTracking(Transform tgt,Vector3 Random3){
		transform.LookAt (tgt.transform.position + Random3);
		StartCoroutine(MoveForward ());
	}
	private void RefreshSelfBreak(){
		StopCoroutine (SelfBreak());
		LifeTime = 4;
		StartCoroutine (SelfBreak());
	}

	private float Distance(Transform tgt){
		try{
		return Mathf.Abs(Vector3.Distance(tgt.position,transform.position));
		}catch{
			return 0f;
		}
	}

	private void ShootReady ()
	{
		AirFrame.Reload (StartPos, StartRot); //StartCoroutine (GameObject.Find("GameManager").GetComponent<GameManager>().reloadMissile(startPos,startRot));
		transform.parent = null;
		AudioS.Play ();
		StartCoroutine (SelfBreak ());
		transform.FindChild ("Steam").gameObject.SetActive (true);
		transform.FindChild ("Afterburner").gameObject.SetActive (true);
	}

	private void ShootReady_E ()
	{
		//audioS.Play();
		StartCoroutine (SelfBreak ());
	}

	private IEnumerator GetAimingPlayer(Transform tgt){
		Vector3 TgtPos = new Vector3 (tgt.transform.position.x, tgt.transform.position.y, tgt.transform.position.z);
		transform.LookAt (TgtPos);
		yield return null;
	}

//	private Vector3 GetAimingEnemy (Transform tgt)
//	{
//		
////		try{
////			Vector3 Distance = new Vector3 ((tgt.transform.position.x+ Random.Range (-40, 40)) - transform.position.x,(tgt.transform.position.y+ Random.Range (-40, 40)) - transform.position.y, (tgt.transform.position.z+ Random.Range (-40, 40)) - transform.position.z);
////			Vector3 TgtPos = new Vector3 (tgt.position.x - Distance.x/2,tgt.position.y - Distance.y/2,tgt.position.z - Distance.z/2);
////		transform.LookAt (TgtPos);
////			return TgtPos;
////		}catch{
////			return Vector3.zero;
////		}
//		Vector3 a;
//		return Vector3;
//	}
//
	private IEnumerator MoveForward ()
	{
		transform.Translate (Vector3.forward * Time.deltaTime * Speed);
		yield return null;
	}

	void OnTriggerEnter (Collider col)
	{
		if (transform.parent != null) {
			return;
		}
		StartCoroutine (BreakMissile ());
	}

	private IEnumerator SelfBreak ()
	{
		float time = 0f;
		while (time < LifeTime) {
//			if(transform.localPosition.y <= 0){
//				StartCoroutine (BreakMissile ());
//				yield break;
//			}
			time += Time.deltaTime;
			yield return null;
		}
		StartCoroutine (BreakMissile ());
		yield return null;
	}

	private IEnumerator BreakMissile ()
	{
		Instantiate (Resources.Load ("prefabs/Explosion"), transform.position, Quaternion.identity);
		StopAllCoroutines ();
		MissileRader.DestroyMissile (gameObject.transform);
		EstimationSystem.RemoveList (gameObject);
		yield return new WaitForSeconds(0.2f);
		Destroy (gameObject);
		yield return null;
	}
}

