using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Video;

/**
 *	Add this class to a gameobject in the scene to start designing a dialog scene
 */

namespace CharlesEngine
{
	[ExecuteInEditMode]
	[AddComponentMenu("Dialog/Dialogues")]
	public class Dialogues : MonoBehaviour
	{
		public static Node CurrentlyPlayedNode;//only in play mode
	
		// Properties
		[HideInInspector] public DialogTree CurrentTree;
		[HideInInspector] public List<DialogTree> Trees = new List<DialogTree>();
		[HideInInspector] public bool TextMode;
		private string[] _tempResult;
		public string[] TabNames{
			get
			{
				if (Trees.Count == 0) return null;
				if (_tempResult == null || _tempResult.Length != Trees.Count)
				{
					_tempResult = new string[Trees.Count];
				}
				for(int i = 0; i < Trees.Count; i++)
				{
					_tempResult[i] = Trees[i].Name;
				}
				return _tempResult;
			}
		}
		//------------

		public DialogTree GetTreeByName(string name)
		{
			foreach (var tree in Trees)
			{
				if (tree.Name == name) return tree;
			}
			return null;
		}
	
		public DialogTree FindTreeForNode(Node node)
		{
			foreach (var tree in Trees)
			{
				if ( tree.Nodes.Contains(node) ) return tree;
			}
			return null;
		}
	}
}