using TMPro;
using UnityEngine;

namespace CharlesEngine
{
	//[ExecuteInEditMode]
	[AddComponentMenu("CE Toolbox/Alpha Group")]
	public class AlphaGroup : MonoBehaviour
	{
		[HideInInspector][Range(0,1f)] public float Alpha;

		private SpriteRenderer[] _selectedSprites = {};
		private float[] _targetSpriteAlphas;

		private TextMeshPro[] _selectedTexts= {};
		private float[] _targetTextAlphas;

		private float _oldAlpha;

		void Awake()
		{
			if (Application.isPlaying)
			{
				// SPRITES
				_selectedSprites = GetComponentsInChildren<SpriteRenderer>();
				_targetSpriteAlphas = new float[_selectedSprites.Length];
				for (var i = 0; i < _selectedSprites.Length; i++)
				{
					_targetSpriteAlphas[i] = _selectedSprites[i].color.a;
				}

				// TEXTS
				_selectedTexts = GetComponentsInChildren<TextMeshPro>();
				_targetTextAlphas = new float[_selectedTexts.Length];
				for (var i = 0; i < _selectedTexts.Length; i++)
				{
					_targetTextAlphas[i] = _selectedTexts[i].color.a;
				}

				_oldAlpha = Alpha;
			}
			//	Debug.Log("Alpha group - sprites: "+_selectedSprites.Length+" texts: "+_selectedTexts.Length);
		}

		public void UpdateAlpha()
		{
			// Sprites
			for (var i = 0; i < _selectedSprites.Length; i++)
			{
				var render = _selectedSprites[i];
				if (render == null) continue;
				var c = render.color;
				c.a = Alpha;
				if (_targetSpriteAlphas != null && i < _targetSpriteAlphas.Length)
				{
					c.a *= _targetSpriteAlphas[i];
				}
				render.color = c;
			}
			// Texts
			for (var i = 0; i < _selectedTexts.Length; i++)
			{
				var render = _selectedTexts[i];
				var c = render.color;
				c.a = Alpha;
				if (_targetTextAlphas != null && i < _targetTextAlphas.Length)
				{
					c.a *= _targetTextAlphas[i];
				}
				render.color = c;
			}
		}

		public void SetA(float a)
		{
			Alpha = a;
			UpdateAlpha();
		}

		private void Reset()
		{
			Alpha = 1;
			// SPRITES
			_selectedSprites = GetComponentsInChildren<SpriteRenderer>();
			_targetSpriteAlphas = new float[_selectedSprites.Length];
			for (var i = 0; i < _selectedSprites.Length; i++)
			{
				_targetSpriteAlphas[i] = _selectedSprites[i].color.a;
			}
		
			// TEXTS
			_selectedTexts = GetComponentsInChildren<TextMeshPro>();
			_targetTextAlphas = new float[_selectedTexts.Length];
			for (var i = 0; i < _selectedTexts.Length; i++)
			{
				_targetTextAlphas[i] = _selectedTexts[i].color.a;
			}
			//	Debug.Log("Alpha group - sprites: "+_selectedSprites.Length+" texts: "+_selectedTexts.Length);
		}

#if UNITY_EDITOR
		void Update()
		{
			if (!Application.isPlaying)
			{
				if (transform.childCount != _selectedSprites.Length+_selectedTexts.Length)
				{
					_selectedSprites = GetComponentsInChildren<SpriteRenderer>();
					_selectedTexts = GetComponentsInChildren<TextMeshPro>();
				}
				if (_oldAlpha != Alpha)
				{
					_oldAlpha = Alpha;
					UpdateAlpha();
				}
			}
		}
#endif
	}
}