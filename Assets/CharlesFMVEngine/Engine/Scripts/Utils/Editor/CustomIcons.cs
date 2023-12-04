/*The MIT License (MIT)
Copyright (c) 2016 Edward Rowe (@edwardlrowe)
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace CharlesEngine
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws a Comment Icon on GameObjects in the Hierarchy
    /// </summary>
    [InitializeOnLoad]
    public class CustomIcons
    {
        private static readonly Texture2D AppleIcon;
        private static readonly Texture2D DialogsIcon;
        private static readonly Texture2D RootIcon;
        private static readonly Texture2D SceneManagerIcon;

        static CustomIcons()
        {
            var guids = new[] {"c251abf266918384eadd0d1f30356775", "e3a17ed346a522045a01c277be753f4f", "0c95b6d35ac57d040aec0c02522b2ef4"};
            var paths = new[] {"Assets/CharlesFMVEngine/Gizmos/Dialogs.png", "Assets/CharlesFMVEngine/Gizmos/Vine.png", "Assets/CharlesFMVEngine/Gizmos/SceneManager.png"};
            
            DialogsIcon = LoadGizmosTexture(guids[0], paths[0]);
            RootIcon = LoadGizmosTexture(guids[1], paths[1]);
            SceneManagerIcon = LoadGizmosTexture(guids[2], paths[2]);

            if (DialogsIcon != null)
            {
                EditorApplication.hierarchyWindowItemOnGUI += DrawIconOnWindowItem;
            }
        }

        private static Texture2D LoadGizmosTexture(string guid, string p)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
            {
                path = p;
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static void DrawIconOnWindowItem(int instanceID, Rect rect)
        {
            GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (gameObject == null)
            {
                return;
            }

            if (gameObject.GetComponent<CESceneRoot>() != null)
            {
                DrawIcon(rect, RootIcon);
                return;
            }

            if (gameObject.GetComponent<Dialogues>() != null)
            {
                DrawIcon(rect, DialogsIcon);
                return;
            }
            
            if (gameObject.GetComponent<CEScene>() != null)
            {
                DrawIcon(rect, SceneManagerIcon);
                return;
            }
        }

        public static void DrawIcon(Rect rect, Texture2D Icon, float iconWidth = 15)
        {
            if (Icon == null) return;
            EditorGUIUtility.SetIconSize(new Vector2(iconWidth, iconWidth));
            var padding = new Vector2(5, 0);

            var iconDrawRect = new Rect(
                rect.xMax - (iconWidth + padding.x),
                rect.yMin,
                rect.width,
                rect.height);

            //var iconDrawRect = new Rect(
            //                       28+padding.x,
            //                       rect.yMin,
            //                       28 + padding.x + iconWidth,
            //                       rect.height);

            var iconGUIContent = new GUIContent(Icon);
            EditorGUI.LabelField(iconDrawRect, iconGUIContent);
            EditorGUIUtility.SetIconSize(Vector2.zero);
        }
    }
}