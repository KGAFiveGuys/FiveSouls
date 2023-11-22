using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SaveData
{
	public Vector3 respawnPosition;
	public Pocket healthPotion;
	public Pocket staminaPotion;
	public Pocket baseDamagePotion;
	public Pocket counterDamagePotion;
}

public class SaveManager : MonoBehaviour
{
	[field:SerializeField] public SaveData SaveData { get; private set; }

	public Collider SaveCollider { get; set; }
	private RespawnManager respawnPoint;

	private void Awake()
	{
		SaveCollider = GetComponent<BoxCollider>();
		respawnPoint = FindObjectOfType<RespawnManager>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			SaveCollider.enabled = false;

			var newSave = new SaveData();

			newSave.respawnPosition = respawnPoint.transform.position;
			var inventory = other.gameObject.GetComponent<PocketInventory>();
			newSave.healthPotion = inventory.PocketList[0];
			newSave.staminaPotion = inventory.PocketList[1];
			newSave.baseDamagePotion = inventory.PocketList[2];
			newSave.counterDamagePotion = inventory.PocketList[3];

			SaveData = newSave;
		}
	}
}
