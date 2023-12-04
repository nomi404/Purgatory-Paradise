using System;
using System.Collections.Generic;
using UnityEngine.Video;

namespace CharlesEngine
{
		[Serializable]
		public class DialogTree
		{
			public int NodeIdAutoIncrement;
		
			public string Name;
			public int FirstNode = -562;
			public List<Node> Nodes = new List<Node>();

			public string LocalizationPrefix;

            [NonSerialized]
            public bool InErrorState = false;
		
			public Node GetNode(int ID)
			{
				for (int i = 0; i < Nodes.Count; i++)
				{
					if (Nodes[i].ID == ID)
						return Nodes[i];
				}
				return null;
			}

			public Node GetStart()
			{
				for (int i = 0; i < Nodes.Count; i++)
				{
					if ( Nodes[i].Type == NodeType.Start )
						return Nodes[i];
				}
				return null;
			}

			public Node GetNodeFromName(string name)
			{
				if (string.IsNullOrWhiteSpace(name)) return null;
				for (int i = 0; i < Nodes.Count; i++)
				{
					if ( Nodes[i].ReadableID == name )
						return Nodes[i];
				}
				return null;
			}

			public List<VideoClip> GetAllVideoClips()
			{
				List<VideoClip> result = new List<VideoClip>();
				SearchTree(GetStart(), result, new List<int>());
				return result;
			}

			private void SearchTree(Node node, List<VideoClip> result, List<int> visited)
			{
				if ( node == null ) return;
			
				visited.Add(node.ID);
			
				if (node.Video != null && result.Contains(node.Video) == false)
				{
					if (node.Type != NodeType.Fork) // Exclude forks, where there's no subtitles
					{
						result.Add(node.Video);
					}
				}
			
				foreach (var con in node.Connections)
				{
					if ( visited.Contains(con) ) continue;
					SearchTree(GetNode(con), result, visited);
				}
			}

			public override string ToString()
			{
				return "[Tree "+Name+"]";
			}
		}
}