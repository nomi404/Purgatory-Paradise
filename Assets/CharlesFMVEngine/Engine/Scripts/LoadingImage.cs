using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class LoadingImage : MonoBehaviour  // This is for an image that shows while the next scene is being loaded
{
    private bool inited;
    private SpriteRenderer _spriteRenderer;

    public static bool SkipNext;
    private const float Speed = 120f;
    private void Init()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.color = new Color(1,1,1,0);
        inited = true;
    }
    
    private void OnEnable()
    {
        if (!inited) Init();
#if UNITY_ANDROID || UNITY_IOS
        _spriteRenderer.color = new Color(1,1,1,0);
        if (SkipNext)
        {
            SkipNext = false;
            return;
        }
        StartCoroutine("Fade");
#endif
    }

    private IEnumerator Fade()
    {
        yield return new WaitForSeconds(1);
        for (int i = 0; i < 120; i++)
        {
            _spriteRenderer.color = new Color(1,1,1, (float) i/119);
            yield return null;
        }
        yield return new WaitForSeconds(1);
        for (int i = 0; i < 120; i++)
        {
            _spriteRenderer.color = new Color(1,1,1, 1f- (float) i/119);
            yield return null;
        }
    }
#if UNITY_ANDROID || UNITY_IOS
    private void Update()
    {
        transform.localRotation = Quaternion.Euler(0,0, -Time.time*Speed);
    }
#endif
    private void OnDisable()
    {
#if UNITY_ANDROID || UNITY_IOS
        StopCoroutine("Fade");
#endif
    }
}
