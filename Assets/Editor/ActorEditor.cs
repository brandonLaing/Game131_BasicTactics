using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(Actor))]
[CanEditMultipleObjects]
public class ActorEditor : Editor
{
  bool showHealth = false, showCombat = false, rareActor = false, showImmunities = false, showBoardPosition = false;
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

      actorScript.hitPoints = EditorGUILayout.IntSlider(new GUIContent("Current health", "Rare actors limit is set to 1000 and can be adjusted over that"), actorScript.hitPoints, 0, actorScript.maxHitPoints);

      if (rareActor)
      {
        if (maxHealthUpperBound > 1000)
          actorScript.maxHitPoints = EditorGUILayout.IntSlider(new GUIContent("Max Health", "Maximun health of the unit"), actorScript.maxHitPoints, 0, maxHealthUpperBound);
        else
          actorScript.maxHitPoints = EditorGUILayout.IntSlider(new GUIContent("Max Health", "Maximun health of the unit"), actorScript.maxHitPoints, 0, 1000);
        maxHealthUpperBound = EditorGUILayout.IntField(new GUIContent("Max Health UpperBound", "The max that a charecters maximum hp can go"), maxHealthUpperBound);

      }
      else
      {
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
    showCombat = EditorGUILayout.Foldout(showCombat, new GUIContent($"Current Target: {actorScript.currentTarget.name}", "Shows all combat stats and displays current target on dropdown"), true);
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
      actorScript.actionTarget = (ActionTarget)EditorGUILayout.EnumFlagsField(new GUIContent("Action Target", "What kind of target this will actor will prioritize"), actorScript.actionTarget);
      actorScript.actionEffectSource = (ActionSource)EditorGUILayout.EnumFlagsField(new GUIContent("Action Effect Source", "Source of combat effect"), actorScript.actionEffectSource);
      actorScript.actionEffect = (ActionEffect)EditorGUILayout.EnumFlagsField(new GUIContent("Action Effect", "Effect source is making"), actorScript.actionEffect);
      actorScript.targetSelectionRule = (TargetSelectionRule)EditorGUILayout.EnumFlagsField(new GUIContent("Target Selection Rule", "How actor selects its target"), actorScript.targetSelectionRule);

      // Immunities
      showImmunities = EditorGUILayout.Foldout(showImmunities, new GUIContent($"Current Immunities: {AllImmunities(actorScript)}", "Open to set immunities"), true);
      if (showImmunities)
      {
        EditorGUI.indentLevel++;

        bool weapon = false, life = false, death = false, fire = false, earth = false, water = false, air = false;
        // Get the current status of each 
        List<ActionSource> immunitiesList = actorScript.immunities.OfType<ActionSource>().ToList();
        foreach (ActionSource source in immunitiesList)
          switch (source)
          {
            case ActionSource.Weapon:
              weapon = true;
              break;
            case ActionSource.Life:
              life = true;
              break;
            case ActionSource.Death:
              death = true;
              break;
            case ActionSource.Fire:
              fire = true;
              break;
            case ActionSource.Earth:
              earth = true;
              break;
            case ActionSource.Water:
              water = true;
              break;
            case ActionSource.Air:
              air = true;
              break;
          }

        weapon = EditorGUILayout.Toggle("Weapon", weapon);
        life = EditorGUILayout.Toggle("Life", life);
        death = EditorGUILayout.Toggle("Death", death);
        fire = EditorGUILayout.Toggle("Fire", fire);
        earth = EditorGUILayout.Toggle("Earth", earth);
        water = EditorGUILayout.Toggle("Water", water);
        air = EditorGUILayout.Toggle("Air", air);

        bool[] effects = { weapon, life, death, fire, earth, water, air };
        ActionSource[] actionSources = 
        {
          ActionSource.Weapon, ActionSource.Life, ActionSource.Death, ActionSource.Fire,
          ActionSource.Earth, ActionSource.Water, ActionSource.Air
        };

        UpdateImmunitiesList(weapon, immunitiesList, ActionSource.Weapon);
        UpdateImmunitiesList(life, immunitiesList, ActionSource.Life);
        UpdateImmunitiesList(death, immunitiesList, ActionSource.Death);
        UpdateImmunitiesList(fire, immunitiesList, ActionSource.Fire);
        UpdateImmunitiesList(earth, immunitiesList, ActionSource.Earth);
        UpdateImmunitiesList(water, immunitiesList, ActionSource.Water);
        UpdateImmunitiesList(air, immunitiesList, ActionSource.Air);

        actorScript.immunities = immunitiesList.ToArray();

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

    actorScript.immunities = 
      CheckboxList("Immunities", actorScript.immunities, 
      Enum.GetValues(typeof(ActionSource)) as ActionSource[],
      Enum.GetNames(typeof(ActionSource)) as string[], 3);

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
  /// Takes an enum and creates a toggle list out of the possible enums for that selection
  /// </summary>
  /// <typeparam name="T">Any enum</typeparam>
  /// <param name="label">Lable to show over list</param>
  /// <param name="selectedValuesArray">Values already selected</param>
  /// <param name="possibleValues">All possible values of enum to select from</param>
  /// <param name="valueNames">List of names for each corrosponding value</param>
  /// <param name="itemsPerRow">Number of selections per row</param>
  private static T[] CheckboxList<T>(string label, T[] selectedValuesArray, T[] possibleValues, string[] valueNames, int itemsPerRow) where T : Enum
  {
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
    // So i want to get an array which would be something like the immunities array from actor
    // I want to turn that array into a list of that enum. Then make a bool[] for each enum
    // and make the bool true or false based on if the list contains that enum. Then display a
    // toggle for each bool showing its correct state and get the responce back. Update the list
    // and return back the list as an array
  }
}
