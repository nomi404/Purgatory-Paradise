using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static CharlesEngine.CELogger;

namespace CharlesEngine
{
    public class ForkNodeClassicText : IForkNode
    {
        public static ForkNodeClassicText Instance = new ForkNodeClassicText();
        private GameObject _text;
        private GameObject _sprite;
        public virtual void Run(Node forkNode, ForkOption[] options, DialogTree dialogDialogTree, OnChoiceHandler choiceCallback)
        {

            _text = new GameObject("text");
            var text = _text.AddComponent<TextMeshPro>();
            text.text = forkNode.Text;
            text.isOrthographic = true;
            text.rectTransform.sizeDelta = new Vector2(1000,300);
            text.enableWordWrapping = true;
            text.sortingLayerID = SortingLayer.NameToID("Video");
            text.sortingOrder = 2;
            text.font = Globals.Settings.DefaultFont;
            text.color = Color.green;
            var rootobj = GameObject.Find("SceneRoot").transform;
            _text.transform.SetParent(rootobj, false);

            if (forkNode.Sprite != null)
            {
                _sprite = new GameObject("sprite");
                var sprd = _sprite.AddComponent<SpriteRenderer>();
                sprd.sortingLayerName = "Video";
                sprd.sortingOrder = 1;
                sprd.sprite = forkNode.Sprite;
                _sprite.transform.SetParent(rootobj, false);
            }

            var optionsArray = NodesToArray(options);
            if (optionsArray.Length == 0)
            {
                Log("Error: No dialog choices shown in forknode:"+forkNode.ReadableID, DIALOG);
            }
            Globals.Choices.ShowChoices( optionsArray );
            Globals.Choices.Overlay.SetActive(false);
            Globals.Choices.OnChoiceSelected = a =>
            {
                if (_text != null)
                {
                    Object.Destroy(_text);
                }
                if (_sprite != null)
                {
                    Object.Destroy(_sprite);
                }
                choiceCallback.Invoke(a);
            };
        }

        private Node[] NodesToArray(ForkOption[] options)
        {
            var result = new List<Node>();
            for (int i = 0; i < options.Length; i++)
            {
                if (!options[i].Condition)
                {
                    continue;
                }
                result.Add(options[i].Node);
            }
            return result.ToArray();
        }
    }
}