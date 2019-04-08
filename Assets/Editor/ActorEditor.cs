﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(Actor))]
[CanEditMultipleObjects]
public class ActorEditor : Editor
{
  bool rareActor = false;
  bool showHealth = true, showCombat = true;
  bool showChooseImmunities = false, /*showChooseBoardPosition = false,*/ showChooseActionSource, showChooseActionTarget = false, showChooseActionEffect = false, showChooseTargetSelection = false;
  int maxHealthUpperBound = 1000;

  public override void OnInspectorGUI()
  {
    Actor actorScript = target as Actor;

    // Name
    actorScript.name = EditorGUILayout.TextField(new GUIContent("Actor Name", "Current actors name"), actorScript.name);
    rareActor = EditorGUILayout.Toggle(new GUIContent("Rare Actor", "Determind if actor should have base level stats aviliable"), rareActor);

    // Max HP, Current HP
    #region Health
    showHealth = EditorGUILayout.Foldout(showHealth, new GUIContent($"Current Health: {actorScript.hitPoints}", "Shows all health related info"), true);
    if (showHealth)
    {
      EditorGUI.indentLevel++;

      bool match = (actorScript.hitPoints == actorScript.maxHitPoints);
      int oldMaxHp = actorScript.maxHitPoints;

      // Current Health
      actorScript.hitPoints = EditorGUILayout.IntSlider(new GUIContent("Current health", "Rare actors limit is set to 1000 and can be adjusted over that"), actorScript.hitPoints, 0, actorScript.maxHitPoints);

      if (rareActor)
      {
        // Max Health
        if (maxHealthUpperBound > 1000)
          actorScript.maxHitPoints = EditorGUILayout.IntSlider(new GUIContent("Max Health", "Maximun health of the unit"), actorScript.maxHitPoints, 0, maxHealthUpperBound);
        else
          actorScript.maxHitPoints = EditorGUILayout.IntSlider(new GUIContent("Max Health", "Maximun health of the unit"), actorScript.maxHitPoints, 0, 1000);

        // Max Health Upper Bound
        maxHealthUpperBound = EditorGUILayout.IntField(new GUIContent("Max Health UpperBound", "The max that a charecters maximum hp can go"), maxHealthUpperBound);
      }
      else
      {
        // Max Health
        maxHealthUpperBound = 1000;
        actorScript.maxHitPoints = EditorGUILayout.IntSlider(new GUIContent("Max Health", "Maximun health of the unit"), actorScript.maxHitPoints, 0, 500);
      }



      // Check to match the current hp with max
      if (!Application.isPlaying && match && (oldMaxHp != actorScript.maxHitPoints))
        actorScript.hitPoints = actorScript.maxHitPoints;

      EditorGUI.indentLevel--;
    }
    #endregion

    // Current Target, initiative, damage, chance to hit, action target, action effect source, action effect, immunities
    #region Combat Stats
    string currentTargetName = "";
    if (actorScript.currentTarget != null)
      currentTargetName = actorScript.currentTarget.name;

    showCombat = EditorGUILayout.Foldout(showCombat, new GUIContent($"Current Target: {currentTargetName}", "Shows all combat stats and displays current target on dropdown"), true);
    if (showCombat)
    {
      EditorGUI.indentLevel++;

      // Target
      EditorGUILayout.ObjectField(new GUIContent("Current Target", "Actor that this actor is currently targeting"), actorScript.currentTarget, typeof(Actor), true);

      // Initiative
      int tempInitiative = EditorGUILayout.IntSlider(new GUIContent("Initiative", "Only factors of five"), actorScript.initiative, 10, 100);
      tempInitiative = Mathf.RoundToInt(tempInitiative / 5.0F) * 5;
      actorScript.initiative = tempInitiative;

      // Damage
      if (rareActor)
        actorScript.damage = EditorGUILayout.IntSlider(new GUIContent("Damage", "Rare actors has upper limit of 180"), actorScript.damage, 0, 180);
      else
        actorScript.damage = EditorGUILayout.IntSlider(new GUIContent("Damage", "Rare actors has upper limit of 180"), actorScript.damage, 0, 100);

      // Chance to hit
      actorScript.percentChanceToHit = EditorGUILayout.IntSlider(new GUIContent("Percent Chance to Hit", "Locked between 0 and 100"), actorScript.percentChanceToHit, 0, 100);

      // Target selection
      showChooseActionTarget = EditorGUILayout.Foldout(showChooseActionTarget, new GUIContent($"Action Target: {Enum.GetName(typeof(ActionTarget), actorScript.actionTarget)}"), true);
      if (showChooseActionTarget)
        actorScript.actionTarget = RadioButtonList(actorScript.actionTarget, 2);

      // Action Source
      showChooseActionSource = EditorGUILayout.Foldout(showChooseActionSource, new GUIContent($"Action Source: {actorScript.actionEffectSource}"), true);
      if (showChooseActionSource)
        actorScript.actionEffectSource = RadioButtonList(actorScript.actionEffectSource, 2);

      // Action Effect
      showChooseActionEffect = EditorGUILayout.Foldout(showChooseActionEffect, new GUIContent($"Action Effect: {actorScript.actionEffect}"), true);
      if (showChooseActionEffect)
        actorScript.actionEffect = RadioButtonList(actorScript.actionEffect, 2);

      // Target Selection Rule
      showChooseTargetSelection = EditorGUILayout.Foldout(showChooseTargetSelection, new GUIContent($"Target Selection Rule: {actorScript.targetSelectionRule.ToString()}"), true);
      if (showChooseTargetSelection)
        actorScript.targetSelectionRule = RadioButtonList(actorScript.targetSelectionRule, 2);

      // Immunities
      showChooseImmunities = EditorGUILayout.Foldout(showChooseImmunities, new GUIContent($"Current Immunities: {AllImmunities(actorScript)}", "Open to set immunities"), true);
      if (showChooseImmunities)
      {
        EditorGUI.indentLevel++;

        actorScript.immunities = CheckboxList(actorScript.immunities, 2);

        EditorGUI.indentLevel--;
      }
      EditorGUI.indentLevel--;
    }
    #endregion

    // Board position
    #region Board position

    #endregion


    EditorGUILayout.Space();
    EditorGUILayout.Space();
    EditorGUILayout.Space();
    EditorGUILayout.Space();
    EditorGUILayout.Space();

    actorScript.boardPosition = (Position)EditorGUILayout.EnumPopup("thing", actorScript.boardPosition);

    string[] splitPosition = (actorScript.boardPosition.ToString()).Split('_');

    bool[] positionInfo = new bool[2];
    string side = splitPosition[0];
    string forwardBack = splitPosition[1];
    string topToBot = splitPosition[2];

    if (side == "left")
      positionInfo[0] = true;
    else
      positionInfo[0] = false;

    EditorGUILayout.BeginHorizontal();
    bool temp = positionInfo[0];

    bool sideResult = ChooseSide(temp);

    //if (sideResult)
    //actorScript.boardPosition = Position.left_front_center;
    //else
    //actorScript.boardPosition = Position.right_front_center; 

    EditorGUILayout.EndHorizontal();

    string[] positionsLeft = new string[] {"Back Top\t", "Front Top", "Back Mid\t", "Front Mid", "Back Bot\t", "Front Bot" };
    string[] positionsRight = new string[] { "Front Top", "Back Top", "Front Mid", "Back Mid", "Front Bot", "Back Bot" };

    string thing = forwardBack + topToBot;

    bool[] positions = new bool[6];

    switch (thing)
    {
      case "fronttop":
        Debug.Log("Front Top");
        if (sideResult)
          positions[0] = true;
        else
          positions[1] = true;
        break;

      case "frontcenter":
        Debug.Log("Front Mid");
        if (sideResult)
          positions[3] = true;
        else
          positions[2] = true;
        break;

      case "frontbottom":
        Debug.Log("Front Bottom");
        break;
      case "backtop":
        Debug.Log("Back Top");
        break;
      case "backcenter":
        Debug.Log("Back Center");
        break;
      case "backbottom":
        Debug.Log("Back Bottom");
        break;
    }

    int num = 0;
    EditorGUILayout.BeginVertical();
    for (int r = 0; r < 3; r++)
    {
      EditorGUILayout.BeginHorizontal();
      for (int i = 0; i < 2; i++)
      {
        if (sideResult)
          GUILayout.Toggle(positions[num], positionsLeft[num]);
        else
          GUILayout.Toggle(positions[num], positionsRight[num]);

        num += 1;
      }
      EditorGUILayout.EndHorizontal();
    }
    EditorGUILayout.EndVertical();
  }

