﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Attack : MonoBehaviour
{

	public static float Reloading;
	private const float missileDelay = 0.15f;
	private const float gunDelay = 0.1f;
	private const float MMIDelay = 1f;
	//MultipleMissileInterceptの省略
	private static ReticleSystem Reticle;
	private static Airframe Frame;

	[SerializeField]
	private Gun Guns;
    [SerializeField]
    private GameObject[] missile = new GameObject[4];

	private static Queue<GameObject> _playerMissiles = new Queue<GameObject> ();

	public static Queue<GameObject> PlayerMissiles {
		get {
			return _playerMissiles;
		}
	}

	private static bool _mmiReady;

	public static bool MMIisReady {
		set {
			if (value == false) {
				ReticleSystem.LockOnTgt = null;
			}
			_mmiReady = value;
		}get {
			return _mmiReady;
		}
	}

	void Awake ()
	{
		Bullet.Lighting = FindObjectOfType<LightingControlSystem> ();
		Reticle = GameObject.FindObjectOfType<ReticleSystem> ();
		Frame = GameObject.FindObjectOfType<Airframe> ();
	}

	void Start ()
	{
        foreach(GameObject m in missile)
        {
            _playerMissiles.Enqueue(m);
        }
		Reloading = 0;
	}

	public void EnableAttack(){
		StartCoroutine (MultipleMissileInterceptSystem ());
		StartCoroutine (MissileShoot ());
		StartCoroutine (GunShoot ());
	}


	//略名 : MMIS 複数ミサイル迎撃システム
	public IEnumerator MultipleMissileInterceptSystem ()
	{
        if (VRMode.isVRMode)
        {
            yield break;

            yield return null;
        }
		while (!GameManager.IsGameOver) {
			Reloading += Time.deltaTime;
			if (Reloading >= MMIDelay) {
				if (!MMIisReady) {
					if (isBoot (Reloading)) {
						yield return StartCoroutine (Reticle.ChangeMode (true));
					} else if (isEnd (Reloading)) {
						LockOrReset (Reloading);
						yield return null;
					} 
				} else if (isCancel ()) {
					MMIisReady = false;
					yield return StartCoroutine (Reticle.ChangeMode (false));
				} else if (isTrackingShoot () ) {
					MMIisReady = false;
					yield return StartCoroutine (MMI_Instruction ());
				}
			}
			yield return null;
		}
	}

	private IEnumerator MMI_Instruction ()
	{
		yield return StartCoroutine (Missile (false).MultipleMissileInterceptShoot ());
		StartCoroutine(Frame.Reload (Missile (false).StartPos, Missile (true).StartRot));//Missile (false).MultipleMissileInterceptShoot ());
		yield return null;
	}

	private bool isBoot (float Reloading)
	{
		return (Input.GetKeyDown (KeyCode.JoystickButton18) || Input.GetKeyDown (KeyCode.Space) || InputVRController.GetPress(InputVRController.InputPress.PressPad,HandType.Right));
	}

	private bool isEnd (float Reloading)
	{
		return (Input.GetKeyUp (KeyCode.JoystickButton18) || Input.GetKeyUp (KeyCode.Space) || InputVRController.GetPress(InputVRController.InputPress.UpPad, HandType.Right));
	}

	private void LockOrReset (float Reloading)
	{
		if (ReticleSystem.MultiMissileLockOn.Count <= 0) {
			StartCoroutine (Reticle.ChangeMode (false));
		} else {
			Reticle.MMIReady ();
		}
	}

	public static bool isCancel ()
	{
        return (Input.GetKeyUp(KeyCode.JoystickButton16) || Input.GetKeyUp(KeyCode.Alpha3) || InputVRController.GetUp(InputVRController.InputPress.UpMenu,HandType.Right));
	}

	private bool isTrackingShoot ()
	{
		if (((Input.GetAxis ("LTrigger") == 1 || Input.GetKeyDown (KeyCode.V)) && _playerMissiles.Count >= 1)) {
			GameManager.MissileCount += 1;
			return true;
		} else {
			return false;
		}
	}

	public IEnumerator MissileShoot ()
	{
		float Reloading = 0.0f;

		while (!GameManager.IsGameOver) {
			Reloading += Time.deltaTime;
			if (Reloading >= missileDelay) {
                if (!VRMode.isVRMode){
                    if (isStraightMissileShoot()) {
                        StartCoroutine(Missile(true).Straight(true));
                        Reloading = 0f;
                    } else if (isTrackingShoot() && ReticleSystem.LockOnTgt != null) {
                        StartCoroutine(Missile(true).TrackingForEnemy(ReticleSystem.LockOnTgt.transform, true));
                        Reloading = 0f;
                    }
                } else if(isStraightMissileShoot())
                {
                    if (ReticleSystem.LockOnTgt != null)
                    {
                        StartCoroutine(Missile(true).TrackingForEnemy(ReticleSystem.LockOnTgt.transform, true));
                        Reloading = 0f;
                    }
                    else
                    {
                        StartCoroutine(Missile(true).Straight(true));
                        Reloading = 0f;
                    }
                }
			}
			yield return null;
		}
	}

	private bool isStraightMissileShoot ()
	{
		if ((Input.GetAxis ("RTrigger") == 1 || InputVRController.GetPress(InputVRController.InputPress.PressTrigger,HandType.Right) || Input.GetKeyDown (KeyCode.C)) && _playerMissiles.Count >= 1) {
			GameManager.MissileCount += 1;
			return true;
		} else {
			return false;
		}
	}


	public IEnumerator GunShoot ()
	{
		float Reloading = 0.0f;
		while (!GameManager.IsGameOver) {
			Reloading += Time.deltaTime;
			if (Reloading >= gunDelay) {
				if (isGunShot ()) {
					StartCoroutine (Guns.Shoot ());//GameObject.Find ("guns").GetComponent<Gun> ().Shoot ());
					Reloading = 0f;
				}
			}
			yield return null;
		}
	}

	private static MissileSystem Missile (bool isDequeue)
	{
		return (isDequeue ? PlayerMissiles.Dequeue () : PlayerMissiles.Peek ()).GetComponent<MissileSystem> ();
	}

	private bool isGunShot ()
	{
		return (Input.GetKey (KeyCode.JoystickButton12) || Input.GetKey (KeyCode.F) || InputVRController.GetPressStay(InputVRController.InputPress.PressTrigger, HandType.Left));
	}

	void OnDestroy(){
		PlayerMissiles.Clear ();
	}
}