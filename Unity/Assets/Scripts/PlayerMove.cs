﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;

public class PlayerMove : MonoBehaviour
{
	[SerializeField]
	private ReticleSystem Reticle;
	[SerializeField]
	private Material SpeedLineMaterial;
	[SerializeField]
	private float SpeedLineThickness = 0.75f;
	[SerializeField]
	private   List<ParticleSystem> Burner  = new List<ParticleSystem>();
	[SerializeField]
	private  List<ParticleSystem> Glow = new List<ParticleSystem>();

    private static GameObject controller;
	private static float speed = 300f;//戦闘機の速さ
	private static Quaternion DefaltRotation;
	private const short Accele = +1;
	private const short Decele = -1;
	private const float MinSpeed = 200f;
	private const float MaxSpeed = 690f;
	private const float Keep = 0;
	private const float MaxAngle = 5;
	private static List<ParticleSystem.EmissionModule> em = new List<ParticleSystem.EmissionModule>();
	private static List<ParticleSystem.MinMaxCurve> rate = new List<ParticleSystem.MinMaxCurve>();
	private static EngineSound EngineS;
	private static CameraMotionBlur MotionBlur;
	public static float Speed{
		get{
			return speed;
		}
	}
		
	void Awake(){
		Glow.ForEach(glow => em.Add(glow.emission));
		Glow.ForEach (glow => rate.Add (glow.emission.rate));
		EngineS = GameObject.FindObjectOfType<EngineSound> ();
	}

	void Start(){
		SpeedLineMaterial.SetColor ("_Color", new Color (1, 1, 1, 0));
		FindObjectOfType<GameManager> ().StartStage ();
		FindObjectOfType<CameraSetting> ().OnScene (Scenes.Stage);
		DefaltRotation = Airframe.AirFrame.transform.localRotation;
	}

	public void Manual ()
	{
        if (VRMode.isVRMode)
        {
            speed = MaxSpeed;
            controller = GameObject.Find("Controller (left)");
        }
		gameObject.GetComponent<Animator>().Stop ();
		CameraSystem cameraSystem = FindObjectOfType<CameraSetting>().MyCamera.GetComponent<CameraSystem>();
		MotionBlur = FindObjectOfType<CameraMotionBlur> ();
		cameraSystem.StartCoroutine(CameraSystem.CameraChangePosition());
		cameraSystem.StartCoroutine(cameraSystem.CameraModeChange());
		cameraSystem.SetUp ();
		FindObjectOfType<EnemyBase> ().StartCoroutine (EnemyBase.PlayerInArea ());
		gameObject.GetComponent<Attack> ().EnableAttack ();
		Reticle.EnableReticle ();
		StartCoroutine (Move ());
		StartCoroutine (ChangeSpeed ());
		StartCoroutine (NotificationSystem.UpdateNotification ("操縦権を搭乗者に委託します"));
	}
	public IEnumerator FadeInSpeedLine(){
		float fadeSpeed = 0.1f;
		while (SpeedLineMaterial.GetColor ("_Color").a < (300/MaxSpeed)*SpeedLineThickness) {
			SpeedLineMaterial.SetColor ("_Color", new Color (1, 1, 1, SpeedLineMaterial.GetColor ("_Color").a + (Time.deltaTime*SpeedLineThickness*fadeSpeed)));
			yield return null;
		}
	}

	private IEnumerator Move ()
	{
		while (!GameManager.IsGameOver) {
            Quaternion crot = controller.transform.localRotation;
            if (VRMode.isVRMode && InputVRController.GetPressStay(InputVRController.InputPress.PressPad, HandType.Left))
            {
                Rotation(new Vector3(crot.x*2f, crot.y*2f, crot.z*2f));//InputController());
            }else if(!VRMode.isVRMode){
                Rotation(InputController());

            }
            MoveForward ();
			yield return null;
		}
	}

	private IEnumerator AutoMove(){
		
		while(true){
			MoveForward ();
			yield return null;
		}
	}

	private void MoveForward ()
	{
		transform.Translate (Vector3.forward * Time.deltaTime * Speed);
	}

	/// <summary>
	/// 機体の速度を加減する。
	/// </summary>
	/// <returns>The speed.</returns>
	private IEnumerator ChangeSpeed ()
	{
		while (!GameManager.IsGameOver && !VRMode.isVRMode) {
            if (isAccele()) {
                FuelInjector(Accele);
            } else if(isDecele()){
                FuelInjector(Decele);
            } else if (isKeyUp()) {
				AfterBurner (Keep);
				StartCoroutine (CameraSystem.CameraPosReset ());
			}
			yield return null;
		}
	}

