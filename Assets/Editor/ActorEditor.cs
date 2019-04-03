using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Actor))]
[CanEditMultipleObjects]
public class ActorEditor : Editor
{
  bool showHealth = false;
  int tempHolder = 0;

  public override void OnInspectorGUI()
  {
    Actor actorScript = target as Actor;

    actorScript.name = EditorGUILayout.TextField("Actor Name", actorScript.name);

    showHealth = EditorGUILayout.Foldout(showHealth, $"Current Health: {actorScript.hitPoints}");

    if (showHealth)
    {
      tempHolder = EditorGUILayout.IntSlider(tempHolder, 0, 100);
    }
  }
}
