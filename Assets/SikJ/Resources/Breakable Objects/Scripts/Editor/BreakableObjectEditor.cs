using UnityEngine;
using System;
using UnityEditor;
/****************************************
	BreakableObject Editor v1.09			
	Copyright 2013 Unluck Software	
 	www.chemicalbliss.com																																				
*****************************************/
[CustomEditor(typeof(BreakableObject))]
[CanEditMultipleObjects]

[System.Serializable]
public class BreakableObjectEditor: Editor {
    public override void OnInspectorGUI() {
    	var target_cs = (BreakableObject)target;

        EditorGUILayout.LabelField("Spawn Potions", EditorStyles.miniLabel);
        target_cs.potionSpawnWeight = EditorGUILayout.CurveField(new GUIContent("Propbablility Weight", "Spawn if value above .8f"), target_cs.potionSpawnWeight, null);
        target_cs.potionSpawnOffsetY = EditorGUILayout.FloatField("OffsetY", target_cs.potionSpawnOffsetY);
        target_cs.healthRegenBoostPotion = (GameObject)EditorGUILayout.ObjectField("Health Regen Boost Potion Prfeab", target_cs.healthRegenBoostPotion, typeof(GameObject), false);
        target_cs.staminaRegenBoostPotion = (GameObject)EditorGUILayout.ObjectField("Stamina Regen Boost Potion Prfeab", target_cs.staminaRegenBoostPotion, typeof(GameObject), false);
        target_cs.baseDamageBoostPotion = (GameObject)EditorGUILayout.ObjectField("Base Damage Boost Potion Prfeab", target_cs.baseDamageBoostPotion, typeof(GameObject), false);
        target_cs.counterDamageBoostPotion = (GameObject)EditorGUILayout.ObjectField("Counter Damage Boost Potion Prfeab", target_cs.counterDamageBoostPotion, typeof(GameObject), false);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Drag & Drop", EditorStyles.miniLabel);
    	target_cs.fragments = (Transform)EditorGUILayout.ObjectField("Fractured Object Prefab", target_cs.fragments, typeof(Transform) ,false );
    	target_cs.breakParticles = (ParticleSystem)EditorGUILayout.ObjectField("Particle System Prefab", target_cs.breakParticles, typeof(ParticleSystem) ,false);
    	EditorGUILayout.Space();
    	EditorGUILayout.LabelField("Seconds before removing fragment colliders (zero = never)", EditorStyles.miniLabel);   	
    	target_cs.waitForRemoveCollider = EditorGUILayout.FloatField("Remove Collider Delay" , target_cs.waitForRemoveCollider);
    	EditorGUILayout.Space();
    	EditorGUILayout.LabelField("Seconds before removing fragment rigidbodies (zero = never)", EditorStyles.miniLabel);   	
    	target_cs.waitForRemoveRigid = EditorGUILayout.FloatField("Remove Rigidbody Delay" , target_cs.waitForRemoveRigid);	
    	EditorGUILayout.Space();
  		EditorGUILayout.LabelField("Seconds before removing fragments (zero = never)", EditorStyles.miniLabel);   	
    	target_cs.waitForDestroy = EditorGUILayout.FloatField("Destroy Fragments Delay" , target_cs.waitForDestroy);	
    	EditorGUILayout.Space();
    	EditorGUILayout.LabelField("Force applied to fragments after object is broken", EditorStyles.miniLabel);   
    	target_cs.explosiveForce = EditorGUILayout.FloatField("Fragment Force" , target_cs.explosiveForce);
    	EditorGUILayout.Space();
    	EditorGUILayout.LabelField("How hard must object be hit before it breaks", EditorStyles.miniLabel);   	
    	target_cs.durability = EditorGUILayout.FloatField("Object Durability" , target_cs.durability);	
    	target_cs.mouseClickDestroy = EditorGUILayout.Toggle("Click To Break Object" , target_cs.mouseClickDestroy);
        if (GUI.changed)
            EditorUtility.SetDirty (target_cs);
    }
}