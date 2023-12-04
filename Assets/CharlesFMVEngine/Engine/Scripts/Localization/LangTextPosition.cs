using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if CE_USE_I2Loc
using I2.Loc;
#endif

using TMPro;

using UnityEngine;

namespace CharlesEngine
{
    [Serializable]
    public struct TextPosData
    {
        public float PosX;
        public float PosY;
        public float FontSize;
        public float Width;
        public float Height;
    }
    
    [ExecuteInEditMode]
    [AddComponentMenu("CE Toolbox/LangTextPosition")]
    [RequireComponent(typeof(TextMeshPro))]
    public class LangTextPosition : MonoBehaviour
    {
        public TextPosData[] Data;

        private void Awake()
        {
            Apply();
        }

        private void Apply()
        {
#if CE_USE_I2Loc
            if (Data == null) return;
            if (string.IsNullOrEmpty(LocalizationManager.CurrentLanguageCode))
                return;

            LangEnum enm;
            if (Enum.TryParse(LocalizationManager.CurrentLanguageCode, out enm))
            {
                int enmInt = (int) enm;
                if (Data.Length <= enmInt) return;
                var r = Data[enmInt];
                if (Math.Abs(r.PosX) < 0.01f && Math.Abs(r.Width) < 0.01f) return;

                var txt = GetComponent<TextMeshPro>();
                var rect = GetComponent<RectTransform>();
                var pos = new Vector3(r.PosX, r.PosY);
                rect.anchoredPosition = pos;
                var delt = new Vector2(r.Width, r.Height);
                rect.sizeDelta = delt;
                txt.fontSize = r.FontSize;
            }
    #endif
        }

        
#if CE_USE_I2Loc && UNITY_EDITOR

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                LocalizationManager.OnLocalizeEvent += Apply;
                EditorApplication.hierarchyChanged += Save;
            }
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
            {
                LocalizationManager.OnLocalizeEvent -= Apply;
                EditorApplication.hierarchyChanged -= Save;
            }
        }

        [EditorButton]
        public void Save()
        {
            if (LocalizationManager.Sources.Count == 0)
                LocalizationManager.UpdateSources();

            var current = LocalizationManager.CurrentLanguageCode;
            LangEnum enm;
            if (Enum.TryParse(current, out enm))
            {
                int enmInt = (int) enm;
                if (enmInt < 0 || enmInt > 66) return;
                var list = new List<TextPosData>(Data);
                while (list.Count <= enmInt)
                {
                    list.Add(new TextPosData());
                }

                list[enmInt] = ReadTextData();
                Data = list.ToArray();
            }
        }

        private TextPosData ReadTextData()
        {
            var r = new TextPosData();
            var txt = GetComponent<TextMeshPro>();
            var rect = GetComponent<RectTransform>();
            r.PosX = rect.anchoredPosition.x;
            r.PosY = rect.anchoredPosition.y;
            r.Width = rect.sizeDelta.x;
            r.Height = rect.sizeDelta.y;
            r.FontSize = txt.fontSize;
            return r;
        }

        private void Reset()
        {
            var list = new List<TextPosData>(Data);
            foreach(LangEnum foo in Enum.GetValues(typeof(LangEnum))){
                int enmInt = (int) foo;
                if (enmInt < 0 || enmInt > 66) return;       
                while (list.Count <= enmInt)
                {
                    list.Add(new TextPosData());
                }
                list[enmInt] = ReadTextData();
            }
            Data = list.ToArray();
        }
#endif
    }
}