using System;
#if UNITY_ANDROID || UNITY_IOS
#endif
using UnityEngine;
using Uween;
#if CE_USE_I2Loc
using I2.Loc;
#endif

namespace CharlesEngine
{
	public class DialogChoices : MonoBehaviour
	{
		
		public OnChoiceHandler OnChoiceSelected;
		// Set in inspector
		public GameObject Overlay;
		
		private SpriteRenderer _overlayRender;
		private IDialogChoiceLayout _layout;
		private GameObject _layoutObject;
		void Awake()
		{
			_overlayRender = Overlay.GetComponent<SpriteRenderer>();
			Overlay.SetActive(false);
			Init();
		}

		private void Init()
		{
			if (_layout == null && Globals.Settings != null)
			{
				if (Globals.Settings.DialogChoicePrefab == null)
				{
#if UNITY_EDITOR
					var settingsobject = Resources.Load<CEngineSettings>("CEngineSettings");
					Debug.LogError("Make sure to assign a dialog choice prefab in CESettings", settingsobject);
#endif
					throw new Exception("No dialog choice prefab found");
				}
				_layoutObject = Instantiate(Globals.Settings.DialogChoicePrefab, transform);
				_layout = _layoutObject.GetComponent<IDialogChoiceLayout>();
				if (_layout == null)
				{
					throw new Exception("Dialog choice layout not found in Dialog Prefab. Make sure the prefab has a component that implements the IDialogChoiceLayout interface.");
				}
			}
		}

		public void ReplaceLayout(GameObject layoutPrefab)
		{
			if (Overlay.activeSelf)
			{
				Debug.LogError("Cannot change choice layout, while choices are showing!");
				return;
			}

			if (layoutPrefab == null)
			{
				Debug.LogError("ReplaceLayout was called, but no prefab was supplied. Probably a missing reference.");
				return;
			}
			
			Destroy(_layoutObject);
			
			var layoutObject = Instantiate(layoutPrefab, transform);
			_layout = layoutObject.GetComponent<IDialogChoiceLayout>();
			if (_layout == null)
			{
				throw new Exception("Dialog choice layout not found in layout prefab. Make sure the prefab has a component that implements the IDialogChoiceLayout interface.");
			}
		}

		public void ShowChoices(Node[] answerNodes)
		{
			Init();
			StopAllCoroutines();
			Globals.Videos.OnVideoStartPlaying.RemoveListener(ReallyHide);
			_overlayRender.color = new Color(1,1,1,Globals.Settings.ForkOverlayAlpha);
			Overlay.SetActive(true);
			Overlay.PauseTweens();
			_layout.ShowLines(answerNodes.Length);

			for (int i = 0; i < answerNodes.Length; i++)
			{
				var line = _layout.Lines[i];
				if (line.IsActive())
				{
					line.SetText( GetLocalizedText(answerNodes[i]) );
					line.Node = answerNodes[i];
					line.OnClick.AddListener(OnAnswerClicked);
				}
			}
		}

		private string GetLocalizedText(Node node)
		{
#if CE_USE_I2Loc
			string locString = null;

			/*if (GenderVariable == null)
			{
				GenderVariable = Globals.Persistence.VariableManager.GetVariableByName("Gender") as GenderVariable;
			}

			if (GenderVariable != null && GenderVariable.RuntimeValue == Gender.Female)
			{
				LocalizedString localized = node.Text + "_f";
				locString = localized; //auto-translates to set language
			}*/
			
			if (string.IsNullOrEmpty(locString))
			{
				LocalizedString localized = node.Text;
				locString = localized; //auto-translates to set language
			}
			
			if (string.IsNullOrEmpty(locString))
			{
				locString = node.Text; //fall back on the orig text
				Debug.LogWarning("Unlocalized answer id:"+node.ID+" text:"+locString);
			}
			return locString;
#else
			return node.Text;
#endif
		}

		

		private void OnAnswerClicked(IAnswerLine line)
		{
			if (OnChoiceSelected == null)
			{
				Debug.LogWarning("No listener for answer");
				return;
			}
			foreach (var answerline in _layout.Lines)
			{
				answerline.OnClick.RemoveListener(OnAnswerClicked);
			} 
#if UNITY_ANDROID || UNITY_IOS
			if (_layout.Lines.Count > 1)
			{
				StartCoroutine(ShowSelectedAnswerRoutine(line.Node));
			}
			else
			{
				OnChoiceSelected.Invoke(line.Node);
			}
#else
			OnChoiceSelected.Invoke(line.Node);
#endif
		}
		
#if UNITY_ANDROID || UNITY_IOS
		private IEnumerator ShowSelectedAnswerRoutine(Node node)
		{
			HideAllOptionsButThis(node);
			yield return new WaitForSeconds(1f);
			OnChoiceSelected.Invoke(node);
		}
#endif
		public void Hide(bool waitForNextVid)
		{
			_layout.HideAll();

			if (waitForNextVid)
			{
				Globals.Videos.OnVideoStartPlaying.AddListener(ReallyHide);
			}
			else
			{
				ReallyHide();
			}
		}

		private void ReallyHide()
		{
			Globals.Videos?.OnVideoStartPlaying.RemoveListener(ReallyHide);
			TweenA.Add(Overlay, 0.4f, 0).Then(HideOverlay);
			//DoFade(_overlayRender, 0.4f, HideOverlay);
			//_overlayRender.DOFade(0, 0.4f).OnComplete(HideOverlay);
		}

		private void HideOverlay()
		{
			_overlayRender.color = new Color(1,1,1,0.8f);
			Overlay.SetActive(false);
		}

		public void HideAllOptionsButThis(Node node)
		{
			foreach (var answerline in _layout.Lines)
			{
				answerline.OnClick.RemoveListener(OnAnswerClicked);
			} 
			for (int i = 0; i < _layout.Lines.Count ; i++)
			{
				if( _layout.Lines[i].Node != node ) 
					_layout.Lines[i].SetActive(false);
			}
		}
	}
}
