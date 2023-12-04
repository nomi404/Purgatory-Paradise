using System;
using System.Collections.Generic;
#if CE_USE_I2Loc
using I2.Loc;
#endif
using TMPro;
using UnityEngine;

namespace CharlesEngine
{
    [AddComponentMenu("CE Scripts/Set Text")]
    public class SetText : CEScript
    {
        public TextMeshPro Target;
        [Tooltip("@@ is replaced with a newline automatically")]
#if CE_USE_I2Loc
        public string[] Texts;
#else
        public string Text;
#endif
        
#if CE_USE_I2Loc
        public void SetTexts(Dictionary<LangEnum, string> txts)
        {
            Texts = new string[4];
            foreach (var keyValuePair in txts)
            {
                Texts[(int) keyValuePair.Key] = keyValuePair.Value;
            }
        }
#endif
        public override void Run()
        {
            if (Target != null) Target.text = GetText();
        }

        public string GetText()
        {
            string text = null;
#if CE_USE_I2Loc
            LangEnum le;
            var success = Enum.TryParse(LocalizationManager.CurrentLanguageCode, out le);
            if (success)
            {
                text = Texts[(int) le].Replace("@@","\n");
            }
            else
            {
                Debug.LogError("current lang not parsable" + Globals.Lang);
                text = Texts[0];
            }
#else
            text = Text;
#endif
            return text.Replace("@@","\n");;
        }
    }
}