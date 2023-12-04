using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
using Uween;
using static CharlesEngine.CELogger;

namespace CharlesEngine
{
    public class TextDialogManager : DialogManager
    {
        private bool _playingTextNode;
        public GameObject TextPrefab;
        private GameObject _text;
        private GameObject _sprite;
        private GameObject _overlay;
        protected override void PlayVideoNode(Node node)
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            Log("PlayTextNode " + node.Text, DIALOG);
            if (node.Text == null)
            {
                Debug.LogError("Text node must have text");
            }
            
            // SHOW TEXT
            var rootobj = GameObject.Find("SceneRoot").transform;
            if( _overlay == null )
            {
                _overlay = Instantiate(Globals.Choices.Overlay);
                _overlay.SetActive(true);
                var spriteRenderer = _overlay.GetComponent<SpriteRenderer>();
                spriteRenderer.sortingLayerName = "Video";
                spriteRenderer.color = new Color(1,1,1,0f);
                TweenA.Add(_overlay, 0.2f, 0.6f);
                _overlay.transform.SetParent(rootobj, false);
            }

            TextMeshPro text;
            if (TextPrefab == null)
            {
                _text = new GameObject("text");
                text = _text.AddComponent<TextMeshPro>();
                text.rectTransform.sizeDelta = new Vector2(1000, 300);
                text.enableWordWrapping = true;
                text.isOrthographic = true;
                text.font = Globals.Settings.DefaultFont;
            }
            else
            {
                _text = Instantiate(TextPrefab);
                text = _text.GetComponentInChildren<TextMeshPro>();
                if (text == null)
                {
                    throw new Exception("TextPrefab does not contain a TextMeshPro component!");
                }
            }

            text.text = node.Text;
            text.sortingLayerID = SortingLayer.NameToID("Video");
            text.sortingOrder = 2;
            _text.transform.SetParent(rootobj, false);
            _playingTextNode = true;

            if (node.Sprite != null)
            {
                _sprite = new GameObject("sprite");
                var sprd = _sprite.AddComponent<SpriteRenderer>();
                sprd.sortingLayerName = "Video";
                sprd.sortingOrder = 1;
                sprd.sprite = node.Sprite;
                _sprite.transform.SetParent(rootobj, false);
            }
        }

        public SpriteRenderer GetSpriteObject()
        {
            return _sprite.GetComponent<SpriteRenderer>();
        }

        protected override IForkNode GetForkStrategy(IForkNode type)
        {
            return type ?? ForkNodeClassicText.Instance;
        }

        public override bool HandleInput()
        {
            if (Globals.Input.GetKeyUp(InputAction.SkipVideo) && _playingTextNode)
            {
                if (_text != null)
                {
                    Destroy(_text);
                }       
                if (_sprite != null)
                {
                    Destroy(_sprite);
                }

                OnTextEnd();
                return true;
            }

            return false;
        }
        
        private void OnTextEnd()
        {
            Log("OnTextNodeEnd", DIALOG);
            if (_currentNode.Type == NodeType.Video)
            {
                if (_currentNode.Connections.Count == 1)
                {
                    var next = _currentTree.GetNode(_currentNode.Connections[0]);
                    PlayNode(next);
                }
                else if( _currentNode.Connections.Count > 1 )
                {
                    Debug.LogError("Broken video node, type:" + _currentNode.Type + " id:" + _currentNode.ID + "   conns:" + _currentNode.Connections.Count + " tree" + _currentTree);
                    Debug.LogError("Broken video node " + gameObject.scene.name + "  name" + _currentNode.ReadableID);
                }
                else
                {
                    Debug.LogError("Dialog end. Text node has no connections", gameObject);
                }
            }

        }

        protected override List<VideoClip> GetAllVideoClips()
        {
            return new List<VideoClip>();
        }
    }
}