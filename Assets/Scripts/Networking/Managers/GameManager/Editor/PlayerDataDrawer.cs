using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

[CustomPropertyDrawer(typeof(PlayerData))]
public class PlayerDataDrawer : PropertyDrawer
{
    SerializedProperty wood;
    SerializedProperty steel;
    SerializedProperty food;
    int propertyCount = 1;
    int marginHeight = 0;
    int propertyMargin = 10;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        propertyCount = 1;
        marginHeight = 0;

        wood = property.FindPropertyRelative("Wood");
        steel = property.FindPropertyRelative("Steel");
        food = property.FindPropertyRelative("Food");

        Rect foldoutRect = new(position.min.x, position.min.y, position.size.x, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);

        if (property.isExpanded)
        {
            propertyMargin = 2;
            DrawProperty(wood, position);
            DrawProperty(steel, position);
            DrawProperty(food, position);
        }

        EditorGUI.EndProperty();
    }

    private void DrawProperty(SerializedProperty property, Rect position)
    {
        float linesFromTop = propertyCount * EditorGUIUtility.singleLineHeight;

        Rect propertyRect = new(
            position.min.x,
            position.min.y + linesFromTop + marginHeight,
            position.size.x,
            EditorGUIUtility.singleLineHeight
        );

        propertyCount++;
        marginHeight += propertyMargin;

        GUI.enabled = false;
        EditorGUI.PropertyField(propertyRect, property);
        GUI.enabled = true;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = 1;

        if (property.isExpanded)
        {
            height += 3.5f;
        }
        
        return height * EditorGUIUtility.singleLineHeight;
    }
}
