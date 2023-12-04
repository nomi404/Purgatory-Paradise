using System;
using UnityEditor;
using UnityEngine;

namespace CharlesEngine
{
	[CustomEditor(typeof(BoolVariable))]
	public class BoolVariableInspector : Editor 
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			DrawDefaultInspector();
			
			var bvariable = (BaseVariable) target;
			EditorGUILayout.LabelField("guid "+bvariable.Guid);
			if ( Application.isPlaying )
			{
				var variable = (BoolVariable) target;
				variable.RuntimeValue = EditorGUILayout.Toggle("Value:", variable.RuntimeValue);
			}
		}

		public override bool RequiresConstantRepaint()
		{
			return true;
		}
	}

	[CustomEditor(typeof(IntVariable),true)]
	public class IntVariableInspector : Editor 
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			DrawDefaultInspector();
			
			var bvariable = (BaseVariable) target;
			EditorGUILayout.LabelField("guid "+bvariable.Guid);
			if ( Application.isPlaying )
			{
				var variable = (IntVariable) target;
				variable.RuntimeValue = EditorGUILayout.IntField("Value:", variable.RuntimeValue);
			}
		}
		public override bool RequiresConstantRepaint()
		{
			return true;
		}
	}
	
	[CustomEditor(typeof(BaseVariable),true)]
	public class IEnumVariableInspector : Editor 
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			DrawDefaultInspector();

			var bvariable = (BaseVariable) target;
			EditorGUILayout.LabelField("guid "+bvariable.Guid);
			if ( Application.isPlaying && target is IEnumVariable)
			{
				var variable = (IEnumVariable) target;
				var rname = Enum.GetName(variable.GetValueAsEnum().GetType(), variable.GetValueAsEnum());
				EditorGUILayout.LabelField("Value:" + rname);
			}
		}
		public override bool RequiresConstantRepaint()
		{
			return true;
		}
	}
	
	[CustomEditor(typeof(StringSetVariable),true)]
	public class StringSetVariableInspector : Editor 
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			DrawDefaultInspector();
			var bvariable = (BaseVariable) target;
			EditorGUILayout.LabelField("guid "+bvariable.Guid);
			
			if ( Application.isPlaying )
			{
				var variable = (StringSetVariable) target;
				EditorGUILayout.LabelField("Value:");
				var vals = variable.RuntimeValue.List;
				int i = 0;
				foreach (var v in vals)
				{
					EditorGUILayout.LabelField(i+". "+v);
					i++;
				}
			}
		}
		public override bool RequiresConstantRepaint()
		{
			return true;
		}
	}
}