using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
using static CharlesEngine.CELogger;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
#endif

namespace CharlesEngine
{
	[AddComponentMenu("Dialog/Dialog Manager")]
	public class DialogManager : SceneMainScript
	{
		public static bool IsActive;
		// PUBLIC
		public Dialogues Dialogues;

		public UnityEvent OnDialogStart;
		public UnityEvent OnDialogResume;

		// PRIVATE
		protected Node _currentNode;

		protected DialogTree _currentTree;
	
		private bool _isSkipableVideoPlaying;
		private bool _isResuming;
		private bool hasStarted;
		
		private readonly List<Node> _history = new List<Node>();
		private DialogObserver DialogObserver;
#if UNITY_ANDROID || UNITY_IOS
		private bool _didSetFade;
#endif
		private bool _wasPaused;
		protected override void StartScene()
		{
			// Does not play automatically, must be played be calling StartDialog
			if (_currentNode == null)
			{
				Debug.LogError("First node not set");
			}

			IsActive = true;
			
			if (_isResuming)
			{
				OnDialogResume?.Invoke();
			}
			else
			{
				OnDialogStart?.Invoke();
			}

			_isResuming = false;

			DialogObserver = GetComponent<DialogObserver>();
		}

		private void OnDisable()
		{
			IsActive = false;
		}

		public void StartDialog()
		{
			Globals.Videos.OnVideoAlmostEnd.AddListener(OnVideoAlmostEnd);
			Globals.Videos.OnVideoEnd.AddListener(OnVideoEnd);

			if (hasStarted)
			{
				Debug.LogError("Dialog already started");
				return;
			}
			hasStarted = true;
			
			Log("Dialog start "+gameObject.scene.name, DIALOG);

			var startWithNodeIdVariable = (StringVariable) Globals.Persistence.VariableManager.GetVariableByName("SwitchToDialogNode");
			if (startWithNodeIdVariable != null)
			{
				startWithNodeIdVariable.RuntimeValue = string.Empty;
			}

			// _currentTree and _currentNode must be set in Prepare method
			if (_currentNode == null)
			{
				Debug.LogError("No dialog found");
				return;
			}
			PlayNode(_currentNode);
			Globals.GameManager.RegisterInputHandler(this,1);
		}

		private void OnVideoEnd()
		{
			_isSkipableVideoPlaying = false;
			Log("OnVideoEnd",DIALOG);
			if (_currentNode.Type == NodeType.Video)
			{
				if (_currentNode.Connections.Count == 1)
				{
					var next = _currentTree.GetNode(_currentNode.Connections[0]);
					PlayNode(next);
				}
				else  if( _currentNode.Connections.Count > 1 )
				{
					Debug.LogError("Broken video node, type:"+ _currentNode.Type + " id:"+_currentNode.ID + "   conns:"+_currentNode.Connections.Count + " tree"+_currentTree);
					Debug.LogError("Broken video node " + gameObject.scene.name + "  name"+_currentNode.ReadableID );
				}
				else
				{
					Debug.LogError("Dialog End. Video node has no connections." + _currentNode.ReadableID, gameObject);
				}
			}
		}

		private void OnVideoAlmostEnd()
		{
			if (_currentNode != null && _currentNode.Connections.Count == 1)
			{
				var nextVideo = GetNextVideo(_currentNode);
				if (nextVideo != null)
				{
					Globals.Videos.PrepareVideo(nextVideo);
				}
			}
		}
	
		public override bool HandleInput()
		{
#if UNITY_ANDROID || UNITY_IOS
			if( _currentNode != null && _currentNode.Type == NodeType.Video && GestureDetector.Instance.GetSkipProgress() > 0.4f)
			{
				_didSetFade = true;
				var targetF = (GestureDetector.Instance.GetSkipProgress() - 0.4f) / 0.6f; 
				Globals.Fade.To(targetF);
			}else if (_didSetFade)
			{
				Globals.Fade.To(0);
				_didSetFade = false;
			}
#endif
			if ( Globals.Input.GetKeyUp(InputAction.SkipVideo) && _isSkipableVideoPlaying )
			{
				Globals.Videos.Skip();
				return true;
			}

			return false;
		}
		

