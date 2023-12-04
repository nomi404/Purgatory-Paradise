using System.Collections.Generic;

namespace CharlesEngine
{
    public interface IDialogChoiceLayout
    {
        List<IAnswerLine> Lines { get; }
        void ShowLines(int count);
        void HideAll();
    }
}