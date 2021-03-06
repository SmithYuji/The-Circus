﻿using UnityEngine;
using System.Collections;

public class MissileFactory : MonoBehaviour
{
	private static int Numbering = 0;
	private static Object missilePrefab;

	void Awake(){
		missilePrefab = Resources.Load ("prefabs/missile");
	}

	public GameObject NewPlayerMissile (Vector3 StartPos, Quaternion StartRot,bool hasParent)
	{
		GameObject newMissile = (GameObject)Instantiate (missilePrefab, Vector3.zero, Quaternion.identity);
		newMissile.name = newMissile.name.Substring (0, 7);
		newMissile.transform.transform.parent = hasParent ? GameObject.Find ("missiles").transform : null;
		newMissile.transform.localPosition = StartPos;
		newMissile.transform.localRotation = StartRot;
		return newMissile;
	}

	/// <summary>
	/// 適用のミサイルを生成、引数は初期位置
	/// </summary>
	/// <returns>The missile e.</returns>
	/// <param name="Pos">Position.</param>
	public GameObject NewEnemyMissile (Vector3 Pos)
	{
		Numbering++;
		GameObject newMissileE = (GameObject)Instantiate (missilePrefab, Pos, Quaternion.identity);
		newMissileE.name = newMissileE.name.Substring (0, 7)+Numbering;
		newMissileE.layer = 12;
		newMissileE.tag = "EnemyMissile";
        newMissileE.transform.FindChild("Glow").gameObject.SetActive(true);

        //newMissileE.transform.FindChild ("Steam").gameObject.SetActive(true);
        //newMissileE.transform.FindChild ("Afterburner").gameObject.SetActive(true);
        return newMissileE;
	}

	public GameObject NewEnemyMissile (Vector3 Pos,Vector3 Rot)
	{
		GameObject newMissileE = NewEnemyMissile (Pos);
		newMissileE.transform.Rotate(Rot);
		return newMissileE;
	}
}
