using System;
#if CE_USE_I2Loc
using I2.Loc;
#endif
namespace CharlesEngine
{
    public class LangVariable : EnumVariable<LangEnum>
    {
        
        public override LangEnum RuntimeValue
        {
            get
            {
#if CE_USE_I2Loc
                LangEnum lang;

                if (Enum.TryParse(LocalizationManager.CurrentLanguageCode, out lang))
                {
                    return lang;
                }
#endif
                return LangEnum.en;//throw new Exception
            }
            set
            {
                PersistanceType = PersistenceType.InMemory;
                base.RuntimeValue = value;// write only memory
            }
        }
    }
}
