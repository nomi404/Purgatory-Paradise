using System;
using UnityEngine;

namespace CharlesEngine
{
	public enum PersistenceType
	{
		// Saved for a profile
		Profile,

		// Only temporary value, not saved
		InMemory,

		// Persisted for all profiles
		Game
	}

	[Serializable]
	public abstract class VariableData
	{
		public virtual VariableData Clone()
		{
			return this;
		}
	}

	public abstract class BaseVariable : ScriptableObject
	{
		[HideInInspector]
		public string Guid;
		public abstract void ResetValue();
	
		//Persistence
		[Tooltip("Profile: saved for each profile\nIn Memory: Only temporary value, reset on every scene change\nGame: saved for all profiles")]
		public PersistenceType PersistanceType;
		public abstract VariableData GetData();
		public abstract void LoadFromData(VariableData data);
	}
}