	private bool isKeyUp(){
		return Input.GetKeyUp (KeyCode.JoystickButton13) || Input.GetKeyUp (KeyCode.JoystickButton14) || Input.GetKeyUp (KeyCode.Alpha1) || Input.GetKeyUp (KeyCode.Alpha2) || InputVRController.GetUp(InputVRController.InputPress.UpGrip);
	}

	private bool isAccele(){
		bool _isAccele = Input.GetKey (KeyCode.JoystickButton14) || Input.GetKey (KeyCode.Alpha2) || InputVRController.GetPressStay(InputVRController.InputPress.PressGrip,HandType.Right);
		BlurEffects (_isAccele);
		return _isAccele;
	}

    private bool isDecele()
    {
		return Input.GetKey (KeyCode.JoystickButton13) || Input.GetKey (KeyCode.Alpha1) || InputVRController.GetPressStay(InputVRController.InputPress.PressGrip,HandType.Left);
        
    }

	private void BlurEffects(bool isAccele){
		MotionBlur.maxVelocity = Mathf.Clamp (MotionBlur.maxVelocity + (isAccele ? Time.deltaTime : -Time.deltaTime), 3.5f, 10);//isAccele ? 10 : Mathf.Clamp (MotionBlur.maxVelocity);//3.5f;
		MotionBlur.velocityScale =  Mathf.Clamp (MotionBlur.maxVelocity + (isAccele ? Time.deltaTime*0.1f : -Time.deltaTime*0.1f), 0.35f, 1);
	}

	/// <summary>
	/// 機体の速度に制限。
	/// 巡航速度1000Km 最高速度2484Km (speed*60*60=時速とする)
	/// </summary>
	/// <value>The speed.</value>
	private void FuelInjector (float Power){
		speed = Mathf.Clamp(Speed + Power,MinSpeed,MaxSpeed);
		SpeedLineMaterial.SetColor("_Color",new Color (1,1,1,(speed/MaxSpeed)*SpeedLineThickness));
			if (Speed > MinSpeed && Speed < MaxSpeed) {
				AfterBurner (Power);
				CameraSystem.MoveCamera (Power);
			}
			EngineS.Pitch = Speed;
	}

	private void AfterBurner (float Fuel)
	{

		if (Fuel > Keep) {
			HighPower ();
		} else {
			LowPower ();
		}
	}

	private void HighPower ()
	{
		foreach (ParticleSystem burner in Burner) {
			burner.startSpeed = 25;
			burner.startSize = 1.4f;
		}
		Glow.ForEach(glow => glow.startSpeed = 25);
		rate.ForEach(rt => rt.constantMax = 450);
		em.ForEach(e => e.rate = rate[0]);
	}

	private void LowPower ()
	{
		foreach (ParticleSystem burner in Burner) {
			burner.startSpeed = 4;
			burner.startSize = 0.7f;
		}
		Glow.ForEach(glow => glow.startSpeed = 4);
		rate.ForEach(rt => rt.constantMax = 100);
		em.ForEach(e => e.rate = rate[0]);
	}

	private void Rotation(Vector3 AddRot) {
        transform.Rotate(AddRot);
		//transform.Rotate (AddRot.x / 1.5f, 0f, VRMode.isVRMode ? AddRot.y * 2 : AddRot.z * 2f);
		Airframe.AirFrame.transform.localRotation = new Quaternion (DefaltRotation.x + AddRot.x/50,DefaltRotation.y,DefaltRotation.z,DefaltRotation.w);
//		var RotateX = (DefaltRotation.x + Mathf.Abs (AddRot.x)) < DefaltRotation.x + MaxAngle ? DefaltRotation.x + (AddRot.x * MaxAngle * (Time.deltaTime / 6)) : Airframe.AirFrame.transform.localRotation.x;
//		Airframe.AirFrame.transform.localRotation = new Quaternion (RotateX, DefaltRotation.y, DefaltRotation.z, DefaltRotation.w);
	}
    private Vector3 InputController() {
        if (!VRMode.isVRMode)
        {
            return new Vector3(Mathf.Sign(Input.GetAxis("Vertical")) * 200 * Time.deltaTime, 0,Mathf.Sign( Input.GetAxis("Horizontal") * 140 * Time.deltaTime));
        } else
        {
            Vector2 Axis = InputVRController.GetAxis(HandType.Left);
            return new Vector2(Axis.y*2, Axis.x*2);
        }
	}
}