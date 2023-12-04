using System.Collections;
using TMPro;
using UnityEngine;

namespace CharlesEngine
{
    public enum TextDir{ Center, Right }
    public class TextBackground : MonoBehaviour
    {
        public TextDir Direction = TextDir.Center; 
        public TextMeshPro Text;
        [Range(-50, 300)] public float Pad = 40;
        void Start()
        {
            StartCoroutine(RefreshCollider());
        }

        public IEnumerator RefreshCollider()
        {
            yield return null; // waiting for TMP to initialize
            yield return null;
            if (!gameObject.activeInHierarchy)
            {
                yield break;
            }
            Refresh();
        }
            
        [ContextMenu("RefreshWidth")]
        private void Refresh()
        {
            var spr = GetComponent<SpriteRenderer>();

            if (spr == null || Text == null)
            {
                Debug.LogError("TextBackground does not have text or collider!" + name, gameObject);
                return;
            }

            var spriteSize = spr.sprite.rect.size;
            var localSpriteSize = spriteSize / spr.sprite.pixelsPerUnit;
            var targetXScale = (Text.textBounds.size.x + Pad) / localSpriteSize.x;
            var origScale = transform.localScale.x;
            transform.localScale = new Vector3(targetXScale, transform.localScale.y, 1);

            if (Direction == TextDir.Right)
            {
                var shift = localSpriteSize * (targetXScale - origScale) / 2;
                transform.localPosition += new Vector3(shift.x, 0);
            }
        }
    }
}