		protected void PlayNode(Node node)
		{
			Log("PlayNode "+node?.ID,DIALOG);
			if (node == null) return;
			Dialogues.CurrentlyPlayedNode = node;
			_currentNode = node;
			if (node.VisitedVariable != null)
			{
				node.VisitedVariable.RuntimeValue = true;
			}
			
#if UNITY_EDITOR
			_history.Add(node);
#endif
			
			switch (node.Type)
			{
				case NodeType.Video:
					PlayVideoNode(node);
					break;
				case NodeType.Fork:
					ShowChoice(node);
					break;
				case NodeType.Connector:
					ConnectTo(node.ConnectorTree, node.ConnectorNode);
					break;
				case NodeType.Script:
					ExecuteScript(node);
					break;
				case NodeType.Switch:
					EvalSwitch(node);
					break;
				case NodeType.Start:
					var next = _currentTree.GetNode(node.Connections[0]);
					PlayNode(next);
					break;
				default:
					throw new Exception("Invalid Node Type:" + node.Type);
			}
			
			if( DialogObserver != null && node.Type != NodeType.Switch) //switch case is handled in EvalSwitch function
				DialogObserver.NodeVisited(node);
		}
		
#if UNITY_EDITOR
		[ContextMenu("Print History")]
		private void PrintHistory()
		{
			Debug.Log("-- History --");
			for (var i = 0; i < _history.Count; i++)
			{
				Debug.Log(i+": "+_history[i].ToPrettyString());	
			}
			Debug.Log("--  --");
		}
#endif
	
		private void EvalSwitch(Node node)
		{
			if (node.SwitchConditions.Count != node.Connections.Count)
			{
				Debug.LogError("Switch node is invalid");
				return;
			}

			for (var i = 0; i < node.SwitchConditions.Count; i++)
			{
				var cond = node.SwitchConditions[i];
				if (cond.IsEmpty || cond.Eval())
				{
					if( DialogObserver != null) //switch case is handled in EvalSwitch function
						DialogObserver.NodeVisited(node);
					
					PlayNode( _currentTree.GetNode(node.Connections[i])  );
					return;
				}
			}

			throw new Exception("Switch has no valid connection to go to or all conditions failed");
		}

		protected virtual void PlayVideoNode(Node node)
		{
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
			if( Dialogues.TextMode )
			{
				Debug.LogWarning("To play Dialogues in Text Mode, you should use the TextDialogManager component.");
				return;
			}
			
			Log("PlayVideoNode "+node.Video?.name,DIALOG);
			if (node.Video == null)
			{
				Debug.LogError("Video node must have video");
				return;
			}
			var player = Globals.Videos.PlayVideo(node.Video);
			_isSkipableVideoPlaying = true;
			for (var i = 0; i < node.TimedEvents.Count; i++)
			{
				var evt = node.TimedEvents[i];
				if (evt.Listener == null) continue;
				player.Scheduler.AddListener(scheduler => evt.Listener.Run(), evt.Time);
				if (evt.RepeatOnEnd)
				{
					player.Scheduler.AddListener(scheduler => evt.Listener.Run(), CEVideoPlayer.LastFrameTimeMark);
				}
			}
		}

		private void ExecuteScript(Node node)
		{
			if (node.Script == null)
			{
				throw new Exception("Script node " + node.ID + " in dialog "+ _currentTree.Name+"  has no script!");
			}
			
			node.Script.Run();

			if (node.Script is CERoutine routine)
			{
				StartCoroutine(ExecuteScriptRoutine(routine));
			}
			else
			{
				FinishScriptExecution();
			}
		}

		private IEnumerator ExecuteScriptRoutine(CERoutine routine)
		{
			yield return routine.RunRoutine();
			FinishScriptExecution();
		}

		private void FinishScriptExecution()
		{
			if (Globals.GameManager.IsLoading) // SwitchToScene called
			{
				return;
			}

			if (_currentNode.Connections.Count == 0)
			{
				Log("DIALOG END", DIALOG);
			}
			else if (_currentNode.Connections.Count == 1)
			{
				PlayNode(_currentTree.GetNode(_currentNode.Connections[0]));
			}
			else
			{
#if UNITY_EDITOR
				_currentTree.InErrorState = true;
				Selection.activeGameObject = Dialogues.gameObject;
				EditorApplication.ExecuteMenuItem("Tools/Charles Engine/Editors/Dialog Editor");                
#endif
				throw new Exception("Script node " + _currentNode.ID + " in dialog " + _currentTree.Name + " has multiple child nodes! It cannot have more than one.");
			}
		}

