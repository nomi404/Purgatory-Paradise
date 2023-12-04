using UnityEngine;

public class GestureDetector
{
    private static GestureDetector _instance;
    
    private const float EscapeMenuTime = 0.7f;
    private const float SkipVidSizeInches = 2.5f;
    private bool _escapeMenuGesture;
    private float _twoFingerTimeStart;
    private bool _twoFingerInProgress;
    
    private bool _skipVidGesture;
    private bool _skipVidInProgress;
    private Vector2 _skipVidStartPos;
    private float _skipVidDist;

    public bool EscapeMenuGesture => _escapeMenuGesture;
    public bool SkipVidGesture => _skipVidGesture;
    

    public GestureDetector()
    {
        _instance = this;
    }

    public static GestureDetector Instance => _instance;
    
    public void Update()
    {
        if (Input.touchCount == 2)
        {
            if (_twoFingerInProgress)
            {
                if (Time.time - _twoFingerTimeStart > EscapeMenuTime)
                {
                    _escapeMenuGesture = true;
                    _twoFingerInProgress = false;
                }
            }
            else
            {
                _escapeMenuGesture = false;
            }
            if( Input.GetTouch(0).phase == TouchPhase.Began || Input.GetTouch(1).phase == TouchPhase.Began )
            {
                _twoFingerInProgress = true;
                _twoFingerTimeStart = Time.time;
            }
        }
        else if (Input.touchCount == 1)
        {
            var touch = Input.GetTouch(0);
            if (_skipVidInProgress)
            {
                _skipVidDist = Mathf.Abs(_skipVidStartPos.x - touch.position.x);
                if ( _skipVidDist > SkipVidSizeInches*Dpi())
                {
                    _skipVidGesture = true;
                    _skipVidInProgress = false;
                }
            }
            else if( touch.phase == TouchPhase.Began )
            {
                _skipVidStartPos = Input.GetTouch(0).position;
                _skipVidInProgress = true;
            }
            else
            {
                _skipVidGesture = false;
                _skipVidDist = 0;
            }
        }
        else
        {
            _escapeMenuGesture = false;
            _skipVidGesture = false;
            _skipVidInProgress = false;
            _skipVidDist = 0;
            _twoFingerTimeStart = Time.time;
        }
    }

    private float Dpi()
    {
        var sdpi = Screen.dpi;
        return sdpi > 100 ? sdpi : 100f;
    }

    public void DisableZoom()
    {
        _twoFingerInProgress = false;
    }

    public float GetSkipProgress()
    {
        if (_skipVidInProgress)
        {
            return Mathf.Clamp01(_skipVidDist / ( SkipVidSizeInches*Dpi()));
        }
        return 0;
    }
}