  private static bool ChooseSide(bool temp)
  {
    bool temp1 = EditorGUILayout.ToggleLeft("Left", temp);
    bool temp2 = !EditorGUILayout.ToggleLeft("Right", !temp);

    if (temp1 != temp)
    {
      Debug.Log("Choose Left");
      return true;
    }
    if (temp2 != temp)
    {
      Debug.Log("Choose Right");
      return false;
    }

    return temp;
  }

  private static void UpdateImmunitiesList(bool sourceTypeBool, List<ActionSource> immunitiesList, ActionSource source)
  {
    if (sourceTypeBool && !immunitiesList.Contains(source))
      immunitiesList.Add(source);
    else if (!sourceTypeBool && immunitiesList.Contains(source))
      immunitiesList.Remove(source);
  }

  private static string AllImmunities(Actor actorScript)
  {
    string immunitieString = "";
    for (int i = 0; i < actorScript.immunities.Length; i++)
    {
      immunitieString += actorScript.immunities[i].ToString();
      if (i != actorScript.immunities.Length - 1)
        immunitieString += ", ";
    }

    return immunitieString;
  }

  /// <summary>
  /// Takes an enum and creates a toggle list out of the possible enum for that selection
  /// </summary>
  /// <typeparam name="T">Enum type</typeparam>
  /// <param name="label">Lable in front of the list</param>
  /// <param name="selectedValuesArray">This is the values that are currently selected</param>
  /// <param name="itemsPerRow">Number of choices that will be on each row</param>
  /// <returns>The selected values updated to what has been selected</returns>
  private static T[] CheckboxList<T>(string label, T[] selectedValuesArray, int itemsPerRow) where T : Enum
  {
    T[] possibleValues = Enum.GetValues(typeof(T)) as T[];
    string[] valueNames = Enum.GetNames(typeof(T)) as string[];

    // Get the array and turn it into a list
    List<T> selectedValuesList = selectedValuesArray.OfType<T>().ToList();
    // Make a bool array of the same size as possible values
    bool[] boolArr = new bool[possibleValues.Length];

    // go though the whole bool array
    for (int i = 0; i < boolArr.Length; i++)
    {
      // As we go though each one check if thats possible values is contained within the selected ones
      if (selectedValuesList.Contains(possibleValues[i]))
        // if it is set that bool to true
        boolArr[i] = true;
      else
        // if not set it to false
        boolArr[i] = false;
    }

    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.LabelField(label, GUILayout.MaxWidth(100));

    EditorGUILayout.BeginVertical();
    for (int r = 0; r < boolArr.Length; r += itemsPerRow)
    {
      EditorGUILayout.BeginHorizontal();
      for (int i = r; i < r + itemsPerRow && i < boolArr.Length; i++)
      {
        if (valueNames[i].Length > 5)
          boolArr[i] = GUILayout.Toggle(boolArr[i], valueNames[i] + "\t");
        else
          boolArr[i] = GUILayout.Toggle(boolArr[i], valueNames[i] + "\t");

        if (boolArr[i] && !selectedValuesList.Contains(possibleValues[i]))
          selectedValuesList.Add(possibleValues[i]);
        else if (!boolArr[i] && selectedValuesList.Contains(possibleValues[i]))
          selectedValuesList.Remove(possibleValues[i]);
      }
      EditorGUILayout.EndHorizontal();
    }
    EditorGUILayout.EndVertical();

    EditorGUILayout.EndHorizontal();

    return selectedValuesList.ToArray();
  }

