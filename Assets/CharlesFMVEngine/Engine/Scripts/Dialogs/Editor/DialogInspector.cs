using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CharlesEngine
{
	[CustomEditor(typeof(Dialogues))]
	public class DialogInspector : UnityEditor.Editor 
	{
		public override void OnInspectorGUI()
		{
			Dialogues myTarget = (Dialogues)target;
			EditorGUILayout.LabelField("Trees ", myTarget.Trees.Count.ToString());
			int up = -1;
			int down = -1;
			var btnStyle = EditorStyles.miniButtonRight;
			btnStyle.normal.textColor = Color.black;
			btnStyle.font = EditorStyles.boldFont;
			btnStyle.fixedWidth = 35;
			for (var i = 0; i < myTarget.Trees.Count; i++)
			{
				var tree = myTarget.Trees[i];
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("["+i+"]", tree.Name + " ("+tree.Nodes.Count+" nodes)");
				if (i > 0 && GUILayout.Button("↑",btnStyle))
				{
					up = i;
				}
				if (i < myTarget.Trees.Count - 1 && GUILayout.Button("↓",btnStyle))
				{
					down = i;
				}
				EditorGUILayout.EndHorizontal();
			}

			if (up >= 0)
			{
				var tmp = myTarget.Trees[up];
				myTarget.Trees[up] = myTarget.Trees[up - 1];
				myTarget.Trees[up - 1] = tmp;
			}
		
			if (down >= 0)
			{
				var tmp = myTarget.Trees[down];
				myTarget.Trees[down] = myTarget.Trees[down + 1];
				myTarget.Trees[down + 1] = tmp;
			}
		
			if(GUILayout.Button("Edit"))
			{
				EditorWindow.GetWindow(typeof(DialogueEditorWindow), false, "Dialog Editor");
			}

			/*if (GUILayout.Button("FIX"))
			{
				for (var i = 0; i < myTarget.Trees.Count; i++)
				{
					var tree = myTarget.Trees[i];
					foreach (var treeNode in tree.Nodes)
					{
						var seet = new HashSet<int>(treeNode.Connections);
						if (seet.Count != treeNode.Connections.Count)
						{
							treeNode.Connections = new List<int>();
							foreach (var i1 in seet)
							{
								treeNode.Connections.Add(i1);
							}
							Debug.Log("fixed:"+treeNode.Type+" "+treeNode.ID);
						}
					}
				}
			}*/
		}
	}
}
