using System;

namespace CharlesEngine
{
    public interface IEnumVariable
    {
        int GetValueAsInt();
        Enum GetValueAsEnum();
    }
}