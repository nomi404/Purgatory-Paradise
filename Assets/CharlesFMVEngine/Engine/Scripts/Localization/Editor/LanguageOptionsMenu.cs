#if CE_USE_I2Loc
using I2.Loc;
#endif
using UnityEditor;
using UnityEngine;

namespace CharlesEngine
{
	public class LanguageOptionsMenu : MonoBehaviour 
	{
#if CE_USE_I2Loc
		[MenuItem("Tools/Charles Engine/Language/CZ &1")]
		private static void SetCzech()
		{
			SetLang("cs");
		}
	
		[MenuItem("Tools/Charles Engine/Language/EN &2")]
		private static void SetEnglish()
		{
			SetLang("en");
		}
		
		[MenuItem("Tools/Charles Engine/Language/DE &3")]
		private static void SetDeutsch()
		{
			SetLang("de");
		}
		
		[MenuItem("Tools/Charles Engine/Language/RU &4")]
		private static void SetRussian()
		{
			SetLang("ru");
		}
#endif

#if CE_USE_I2Loc && UNITY_EDITOR
		[MenuItem("Tools/Charles Engine/Language/Save Text Position &S")]
		private static void SaveText()
		{
			if (Selection.gameObjects.Length == 1)
			{
				var ltp = Selection.gameObjects[0].GetComponent<LangTextPosition>();
				if (ltp != null)
				{
					ltp.Save();
				}
			}
		}
#endif
		private static void SetLang(string langcode)
		{
#if CE_USE_I2Loc
			if (LocalizationManager.Sources.Count == 0)
				LocalizationManager.UpdateSources();
		
			var l = LocalizationManager.GetLanguageFromCode(langcode);
			if( LocalizationManager.HasLanguage(l))
			{
				LocalizationManager.CurrentLanguage = l;
				Debug.Log("Switching lang to :"+l);
				if (Globals.Loaded)
				{
					Globals.Lang = langcode;
				}
			}
#endif
		}
	}
}
