using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

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
      actorScript.actionTarget = (Actor.ActionTarget)EditorGUILayout.EnumFlagsField(new GUIContent("Action Target", "What kind of target this will actor will prioritize"), actorScript.actionTarget);
      actorScript.actionEffectSource = (Actor.ActionSource)EditorGUILayout.EnumFlagsField(new GUIContent("Action Effect Source", "Source of combat effect"), actorScript.actionEffectSource);
      actorScript.actionEffect = (Actor.ActionEffect)EditorGUILayout.EnumFlagsField(new GUIContent("Action Effect", "Effect source is making"), actorScript.actionEffect);
      actorScript.targetSelectionRule = (Actor.TargetSelectionRule)EditorGUILayout.EnumFlagsField(new GUIContent("Target Selection Rule", "How actor selects its target"), actorScript.targetSelectionRule);

      // Immunities
      showImmunities = EditorGUILayout.Foldout(showImmunities, new GUIContent($"Current Immunities: {AllImmunities(actorScript)}", "Open to set immunities"), true);
      if (showImmunities)
      {
        EditorGUI.indentLevel++;

        bool weapon = false, life = false, death = false, fire = false, earth = false, water = false, air = false;
        // Get the current status of each 
        List<Actor.ActionSource> immunitiesList = actorScript.immunities.OfType<Actor.ActionSource>().ToList();
        foreach (Actor.ActionSource source in immunitiesList)
          switch (source)
          {
            case Actor.ActionSource.Weapon:
              weapon = true;
              break;
            case Actor.ActionSource.Life:
              life = true;
              break;
            case Actor.ActionSource.Death:
              death = true;
              break;
            case Actor.ActionSource.Fire:
              fire = true;
              break;
            case Actor.ActionSource.Earth:
              earth = true;
              break;
            case Actor.ActionSource.Water:
              water = true;
              break;
            case Actor.ActionSource.Air:
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

        UpdateImmunitiesList(weapon, immunitiesList, Actor.ActionSource.Weapon);
        UpdateImmunitiesList(life, immunitiesList, Actor.ActionSource.Life);
        UpdateImmunitiesList(death, immunitiesList, Actor.ActionSource.Death);
        UpdateImmunitiesList(fire, immunitiesList, Actor.ActionSource.Fire);
        UpdateImmunitiesList(earth, immunitiesList, Actor.ActionSource.Earth);
        UpdateImmunitiesList(water, immunitiesList, Actor.ActionSource.Water);
        UpdateImmunitiesList(air, immunitiesList, Actor.ActionSource.Air);

        actorScript.immunities = immunitiesList.ToArray();

        EditorGUI.indentLevel--;
      }
      EditorGUI.indentLevel--;
    }
    #endregion

    // Board position
    #region Board position

    #endregion
  }

  private static void UpdateImmunitiesList(bool sourceTypeBool, List<Actor.ActionSource> immunitiesList, Actor.ActionSource source)
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
}
