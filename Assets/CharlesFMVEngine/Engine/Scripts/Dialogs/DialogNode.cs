using System;
using System.Collections.Generic;
#if CE_USE_I2Loc
using I2.Loc;
#endif
using UnityEngine;
using UnityEngine.Video;

namespace CharlesEngine
{
// ReSharper disable FieldCanBeMadeReadOnly.Global

	public enum NodeType
	{
		Video,
		Fork,
		ChoiceAnswer,
		Connector,
		Conditional,
		Script,
		Switch,
		Start
	}

	[Serializable]
	public class VideoTimedEvent
	{
		public CEScript Listener;
		public float Time;
		public bool RepeatOnEnd;
	}

	[Serializable]
	public class Node
	{
		public int ID;
		public string ReadableID;
		public Rect Rect;
		public NodeType Type;
		public List<int> Connections = new List<int>(); //do not make any fields readonly, it breaks serialization

		// Fork
		[NonSerialized]
		public IForkNode ForkType; // set from CustomForkStrategy scripts during runtime
		//
		
		// Answer
		public string Text;
		public int AnswerOrder;
		//
		
		//connector properties
		public string ConnectorTree;
		public int ConnectorTreeIndex;
		public string ConnectorNode;
		//
	
		// Condition
		public Condition Condition;
		//
	
		// Script
		public CEScript Script;
		//
	
		public BoolVariable VisitedVariable;
	
		// Video
		public VideoClip Video;
		public List<VideoTimedEvent> TimedEvents = new List<VideoTimedEvent>();
		//

		// Switch
		public List<Condition> SwitchConditions = new List<Condition>();
		//
		
		//For Text Mode
		public Sprite Sprite;
		public bool Maximized;
		//
	
		public Node(int id, Rect newRect, NodeType type = NodeType.Video)
		{
			//Debug.Log("Node created id "+id);
			ID = id;
			Rect = newRect;
			Text = "";
			Type = type;
		}
	
		public override string ToString()
		{
			return "[Node "+ID+"]";
		}
	
		public string ToPrettyString()
		{
			var typestring = Enum.GetName(typeof(NodeType), Type);
			switch (Type)
			{
				case NodeType.Video: return typestring + "- " + (Video != null ? Video.name : "empty");

				case NodeType.ChoiceAnswer:
#if CE_USE_I2Loc
					LocalizedString localized = Text;
#else
				string localized = Text;
#endif
					return typestring + " " + localized;
				case NodeType.Script: return typestring + "- " + (Script != null ? Script.gameObject.name : "empty");
				case NodeType.Connector: return typestring + "- " + ConnectorTree + "/" + ConnectorNode;
				default: return typestring;
			}
		}
	}
}