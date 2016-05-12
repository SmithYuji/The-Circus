﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Attack : MonoBehaviour {

	struct Delay{
		public const float missileDelay = 0.15f;
		public const float gunDelay = 0.1f;
	}

	private static Queue<GameObject> missiles = new Queue<GameObject> ();

	public static Queue<GameObject> Missiles{
		get{
			return missiles;
		}
	}

	void Start () {
		missiles.Enqueue (GameObject.Find ("missileA"));
		missiles.Enqueue (GameObject.Find ("missileB"));
		missiles.Enqueue (GameObject.Find ("missileC"));
		missiles.Enqueue (GameObject.Find ("missileD"));
        StartCoroutine(MissileShoot());
		StartCoroutine(GunShoot());
	}

    public IEnumerator MissileShoot()
    {
		float Reloading = 0.0f;
        while (!GameManager.GameOver)
        {
            Reloading += Time.deltaTime;
			if(Reloading >= Delay.missileDelay)
            {
                if ((Input.GetAxis("RTrigger") == 1 || Input.GetKeyDown(KeyCode.C)) && missiles.Count >= 1)
                {
                    StartCoroutine(missiles.Dequeue().GetComponent<Missile>().Straight());
                    Reloading = 0f;
				}else if ((Input.GetAxis("LTrigger") == 1 || Input.GetKeyDown(KeyCode.V)) && missiles.Count >= 1 && ReticleSystem.LockOnTgt != null)
				{
					StartCoroutine(missiles.Dequeue().GetComponent<Missile>().Tracking(ReticleSystem.LockOnTgt.transform));
					Reloading = 0f;
				}
            }
            yield return null;
        }
    }
	public IEnumerator GunShoot(){
		float Reloading = 0.0f;
		while (!GameManager.GameOver)
		{
			Reloading += Time.deltaTime;
			if(Reloading >= Delay.gunDelay)
			{
				if ((Input.GetKey(KeyCode.JoystickButton12) || Input.GetKey(KeyCode.F)))
				{
					StartCoroutine (GameObject.Find ("guns").GetComponent<Gun> ().shoot ());
					Reloading = 0f;
				}
			}
			yield return null;
		}
		yield return null;
	}
}