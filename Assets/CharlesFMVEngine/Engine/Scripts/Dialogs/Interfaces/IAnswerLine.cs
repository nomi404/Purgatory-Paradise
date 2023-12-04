using System;
using UnityEngine.Events;

namespace CharlesEngine
{
    [Serializable]
    public class AnswerLineEvent : UnityEvent<IAnswerLine>
    {
    }

    public interface IAnswerLine
    {
        void SetActive(bool value);
        void SetText(string text);
        AnswerLineEvent OnClick { get; }
        bool IsActive();
        Node Node { get; set; }
    }
}