		private void ConnectTo(string treeName, string nodeName)
		{
			_currentTree = Dialogues.GetTreeByName(treeName);
			if (_currentNode == null) throw new Exception("Connector tree not found:" + treeName);
		
			var node = _currentTree.GetNodeFromName(nodeName);
			if (node == null) throw new Exception("Connector node not found:" + nodeName);
			if (node.Type == NodeType.Start)
			{
				node = _currentTree.GetNode(node.Connections[0]); // skip to following node, starting node is empty
			}
			PlayNode(node);
		}

		private void ShowChoice(Node forkNode)
		{
			Screen.sleepTimeout = SleepTimeout.SystemSetting;
			Globals.Subtitles.Hide();

			// Prepare options
			
			var options = new ForkOption[forkNode.Connections.Count];			
			var preparingNext = false;
			for (int i = 0; i < forkNode.Connections.Count; i++)
			{
				var node = _currentTree.GetNode(forkNode.Connections[i]);
				bool condition = true;
				if (node.Type == NodeType.Conditional)
				{
					condition = EvalCondition(node.Condition);
					node = _currentTree.GetNode(node.Connections[0]); //select the actual answer node
				}
				options[i] = new ForkOption(node, condition);

				//start preparing video to play after this answer is clicked
				if (node.Connections.Count == 0)
				{
					continue;
				}
				var nextVideo = GetNextVideo(node);
				if (nextVideo != null && !preparingNext && !Dialogues.TextMode)
				{
					StartCoroutine(PrepareWithDelay(nextVideo));
					preparingNext = true;
				}
			}
			
			// Sort options by answer order
			Array.Sort( options, (a, b) => a.Node.AnswerOrder.CompareTo(b.Node.AnswerOrder));		
			GetForkStrategy(forkNode.ForkType).Run(forkNode, options, _currentTree, OnChoiceSelected);
		}
		
		protected virtual IForkNode GetForkStrategy(IForkNode type)
		{
			return type ?? ForkNodeClassic.Instance;
		}

		private IEnumerator PrepareWithDelay(VideoClip vid)
		{
			// Wait a bit for the last video to enter the pool
			yield return new WaitForSeconds(0.8f);
			Globals.Videos.PrepareVideo(vid);
		}

		private bool EvalCondition(Condition cond)
		{
			if (cond == null) return true;
			return cond.Eval();
		}

		private void OnChoiceSelected(Node selectedNode)
		{
			Log("OnChoiceSelected "+selectedNode.AnswerOrder, DIALOG);
			StopAllCoroutines(); // should cancel any possible PrepareWithDelay coroutines running
			if (selectedNode.VisitedVariable != null)
			{
				selectedNode.VisitedVariable.RuntimeValue = true;
			}
		
#if UNITY_EDITOR
			_history.Add(selectedNode);
#endif
		
			CleanUpAfterChoice(selectedNode);
			
			if( DialogObserver != null )
				DialogObserver.NodeVisited(selectedNode);
			
			if (selectedNode.Connections.Count > 0)
			{
				var next = _currentTree.GetNode(selectedNode.Connections[0]);
				PlayNode(next);
			}
		}