  /// <summary>
  /// Takes an enum and creates a toggle list out of the possible enum for that selection
  /// </summary>
  /// <typeparam name="T">Enum type</typeparam>
  /// <param name="selectedValuesArray">This is the values that are currently selected</param>
  /// <param name="itemsPerRow">Number of choices that will be on each row</param>
  /// <returns>The selected values updated to what has been selected</returns>
  private static T[] CheckboxList<T>(T[] selectedValuesArray, int itemsPerRow) where T : Enum
  {
    // All possible values for the enum
    T[] possibleValues = Enum.GetValues(typeof(T)) as T[];
    // size to track array sizes
    int arraySize = possibleValues.Length;
    // Possible names for the enum
    string[] valueNames = Enum.GetNames(typeof(T)) as string[];
    // Bool to track each option true or false
    bool[] boolArr = new bool[arraySize];
    // List of selected values
    List<T> selectedValuesList = selectedValuesArray.OfType<T>().ToList();

    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.BeginVertical();

    for (int r = 0; r < arraySize; r += itemsPerRow)
    {
      EditorGUILayout.BeginHorizontal();
      for (int i = r; i < r + itemsPerRow && i < arraySize; i++)
      {
        if (selectedValuesList.Contains(possibleValues[i]))
          boolArr[i] = true;
        else
          boolArr[i] = false;


        boolArr[i] = EditorGUILayout.ToggleLeft(valueNames[i] + "\t", boolArr[i]);

        if (boolArr[i] && !selectedValuesList.Contains(possibleValues[i]))
          selectedValuesList.Add(possibleValues[i]);
        else if (!boolArr[i] && selectedValuesList.Contains(possibleValues[i]))
          selectedValuesList.Remove(possibleValues[i]);
      }
      EditorGUILayout.EndHorizontal();
    }

    EditorGUILayout.EndVertical();
    EditorGUILayout.EndHorizontal();

    return selectedValuesList.ToArray();
  }

