#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CharlesEngine
{
	public class ConditionEditor : EditorWindow
	{
		public Condition Condition;
		public Object TargetObject;

		public static Condition ClipBoardCopy;
		
		// Add menu item named "ConditionEditor" to the Window menu
		[MenuItem("Tools/Charles Engine/Condition Editor", false, 303)]
		public static void ShowWindow()
		{
			//Show existing window instance. If one doesn't exist, make one.
			GetWindow(typeof(ConditionEditor), true, "Condition Editor");
		}
	
		bool IsDialogueSelected()
		{
			if (Selection.activeTransform)
			{
				var currentDialogue = Selection.activeTransform.GetComponent<Dialogues>();
				if (currentDialogue != null)
				{
					return true;
				}

				var currentConditional = Selection.activeTransform.GetComponent<ConditionalScript>();
				if (currentConditional != null)
				{
					return true;
				}
				
				var currentObjectVisibleConditional = Selection.activeTransform.GetComponent<ObjectVisibleIf>();
				if (currentObjectVisibleConditional != null)
				{
					return true;
				}
			
				var currentDialogStartConditional = Selection.activeTransform.GetComponent<DialogStartCondition>();
				if (currentDialogStartConditional != null)
				{
					return true;
				}
			}
			return false;		
		}

		void OnGUI()
		{
			if (!IsDialogueSelected())
			{
				Condition = null;
			}

			Color orig = EditorStyles.centeredGreyMiniLabel.normal.textColor;
			EditorStyles.centeredGreyMiniLabel.normal.textColor = GUI.color;//Color.black;
			
			if (Condition == null)
			{
				
				GUILayout.Label( "No condition selected", EditorStyles.centeredGreyMiniLabel );
				return;
			}

			if (Condition.AndGroups == null)
			{
				Condition.AndGroups = new List<AndGroup>();
			}
		
			if (Condition.Custom == null && Condition.AndGroups.Count > 0)
			{
				GUILayout.Space(25);
				GUILayout.Label( Condition.ToString(), EditorStyles.centeredGreyMiniLabel );
				GUILayout.Space(25);
			}

			EditorStyles.centeredGreyMiniLabel.normal.textColor = orig;
		
			//font style for delete buttons
			var delStyle = new GUIStyle(GUI.skin.button);
			delStyle.normal.textColor = new Color(0.5f, 0.1f, 0.1f);
			delStyle.fontSize = 8;
			
			var varStyle = new GUIStyle(GUI.skin.button);
			varStyle.normal.textColor = new Color(0.1f, 0.1f, 0.1f);
			varStyle.fontSize = 8;
			EditorGUI.BeginChangeCheck();

			bool anyChanged = false; // used to track changes, to mark the scene dirty
		
		//	GUILayout.Label("&& groups", EditorStyles.boldLabel);
			AndGroup groupToDelete = null;
			for (var i = 0; i < Condition.AndGroups.Count; i++)
			{
				var group = Condition.AndGroups[i];
				if (group.Expressions == null)
				{
					group.Expressions = new List<Expression>();
				}
				if (group.Expressions.Count == 0)
				{
					group.Expressions.Add(new Expression());
				}

				//======== EXPRESIONS ===================
				Expression toDelete = null;
				foreach (var exp in group.Expressions)
				{
					GUILayout.Space(8);
					GUILayout.BeginHorizontal();
					if (!Application.isPlaying && GUILayout.Button(new GUIContent("X", "Delete expression"), delStyle, GUILayout.Width(18)))
					{
						toDelete = exp;
					}
					if (exp.Variable is BoolVariable || exp.Variable is StringSetVariable)
					{
						exp.ComparisonVarValue = null;
						exp.CompareToAnotherVariable = false;
						EditorGUIUtility.labelWidth = 30;
						var notVal = EditorGUILayout.Toggle("NOT:", exp.Not);
						if (notVal != exp.Not)
						{
							exp.Not = notVal;
							anyChanged = true;
						}
						EditorGUIUtility.labelWidth = 0;
					}

					var wasVar = exp.Variable;
					exp.Variable = (BaseVariable) EditorGUILayout.ObjectField(exp.Variable, typeof(BaseVariable), true);
					if (wasVar != exp.Variable)
					{
						anyChanged = true;
					}
					if (exp.Variable is IntVariable)
					{
						var compareVal = (FloatComparator) EditorGUILayout.EnumPopup(exp.Comparator);
						if (exp.Comparator != compareVal)
						{
							exp.Comparator = compareVal;
							anyChanged = true;
						}
						
						if (exp.CompareToAnotherVariable)
						{
							var compareValVal = (BaseVariable) EditorGUILayout.ObjectField(exp.ComparisonVarValue, exp.Variable.GetType(), true);	
							if ( exp.ComparisonVarValue != compareValVal)
							{
								exp.Comparator = compareVal;
								exp.ComparisonVarValue = compareValVal;
								anyChanged = true;
							}
						}
						else
						{
							var compareValVal = EditorGUILayout.IntField(exp.ComparisonValue);
							if (exp.ComparisonValue != compareValVal)
							{
								exp.Comparator = compareVal;
								exp.ComparisonValue = compareValVal;
								anyChanged = true;
							}
						}
						
						
						
						if (!Application.isPlaying &&  
							    GUILayout.Button(new GUIContent(exp.CompareToAnotherVariable ? "#" : "Θ", exp.CompareToAnotherVariable ? "Compare to value" : "Compare to another variable"), varStyle,
								    GUILayout.Width(20),GUILayout.Height(18)))
						{
								exp.CompareToAnotherVariable = !exp.CompareToAnotherVariable;
						}
					}else
					if (exp.Variable is IEnumVariable)
					{
						var enumVar = exp.Variable as IEnumVariable;
						var enumVal = enumVar.GetValueAsEnum();
						var enumValType = enumVal.GetType();
						
						var compareVal = (FloatComparator) EditorGUILayout.EnumPopup(exp.Comparator);
						Enum enumval = (Enum) Enum.ToObject(enumValType, exp.ComparisonValue);
						var compareValVal = Convert.ToInt32( EditorGUILayout.EnumPopup(enumval) );
						if (exp.Comparator != compareVal || exp.ComparisonValue != compareValVal)
						{
							exp.Comparator = compareVal;
							exp.ComparisonValue = compareValVal;
							anyChanged = true;
						}
					}else
					if (exp.Variable is StringSetVariable)
					{
						var compareVal = EditorGUILayout.TextField("contains", exp.StringSetContainsValue);
						if (exp.StringSetContainsValue != compareVal )
						{
							exp.StringSetContainsValue = compareVal;
							anyChanged = true;
						}
					}else
					if (exp.Variable != null && !(exp.Variable is BoolVariable))
					{
						// any other variable, lets default to a comparism with another variable of the same type
						exp.CompareToAnotherVariable = true;
						
						var compareVal = (FloatComparator) EditorGUILayout.EnumPopup(exp.Comparator);
						if (exp.Comparator != compareVal)
						{
							exp.Comparator = compareVal;
							anyChanged = true;
						}
						var compareValVal = (BaseVariable) EditorGUILayout.ObjectField(exp.ComparisonVarValue, exp.Variable.GetType(), true);
						if (exp.ComparisonVarValue != compareValVal)
						{
							exp.Comparator = compareVal;
							exp.ComparisonVarValue = compareValVal;
							anyChanged = true;
						}
					}

					GUILayout.EndHorizontal();
				}
				if (toDelete != null)
				{
					group.Expressions.Remove(toDelete);
				}
				//========
				GUILayout.Space(5);
				GUILayout.BeginHorizontal();
				if (!Application.isPlaying && GUILayout.Button(new GUIContent("AND", "Add expression"), GUILayout.Width(60),GUILayout.Height(40)))
				{
					group.Expressions.Add(new Expression());
					anyChanged = true;
				}
			
				GUILayout.Space(EditorGUIUtility.currentViewWidth-152);

				if (!Application.isPlaying && GUILayout.Button("Delete group", delStyle, GUILayout.Width(82), GUILayout.Height(18)))
				{
					groupToDelete = group;
				}
				GUILayout.EndHorizontal();
				if (i < Condition.AndGroups.Count - 1)
				{
					GUILayout.Space(5);
					GUILayout.Label("- OR -", EditorStyles.boldLabel);
					GUILayout.Space(5);
				}
			}
			
			GUILayout.Space(5);
			if (!Application.isPlaying && GUILayout.Button(Condition.AndGroups.Count > 0 ? "OR" : "Start", GUILayout.Width(60), GUILayout.Height(40)))
			{
				Condition.AndGroups.Add(new AndGroup());
				anyChanged = true;
			}
	
			if (groupToDelete != null)
			{
				Condition.AndGroups.Remove(groupToDelete);
				anyChanged = true;
			}
			if (anyChanged && TargetObject != null)
			{
				EditorUtility.SetDirty(TargetObject);
				Undo.RecordObject(TargetObject, "edit condition");
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); //to actual mark scene as dirty
			}
		
			// CUSTOM CONDITION 
			GUILayout.Space(20);
			GUILayout.Label("Use Custom Condition", EditorStyles.boldLabel);
			Condition.Custom = (CustomCondition) EditorGUILayout.ObjectField(Condition.Custom, typeof(CustomCondition), true);

			GUILayout.FlexibleSpace();
						
			if (GUILayout.Button("Copy", GUILayout.Width(50)))
			{
				ClipBoardCopy = Condition.GetCopy();
			}

			if (ClipBoardCopy != null)
			{
				if (GUILayout.Button("Paste", GUILayout.Width(50)))
				{
					Condition.AndGroups = ClipBoardCopy.GetCopy().AndGroups;
				}
			}
		}	
	}
}
#endif