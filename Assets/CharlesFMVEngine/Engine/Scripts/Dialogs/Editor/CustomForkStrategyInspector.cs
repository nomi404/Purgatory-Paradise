using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CharlesEngine
{
    [CustomEditor(typeof(ForkNodeWithSafety), true)]
    public class CustomForkStrategySafetyInspector : CustomForkStrategyInspector
    {
        public override void OnInspectorGUI()
        {
            ForkNodeWithSafety myTarget = (ForkNodeWithSafety) target;
            myTarget.FallbackNodeId = EditorGUILayout.TextField("Fallback Node ID", myTarget.FallbackNodeId);
            base.OnInspectorGUI();
        }
    }

    [CustomEditor(typeof(CustomForkStrategy),true)]
    public class CustomForkStrategyInspector : Editor 
    {
        public override void OnInspectorGUI()
        {
            CustomForkStrategy myTarget = (CustomForkStrategy) target;
            SerializedProperty eevnt = serializedObject.FindProperty("Dialog");
            GUILayout.Label("Dialog",EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(eevnt);
            serializedObject.ApplyModifiedProperties();
            if (myTarget.Dialog == null)
            {
                return;
            }

            GUILayout.Label("Tree",EditorStyles.boldLabel);
            var d = myTarget.Dialog;
            
            GUI.changed = false;
            
            int index = string.IsNullOrWhiteSpace(myTarget.Tree) ? 0 : Array.IndexOf(d.TabNames, myTarget.Tree);
            if (index < 0)
            {
                index = 0;
            }

            int newIndex = EditorGUILayout.Popup(index, d.TabNames);
            if (newIndex != index)
            {
                myTarget.Tree = d.TabNames[newIndex];
            }

            if (string.IsNullOrEmpty(myTarget.Tree))
            {
                myTarget.Tree = d.TabNames[0];
            }
            
            if (GUI.changed)
            {
                Undo.RecordObject(myTarget.gameObject, "Changed script");
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); //to actual mark scene as dirty
            }
            
            GUILayout.Label("Fork",EditorStyles.boldLabel);
            
            GUI.changed = false;

            var treename = d.TabNames[newIndex];
            var forkNodes = new List<string>();
            forkNodes.Add("-");
            var ii = 0;
            foreach(var t in d.Trees)
            {
                if (t.Name == treename)
                {
                    foreach (var tNode in t.Nodes)
                    {
                        if (tNode.Type == NodeType.Fork)
                        {
                            if (myTarget.ForkId == tNode.ReadableID)
                            {
                                ii = forkNodes.Count;
                            }
                            forkNodes.Add(tNode.ReadableID);
                        }
                    }
                }
            }

            if (forkNodes.Count == 0)
            {
                GUILayout.Label("no forks found",EditorStyles.boldLabel);
                return;
            }

            var forkNodesAr = forkNodes.ToArray();
            
            int newIi = EditorGUILayout.Popup(ii, forkNodesAr);
            if (ii != newIi)
            {
                myTarget.ForkId = forkNodesAr[newIi];
            }
            
            if (string.IsNullOrEmpty(myTarget.ForkId))
            {
                myTarget.ForkId = forkNodesAr[0];
            }

            if (GUI.changed)
            {
                Undo.RecordObject(myTarget.gameObject, "Changed script");
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); //to actual mark scene as dirty
            }
        }
    }
    
    
    
    
    [CustomEditor(typeof(ForkWithTimer))]
    public class ForkWithTimerInspector : CustomForkStrategyInspector 
    {
        public override void OnInspectorGUI()
        {
            ForkWithTimer myTarget = (ForkWithTimer) target;
            EditorGUILayout.LabelField("Timer");
            myTarget.Timer = (Timer) EditorGUILayout.ObjectField( myTarget.Timer, typeof(Timer), true);
            base.OnInspectorGUI();
        }
    }
}