		private void CleanUpAfterChoice(Node selectedNode)
		{
			Globals.Choices.OnChoiceSelected = null;
			
			Node next = null;
			if (selectedNode != null && selectedNode.Connections.Count > 0)
			{
				next = _currentTree.GetNode(selectedNode.Connections[0]);
				while (next != null && next.Type == NodeType.Script && next.Connections.Count > 0)
				{
					next = _currentTree.GetNode(next.Connections[0]);
				}
			}

			Globals.Choices.Hide(true);
			VideoClip keepvid = null;
			if (selectedNode != null && selectedNode.Connections.Count > 0) 
			{
				// special case when Fork -> Fork , if the second fork does not have a video specified, keep playing the first one
				var nnod = _currentTree.GetNode(selectedNode.Connections[0]);
				if (nnod?.Type == NodeType.Script && nnod.Connections.Count > 0)
				{
					nnod =  _currentTree.GetNode(nnod.Connections[0]);
				}
				if (nnod != null && nnod.Type == NodeType.Fork && nnod.Video == null)
				{
					keepvid = _currentNode.Video;
				}
			}

			//Release all other videos
			for (int i = 0; i < _currentNode.Connections.Count; i++)
			{
				var answerNode = _currentTree.GetNode(_currentNode.Connections[i]);
				if (answerNode == selectedNode) continue;
				var nextVideo = GetNextVideo(answerNode);
				if (nextVideo != null && next?.Video != nextVideo && nextVideo != keepvid) //two answers can go to the same video
				{
					Globals.Videos.ReleasePreparedVideo(nextVideo);
				}
			} //DEBUG 
			//----
		}

		private VideoClip GetNextVideo(Node node)
		{
			if (node.Connections.Count == 0) return null;

			var next = _currentTree.GetNode(node.Connections[0]);
			if (next.Video)
			{
				return next.Video;
			}

			if (next.Type == NodeType.Script)
			{
				return GetNextVideo(next);
			}

			//Attemp to find the connected video
			if (next.Type == NodeType.Connector)
			{
				foreach (var t in Dialogues.Trees)
				{
					if (t.Name == next.ConnectorTree)
					{
						var connectingNode = t.GetNodeFromName(next.ConnectorNode);
						if (connectingNode == null) return null;
						if (connectingNode.Type == NodeType.Start)
						{
							connectingNode = t.GetNode(connectingNode.Connections[0]);
						}
						return connectingNode.Video;
					}
				}
			}

			//Attemp to find the switch video
			if (next.Type == NodeType.Switch)
			{
				for (var i = 0; i < next.SwitchConditions.Count; i++)
				{
					var cond = next.SwitchConditions[i];
					if (cond.IsEmpty || cond.Eval())
					{
						return _currentTree.GetNode(next.Connections[i]).Video;
					}
				}
			}
			return null;
		}

		public override void Prepare()
		{		
			if (!Dialogues)
			{
				Debug.LogWarning("No Dialogues specified, looking for them:");
				Dialogues = FindObjectOfType<Dialogues>();
				if (Dialogues == null)
				{
					Debug.LogError("No dialogs found.");
					return;
				}
			}
			
			if (Dialogues.Trees == null || Dialogues.Trees.Count == 0)
			{
				Debug.LogWarning("No trees in dialog");
				return;
			}
			
			// Load and parse subtitles for all videos in dialog
			Globals.Videos.UnloadAllSubtitles();
			Globals.Videos.LoadSubtitlesFor(GetAllVideoClips());

			var conditions = GetComponents<DialogStartCondition>();
			var startWithNodeIdVariable = (StringVariable) Globals.Persistence.VariableManager.GetVariableByName("SwitchToDialogNode");
			if (startWithNodeIdVariable != null && !startWithNodeIdVariable.Empty())
			{
				var startWithNodeID = startWithNodeIdVariable.RuntimeValue.Trim();
				// Find the tree and node specified in StartWithNodeID
				var slashIndex = startWithNodeID.IndexOf("/", StringComparison.Ordinal);
				if (slashIndex < 0)
				{
					throw new Exception("StartWithNodeID must be format tree/nodeid , no slash found ! "+startWithNodeID);
				}
				var treeName = startWithNodeID.Substring(0, slashIndex);
				var nodeName = startWithNodeID.Substring(slashIndex + 1);
				_currentTree = Dialogues.GetTreeByName(treeName);
				_currentNode = _currentTree?.GetNodeFromName(nodeName);
				if (_currentNode == null)
				{
					throw new Exception("Invalid StartWithNodeID "+startWithNodeID);
				}

				_isResuming = true;
			}
			else if (conditions != null && conditions.Length > 0)
			{
				// Eval conditions
				foreach (var cond in conditions)
				{
					bool result = cond.Condition.Eval();				
					if (result)
					{
						_currentTree = Dialogues.GetTreeByName(cond.Tree);
						_currentNode = _currentTree.GetNodeFromName(cond.Node);
						if (_currentNode.Type == NodeType.Start)
						{
							_currentNode = _currentTree.GetNode(_currentNode.Connections[0]);
						}
						break;
					}
				}
			}
		
			if( _currentNode == null )
			{
				// No conditions or StartWithNode directive, so just start with first tree and its start node
				_currentTree = Dialogues.Trees[0];
				_currentNode = GetTreeFirstNode(_currentTree);
			}
		
			if (_currentNode != null && _currentNode.Video != null && !Dialogues.TextMode)
			{
				// Prepare first video
				Globals.Videos.PrepareVideo(_currentNode.Video);
			}
		}

