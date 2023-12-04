using System.Collections.Generic;
using UnityEngine;

namespace CharlesEngine
{
	[AddComponentMenu("CE Scripts/Show Dialog Options")]
	public class ShowDialogOptions : CEScript
	{
		public Dialogues Choices;
		public string ID;
		public override void Run()
		{
			if (Choices == null)
			{
				Choices = GetComponent<Dialogues>();
				if (Choices == null)
				{
					Debug.LogError("Missing dialogues!");
				}
				return;
			}
		
			var currentTree = Choices.Trees[0];
			var forkNode = Choices.Trees[0].GetNodeFromName(ID);
			if (forkNode == null)
			{
				Debug.LogError("Node not found:"+ID);
				return;
			}
			List<Node> nodes = new List<Node>();
			for (int i = 0; i < forkNode.Connections.Count; i++)
			{
				var node = currentTree.GetNode(forkNode.Connections[i]);
				nodes.Add(node);
			}

			nodes.Sort((a, b) => a.AnswerOrder.CompareTo(b.AnswerOrder));
			Globals.Choices.ShowChoices(nodes.ToArray());
			Globals.Choices.OnChoiceSelected += OnChoiceSelected;
		}

		private void OnChoiceSelected(Node selectedNode)
		{
			Debug.Log("Choice selected");
			var currentTree = Choices.Trees[0];
			if (selectedNode.VisitedVariable != null)
			{
				selectedNode.VisitedVariable.RuntimeValue = true;
			}
			
			Globals.Choices.OnChoiceSelected -= OnChoiceSelected;
			
			if (selectedNode.Connections.Count < 1)
			{
				return;
			}
			var next = currentTree.GetNode(selectedNode.Connections[0]);
			if (next != null && next.Script != null)
			{
				next.Script.Run();
			}
			bool dialogEnding = next == null || next.Video == null && next.Connections.Count == 0;
			Debug.Log("dialog ending:"+dialogEnding);
			Globals.Choices.Hide(!dialogEnding);
		}
	}
}
