using System;
using UnityEditor;
using UnityEngine;
#if CE_USE_I2Loc
using I2.Loc;
#endif

namespace CharlesEngine
{
	[CustomEditor(typeof(LocalizedSprite))]
	public class LocalizeSpriteInspector : Editor {

		public override void OnInspectorGUI()
		{
#if CE_USE_I2Loc
			LocalizedSprite myTarget = (LocalizedSprite) target;
			if (myTarget.Sprites == null || myTarget.Sprites.Length < 5)
			{
				myTarget.Sprites = new Sprite[5];	
			}

			for (int i = 0; i < 5; i++)
			{
				LangEnum le = (LangEnum) i;
				if (!Enum.IsDefined(typeof(LangEnum), i))
				{
					break;
				}
				var lang = LocalizationManager.GetLanguageFromCode(le.ToString());
				myTarget.Sprites[i] = (Sprite) EditorGUILayout.ObjectField(lang, myTarget.Sprites[i], typeof(Sprite), false);
			}
#else
	EditorGUILayout.HelpBox("I2Localization not available",MessageType.Warning);
#endif
		}
	}
}