		private Node GetTreeFirstNode(DialogTree dialogTree)
		{
			if (dialogTree == null)
			{
				Debug.LogWarning("Null tree");
				return null;
			}
		
			var start = dialogTree.GetStart();
			if (start == null)
			{
				Debug.LogWarning("Tree has no start " + dialogTree.Name);
				return null;
			}
			if (start.Connections.Count == 0)
			{
				Debug.LogWarning("First node has no connections");
				return null;
			}
			var first = dialogTree.GetNode(start.Connections[0]);
			return first;
		}
	
		protected virtual List<VideoClip> GetAllVideoClips()
		{
			var result = new List<VideoClip>();
			if (Dialogues == null || Dialogues.Trees == null) return result;
			for (var i = 0; i < Dialogues.Trees.Count; i++)
			{
				result.AddRange(Dialogues.Trees[i].GetAllVideoClips());
			}
			return result;
		}

		private void Reset()
		{
			if (Dialogues == null)
			{
				Dialogues = FindObjectOfType<Dialogues>();
			}
		}
		
		/*
		 * If we are currently in a fork, this will act as if the first answer was chosen
		 * (even though it might not even be visible, due to a condition)
		 */
		public void SkipFork()
		{
			if (_currentNode == null || _currentNode.Type != NodeType.Fork)
			{
				return;
			}

			var node = _currentTree.GetNode(_currentNode.Connections[0]);
			if (node.Type == NodeType.Conditional)
			{
				node = _currentTree.GetNode(node.Connections[0]); //select the actual answer node
			}
			OnChoiceSelected(node);
		}

		public void JumpTo(string tree, string node)
		{
			if (_currentNode != null && _currentNode.Type == NodeType.Fork)
			{
				StopAllCoroutines();
				Globals.Choices.OnChoiceSelected = null;
				Globals.Choices.Hide(true);
			}

			_currentTree = Dialogues.GetTreeByName(tree);
			_currentNode = _currentTree.GetNodeFromName(node);
			if (_currentNode == null)
			{
				Debug.LogError("JumpTo: Node not found:"+node +"  in tree "+tree);
			}
			PlayNode(_currentNode);
		}
		
#if UNITY_EDITOR
		[ContextMenu("Switch To Text Mode")]
		public void SwitchToTextMode()
		{
			var textDialogManager = gameObject.AddComponent<TextDialogManager>();
			textDialogManager.Dialogues = Dialogues;
			if (Dialogues != null)
			{
				Dialogues.TextMode = true;
			}
			var cescene = FindObjectOfType<CEScene>();
			var onshowevt = cescene.OnScreenShow;
			var targetIndex = -1;
			for (int i = 0; i < onshowevt.GetPersistentEventCount(); i++)
			{
				var targetref = onshowevt.GetPersistentTarget(i);
				if (targetref == this)
				{
					targetIndex = i;
					break;
				}
			}

			if (targetIndex >= 0)
			{
				UnityEventTools.RegisterVoidPersistentListener(onshowevt, targetIndex, textDialogManager.StartDialog);
			
			}
			Undo.RecordObject(gameObject, "edit condition");
			EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); //to actual mark scene as dirty
			DestroyImmediate(this); //this writes an error to the console, but it seems fine and the workaround is extremely hacky, unity pls fix
		}
#endif
		
		// Only works during video playback
		public void Pause()
		{
			if (_isSkipableVideoPlaying)
			{
				Globals.Videos.PauseAll();
				_wasPaused = true;
			}
		}

		public void Resume()
		{
			if (_wasPaused)
			{
				_wasPaused = false;
				Globals.Videos.Resume();
			}
		}
	}
}