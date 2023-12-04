using System;

namespace CharlesEngine
{

    public class EnumVariable<T> : Variable<T>, IEnumVariable where T : struct, IComparable, IConvertible
    {
        public int GetValueAsInt()
        {
            Enum e = RuntimeValue as Enum;
            if (e == null) return 0;
            return Convert.ToInt32(e);
        }

        public Enum GetValueAsEnum()
        {
            return RuntimeValue as Enum;
        }
    }
}