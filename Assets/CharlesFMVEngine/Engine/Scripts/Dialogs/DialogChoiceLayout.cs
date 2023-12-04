using System.Collections.Generic;
using UnityEngine;

namespace CharlesEngine
{

    public class DialogChoiceLayout : MonoBehaviour, IDialogChoiceLayout
    {
        public GameObject AnswerLinePrefab;
        private List<IAnswerLine> _answerLines = new List<IAnswerLine>();
        public List<IAnswerLine> Lines => _answerLines;

        public float StartY = -264f;
        public float LineHeight = 68f;
        [Tooltip("This will cause the answers to be shown next to each other, dividing the screen horizontally. (LineHeight will be ignored)")]
        public bool HorizontalLayout;
        
        public void ShowLines(int count)
        {
            if (count > _answerLines.Count)
            {
                AddLines(count);
            }
		
            for (int i = 0; i < _answerLines.Count ; i++)
            {
                _answerLines[i].SetActive(false);
            }
		
            float startY = StartY;
#if UNITY_ANDROID || UNITY_IOS
			LineHeight *= 1.3f;
#endif
            if (!HorizontalLayout)
            {
                for (int i = 0; i < count; i++)
                {
                    var line = _answerLines[i];
                    line.SetActive(true);
                    ((MonoBehaviour) line).transform.localPosition = new Vector3(0, LineHeight * (count - 1 - i) + startY, -2);
                }
            }
            else
            {
                var screenW = (float) Globals.Settings.Resolution.x;
                var pad = screenW / (count + 1);
                var startX = -screenW / 2 + pad;
                for (int i = 0; i < count; i++)
                {
                    var line = _answerLines[i];
                    line.SetActive(true);
                    ((MonoBehaviour) line).transform.localPosition = new Vector3(startX+i*pad, startY, -2);
                }
            }
        }

        private void AddLines(int max)
        {
            while (_answerLines.Count < max)
            {
                var newLine = Instantiate(AnswerLinePrefab, transform);
                newLine.name = "line" + _answerLines.Count;
                _answerLines.Add(newLine.GetComponent<IAnswerLine>());
            }
        }

        public void HideAll()
        {
            for (int i = 0; i < _answerLines.Count; i++)
            {
                var child = _answerLines[i];
                child.SetActive(false);
            }
        }
    }
}