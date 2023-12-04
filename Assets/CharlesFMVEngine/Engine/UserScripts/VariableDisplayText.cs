using TMPro;
using UnityEngine;
#if CE_USE_I2Loc
using I2.Loc;
#endif

namespace CharlesEngine
{
	[RequireComponent(typeof(TextMeshPro))]
	[AddComponentMenu("CE Toolbox/Variable Display Text")]
	public class VariableDisplayText : MonoBehaviour
	{
		public IntVariable Variable;
		public string LocalizationKey;

		private TextMeshPro _text;
#if CE_USE_I2Loc
		public bool DisableLocalization;
#endif
		private void Start()
		{
			_text = GetComponent<TextMeshPro>();
		
			UpdateText(Variable.RuntimeValue);

			Variable.OnVariableChange += UpdateText;
		}

		private void UpdateText(int variableRuntimeValue)
		{
			string lst = "";
#if CE_USE_I2Loc
			if (DisableLocalization)
			{
				lst = LocalizationKey;
			}
			else
			{

			LocalizedString ls = LocalizationKey;
			lst = ls.ToString();
			}
#else
			lst = LocalizationKey;
#endif
			

			string s = lst.Replace("#", variableRuntimeValue.ToString());
			_text.text = s;
		}
	}
}
