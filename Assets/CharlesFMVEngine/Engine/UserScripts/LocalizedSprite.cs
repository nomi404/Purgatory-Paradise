using System;
using UnityEngine;
#if CE_USE_I2Loc
using I2.Loc;
#endif

namespace CharlesEngine
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(SpriteRenderer))]
	[AddComponentMenu("CE Toolbox/Localized Sprite")]
	public class LocalizedSprite : MonoBehaviour
	{
		public Sprite[] Sprites = new Sprite[5];

		void Awake()
		{
#if CE_USE_I2Loc
			if (string.IsNullOrEmpty(LocalizationManager.CurrentLanguage))
				return;

			OnLocalize();
#endif
		}

#if CE_USE_I2Loc && UNITY_EDITOR
		private void OnEnable()
		{
			if (!Application.isPlaying)
			{
				LocalizationManager.OnLocalizeEvent += OnLocalize;
			}
		}
	
		private void OnDisable()
		{
			if (!Application.isPlaying)
			{
				LocalizationManager.OnLocalizeEvent -= OnLocalize;
			}
		}
#endif

		private void OnLocalize()
		{
		
#if CE_USE_I2Loc 
			LangEnum le;
			Enum.TryParse(LocalizationManager.CurrentLanguageCode, out le);
			int index = (int) le;
			var sp = Sprites[index];
			if (sp == null)
			{
				Debug.LogWarning("Missing sprite for lang:" + Globals.Lang + " sprite:" + gameObject.name);
			}
			else
			{
				GetComponent<SpriteRenderer>().sprite = sp;
			}
#endif
		}
	}
}