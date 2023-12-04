using System;
using System.Collections.Generic;
using UnityEngine;

namespace CharlesEngine
{
	[Serializable]
	public class MappedItem
	{
		public KeyCode Code;
		public InputAction Action;
	}

	[CreateAssetMenu(fileName = "inputMapping",menuName ="Input/InputMapping")]
	public class InputMapping : ScriptableObject{
	
		public List<MappedItem> Mappings = new List<MappedItem>();
	
		private Dictionary<InputAction, KeyCode> _dictionary = new Dictionary<InputAction, KeyCode>();
	
		private void OnEnable()
		{
			foreach (var item in Mappings)
			{
				_dictionary[item.Action] = item.Code;
			}
		}

		public KeyCode GetCode(InputAction action)
		{
			return _dictionary[action];
		}
	
		public KeyCode this[InputAction action]
		{
			get { return GetCode(action); }
		}
	}
}