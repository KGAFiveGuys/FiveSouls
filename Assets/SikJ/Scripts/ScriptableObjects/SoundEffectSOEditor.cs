using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SoundEffectSO))]
[ExecuteAlways]
public class SoundEffectSOEditor : Editor
{
    SerializedProperty isFullPlay;
    SerializedProperty clip;
    SerializedProperty startVolume;
    SerializedProperty playRateToMute;

    private void OnEnable()
    {
        isFullPlay = serializedObject.FindProperty("isFullPlay");
        clip = serializedObject.FindProperty("clip");
        startVolume = serializedObject.FindProperty("startVolume");
        playRateToMute = serializedObject.FindProperty("playRateToMute");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(isFullPlay);
        EditorGUILayout.PropertyField(clip);
        EditorGUILayout.PropertyField(startVolume);

        if (!isFullPlay.boolValue)
        {
            EditorGUILayout.PropertyField(playRateToMute);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
