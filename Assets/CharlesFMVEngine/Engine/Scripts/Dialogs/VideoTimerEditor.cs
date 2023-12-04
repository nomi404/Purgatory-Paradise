#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CharlesEngine
{
    public class VideoTimerEditor : EditorWindow
    {
        public Node VideoNode;
        [MenuItem("Tools/Charles Engine/Video Timer Editor", false, 304)]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            GetWindow(typeof(VideoTimerEditor));
        }
    
        bool IsDialogueSelected() // TODO refactor this method to a utils class, duplicates Condition Editor
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
                VideoNode = null;
            }
            if (VideoNode == null)
            {
                GUILayout.Label( "No video node selected", EditorStyles.centeredGreyMiniLabel );
                return;
            }

            bool dirty = false;
            GUILayout.Label("VideoNode "+VideoNode.ReadableID+" ("+VideoNode.ID+")", EditorStyles.boldLabel);
            float videoLength = 80;
            if (VideoNode.Video != null)
            {
                videoLength = (float) VideoNode.Video.length;
                GUILayout.Label(VideoNode.Video.name + " (" + videoLength + "s)");
            }
            else
            {
                GUILayout.Label("No videoclip assigned");
            }

            GUILayout.Space(20);
            var evts = VideoNode.TimedEvents;
            if (evts != null && evts.Count > 0)
            {
                int toDelete = -1;
                for (int i = 0; i < evts.Count; i++)
                {
                    var e = evts[i];
                    GUILayout.Label((i+1)+".", EditorStyles.boldLabel);
                    e.Listener = (CEScript) EditorGUILayout.ObjectField("Listener", e.Listener, typeof(CEScript), true);
                    if (e.Listener == null && GUILayout.Button("Create Script"))
                    {
                        var go = new GameObject("Script");
                        var scriptsRoot = GameObject.Find("Scripts");
                        go.transform.parent = scriptsRoot.transform;
                        e.Listener = go.AddComponent<MultiScript>();
                    }
                    e.Time = EditorGUILayout.Slider("Time", e.Time, 0f, videoLength);
                    e.RepeatOnEnd = EditorGUILayout.Toggle(new GUIContent("Repeat On End","This will fire this event at the end of the clip regardless if it was fired before. Use this to make sure it fires even when video is skipped."), e.RepeatOnEnd);
                    if (GUILayout.Button("DELETE", EditorStyles.miniButtonLeft,  GUILayout.Width(65)))
                    {
                        toDelete = i;
                    }
                }

                if (toDelete >= 0)
                {
                    evts.RemoveAt(toDelete);
                    dirty = true;
                }
            }
            else
            {
                GUILayout.Label("NO TIME EVENTS",EditorStyles.centeredGreyMiniLabel);
            }
            GUILayout.Space(30);
            if (GUILayout.Button("ADD MOVIE TIME EVENT"))
            {
                if (VideoNode.TimedEvents == null)
                {
                    VideoNode.TimedEvents = new List<VideoTimedEvent>();
                } 
                VideoNode.TimedEvents.Add(new VideoTimedEvent());
                dirty = true;
            }

            if (dirty)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); //to actual mark scene as dirty
            }
        }
    }
}
#endif