  /// <summary>
  /// Takes Enum and creates a radio list out of possible enums
  /// </summary>
  /// <typeparam name="T">Enum type</typeparam>
  /// <param name="selectedValue">This is the value that is currently selected</param>
  /// <param name="itemsPerRow">Number of choices that will be on each row</param>
  /// <returns>The value that is selected by the radio buttons</returns>
  private static T RadioButtonList<T>(T selectedValue, int itemsPerRow) where T : Enum
  {
    T[] possibleValues = Enum.GetValues(typeof(T)) as T[];
    string[] valueNames = Enum.GetNames(typeof(T)) as string[];
    int listLength = possibleValues.Length;

    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.BeginVertical();
    for (int r = 0; r < listLength; r+= itemsPerRow)
    {
      EditorGUILayout.BeginHorizontal();
      for (int i = r; i < r + itemsPerRow && i < listLength; i++)
      {
        if (possibleValues[i].Equals(selectedValue))
        {
          if (EditorGUILayout.ToggleLeft(valueNames[i] + "\t", true))
            selectedValue = possibleValues[i];
        }
        else
        {
          if (EditorGUILayout.ToggleLeft(valueNames[i] + "\t", false))
            selectedValue = possibleValues[i];
        }
      }
      EditorGUILayout.EndHorizontal();
    }

    EditorGUILayout.EndHorizontal();
    EditorGUILayout.EndVertical();

    return selectedValue;
  }
}
