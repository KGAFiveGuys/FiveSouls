using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
	private GameObject player;
	private GameObject defaultCamera;
	private PocketInventory playerInventory;
	private SaveManager savePoint;

	private void Awake()
	{
		player = GameObject.FindGameObjectWithTag("Player");
		defaultCamera = GameObject.FindGameObjectWithTag("VCDefault");
		playerInventory = player.GetComponent<PocketInventory>();
		savePoint = FindObjectOfType<SaveManager>();
	}

	public void LoadSaveData()
	{
		savePoint.SaveCollider.enabled = true;

		player.transform.position = transform.position;
		player.transform.rotation = transform.rotation;

		playerInventory.PocketList[0] = savePoint.SaveData.healthPotion;
		playerInventory.PocketList[1] = savePoint.SaveData.staminaPotion;
		playerInventory.PocketList[2] = savePoint.SaveData.baseDamagePotion;
		playerInventory.PocketList[3] = savePoint.SaveData.counterDamagePotion;
	}
}
