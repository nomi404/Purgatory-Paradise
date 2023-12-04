#if UNITY_EDITOR

#if CE_USE_I2Loc
using I2.Loc;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace CharlesEngine
{
	public class DialogueEditorWindow : EditorWindow
	{
	
		private class ConnectionRemoveData
		{
			public int Connection;
			public Node Node;
		}
	
		private class NodeCreateData
		{
			public NodeType Type;
			public Vector2 MousePos;
			public Node PrevNode;
		}
	
		//Saved Textures
		private static Texture2D _backgroundTexture;
		private static Texture2D _panelTexture;
		private static Texture2D _timerTexture;
		private readonly Color _smallLinesColor = new Color(0.30f, 0.30f, 0.30f);

		//Retrieved from the dialogue component on the current selected gameObject
		private Dialogues _currentDialogue;

		//Ids for sending to the nodes
		private Dictionary<int, object[]> _ids = new Dictionary<int, object[]>();

		private bool _connecting;
		private Node _connectingCurrent;

		private float _repaintTimer;

		private bool _zoomIn;

		private const float ZoomScale = 0.5f;

		//Used for scrolling in the main view
		private Vector2 _scrollPosition = Vector2.zero;

		private Vector2 _previousPosition = Vector2.zero;
		private int _currentTab;

		private Rect _newTree = new Rect(100, 100, 400, 200);
		private string _newTreeName = "";
		private string _newTreeInfo = "";

		//Sizes
		private Vector2 _nodeSize = new Vector2(150, 125);
		private Vector2 _workSize = new Vector2(6000, 3000);
	
		private Color[] _connectionColors = new Color[4];
		
		private static bool IsDebug = false;
		
		// Top Left Buttons
		private GUIContent _modeLabel;
		private Rect _modeRect = new Rect(4, 30, 90, 25);

		private GUIContent _zoomLabel;
		private Rect _zoomRect = new Rect(4, 60, 30, 25);

		private GUIContent _centerLabel = new GUIContent("⊙", "Select current node");
		private Rect _centerRect = new Rect(4, 90, 30, 25);

		private GUIContent _alignLabel = new GUIContent("□", "Auto-align");
		private Rect _alignRect = new Rect(4, 120, 30, 25);
		
	
		[MenuItem("Tools/Charles Engine/Dialog Editor", false, 300)]
		static void Init()
		{
			DialogueEditorWindow win = (DialogueEditorWindow) GetWindow(typeof(DialogueEditorWindow), false, "Dialog Editor");
			win.MakeTextures();
	//		BuildStyles();
		}

		#region Default Functions

		void Update()
		{
			//If making a connection, constantly repaint so the line draws to the mouse pointer
			if (_connecting)
				Repaint();
			//Calls repaint more frequently for updating when a Dialogues component is added, etc.
			_repaintTimer += 0.01f;
			if (_repaintTimer > 1)
			{
				_repaintTimer = 0;
				Repaint();
			}
		}

		#endregion

		//=========    ON GUI ===============//

		private void OnGUI()
		{
			_connectionColors[0] = Color.red;
			_connectionColors[1] = Color.blue;
			_connectionColors[2] = Color.magenta;
			_connectionColors[3] = Color.black;
		
			FindCurrentDialogue();
		
#if CE_USE_I2Loc
			if (LocalizationManager.Sources.Count == 0)
				LocalizationManager.UpdateSources();
#endif

			if (!_currentDialogue)
			{
				GUILayout.Label("Select a Dialogues component to edit");
				if( GUILayout.Button("Find And Select",GUILayout.Width(110)) )
				{
					var obj = SceneManager.GetActiveScene().GetRootGameObjects();
					foreach (var go in obj)
					{
						var dial = go.GetComponentInChildren<Dialogues>();
						if (dial != null)
						{
							Selection.activeGameObject = dial.gameObject;
							break;
						}
					}
				}
				return;
			}

			if (_currentDialogue.Trees.Count == 0)
			{
				_newTreeName = "";
				_newTreeInfo = "";
				var newTree = new DialogTree();
				_currentDialogue.Trees.Add(newTree);
				_currentDialogue.CurrentTree = newTree;
			}

			CheckConnections();

			//If texture wasn't setup, set it up
			if (_backgroundTexture == null)
			{
				MakeTextures();
			//	BuildStyles();
			}

			CheckTopLeftButtons();
			
			if (_currentDialogue.Trees.Count == 0)
			{
				GUIStyle style = GUI.skin.GetStyle("Label");
				style.alignment = TextAnchor.MiddleCenter;
				GUILayout.Label("\n\nThere are no existing Dialogue Trees\nClick the 'New' button to add a tab", style);
			}
			else
			{
				if (_currentTab < _currentDialogue.Trees.Count)
				{
					_currentDialogue.CurrentTree = _currentDialogue.Trees[_currentTab];
				}
				else
				{
					_currentDialogue.CurrentTree = _currentDialogue.Trees[0];
				}

			
				if (Event.current.control && Event.current.isKey)
				{
					if (Event.current.keyCode == KeyCode.LeftArrow)
					{
						_scrollPosition += Vector2.left*350;
					}
					if (Event.current.keyCode == KeyCode.RightArrow)
					{
						_scrollPosition += Vector2.right*350;
					}
				}
			
			
				//Creates work area
				_scrollPosition = GUI.BeginScrollView(new Rect(0, 0, position.width, position.height), _scrollPosition,
					new Rect(Vector2.zero, _workSize), GUIStyle.none, GUIStyle.none);
				//Makes the background a dark gray
				GUI.DrawTexture(new Rect(0, 0, _workSize.x, _workSize.y), _backgroundTexture, ScaleMode.StretchToFill);
				Handles.BeginGUI();
				
				//Scroll with the mouse drag
				if (Event.current.type == EventType.MouseDrag)
				{
					var my = Event.current.mousePosition.y;
					var h = position.height;
					var scrollY = _scrollPosition.y;
					if (my < scrollY+5)
					{
						_scrollPosition -= Vector2.up*20;
					}
					if (my > scrollY + h)
					{
						_scrollPosition -= Vector2.down*20;
					}
				}
				
				//Draws grid
				for (int i = 0; i < _workSize.x / 100; i++)
				{
					EditorGUI.DrawRect(new Rect(i * 100, 0, 2, _workSize.y), _smallLinesColor);
				}
				for (int i = 0; i < _workSize.y / 100; i++)
				{
					EditorGUI.DrawRect(new Rect(0, i * 100, _workSize.x, 2), _smallLinesColor);
				}

				DrawConnections();

				Handles.EndGUI();

				BeginWindows();
				_ids.Clear();
				
				if (_currentDialogue?.CurrentTree == null) return;

				if (string.IsNullOrEmpty(_currentDialogue.CurrentTree.Name))
				{
					_newTree = GUI.Window(99999, _newTree, AddNewTree, "Add New Dialogue Tree");
					_scrollPosition = _newTree.center;
					_scrollPosition -= new Vector2(position.width, position.height) / 2f;
				}
				else
				{
					BuildNodes();
				}

				EndWindows();

				GUI.EndScrollView();
				if (_currentDialogue.Trees.Count > 0)
				{
					GUILayout.BeginArea(new Rect(250, 0, position.width-250, 25));
					var wastab = _currentTab;
					_currentTab = GUILayout.Toolbar(_currentTab, _currentDialogue.TabNames, GUILayout.Height(25));
					if (wastab != _currentTab)
					{
						GUI.FocusControl(null); // unfocus, in case user was in middle of text editing 
					}
					GUILayout.EndArea();
				}

				/*if (GUI.Button(new Rect(2, 130, 30, 25), "D"))
				{
					IsDebug = !IsDebug;
				}*/

				if (new Rect(0, 0, position.width, position.height).Contains(Event.current.mousePosition))
				{
					if (Event.current.button == 1)
					{
						GenericMenu Menu = new GenericMenu();

						BuildMenus(Menu);
						if( Menu.GetItemCount() > 0 ) Menu.ShowAsContext();
					}
				}

				if (new Rect(0, 0, position.width, position.height).Contains(Event.current.mousePosition))
				{
					if (Event.current.button == 2 && Event.current.type == EventType.MouseDrag)
					{
						Vector2 currentPos = Event.current.mousePosition;

						if (Vector2.Distance(currentPos, _previousPosition) < 50)
						{
							float x = _previousPosition.x - currentPos.x;
							float y = _previousPosition.y - currentPos.y;

							_scrollPosition.x += x;
							_scrollPosition.y += y;
							Event.current.Use();
						}
						_previousPosition = currentPos;
					}
				}

				if (IsDebug)
				{
					string result = "";
					for (int j = 0; j < _currentDialogue.CurrentTree.Nodes.Count; j++)
					{
						Node nodeList = _currentDialogue.CurrentTree.Nodes[j];
						result += nodeList.ID + "[" + nodeList.Type + "]";
						if (nodeList.Connections.Count > 0)
						{
							result += " -> " + string.Join(", ", nodeList.Connections) + "\n";
						}
						else
						{
							result += "\n";
						}
					}

					GUI.TextArea(new Rect(2, 160, 250, 350), result);
				}
			}

			
			// TREE controls
			GUILayout.BeginArea(new Rect(0, 0, position.width, 50));
			GUILayout.BeginHorizontal();
			var previousH = EditorStyles.toolbarButton.fixedHeight;
			EditorStyles.toolbarButton.fixedHeight = 25;
			if (GUILayout.Button("New Dialogue Tree", EditorStyles.toolbarButton, GUILayout.Width(150)))
			{
				_newTree = new Rect(50 + _scrollPosition.x, 50 + _scrollPosition.y, 400, 150);
				_newTreeName = "";
				_newTreeInfo = "";
				var newTree = new DialogTree();
				_currentDialogue.Trees.Add(newTree);
				_currentDialogue.CurrentTree = newTree;
				_currentTab = _currentDialogue.Trees.Count - 1;
			}

			if (GUILayout.Button("Remove Tree", EditorStyles.toolbarButton, GUILayout.Width(100)))
			{
				Undo.RecordObject(_currentDialogue, "Remove Tree");
				if (_currentTab <= _currentDialogue.Trees.Count - 1)
				{
					_currentDialogue.Trees.RemoveAt(_currentTab);
				}
				_currentDialogue.CurrentTree = _currentDialogue.Trees.Count > 0 ? _currentDialogue.Trees[0] : null;
				_currentTab = 0;
			}
			EditorStyles.toolbarButton.fixedHeight = previousH;
			GUILayout.EndHorizontal();
			GUILayout.EndArea();

			DrawTopLeftButtons();
		}

		public void CheckTopLeftButtons()
		{
			// [Video mode, Text mode] button
			_modeLabel = new GUIContent("Video Mode", "Dialog is using videos.");
			if (_currentDialogue.TextMode)
			{
				_modeLabel = new GUIContent("Text Mode", "Dialog is using texts. For prototyping purposes.");
			}

			if (GUI.Button(_modeRect, _modeLabel))
			{
				_currentDialogue.TextMode = !_currentDialogue.TextMode;
			}

			// ZOOM button
			_zoomLabel = new GUIContent("Z", "Zoom out");
			if (_zoomIn)
			{
				_zoomLabel = new GUIContent("Z̶", "Reset zoom");
			}

			if (GUI.Button(_zoomRect, _zoomLabel))
			{
				_zoomIn = !_zoomIn;
			}

			// CENTER button
			if (GUI.Button(_centerRect, _centerLabel))
			{
				_scrollPosition = new Vector2(0, 0);
				if (Application.isPlaying && Dialogues.CurrentlyPlayedNode != null)
				{
					_scrollPosition = Dialogues.CurrentlyPlayedNode.Rect.center;
					if (_zoomIn)
					{
						_scrollPosition *= ZoomScale;
					}

					_scrollPosition -= new Vector2(position.width, position.height) / 2f;
				}
			}

			// ALIGN button
			if (GUI.Button(_alignRect, _alignLabel))
			{
				var nods = _currentDialogue.CurrentTree.Nodes;
				foreach (var node in nods)
				{
					node.Rect.x = Mathf.Round(node.Rect.x / 20f) * 20f;
					node.Rect.y = Mathf.Round(node.Rect.y / 20f) * 20f;
				}
			}
		}

		public void DrawTopLeftButtons()
		{
			GUI.Button(_modeRect, _modeLabel);
			GUI.Button(_zoomRect, _zoomLabel);
			GUI.Button(_centerRect, _centerLabel);
			GUI.Button(_alignRect, _alignLabel);
		}



		Node CreateNewNode(Vector2 Position, NodeType type = NodeType.Video)
		{
			Node newNode = new Node(_currentDialogue.CurrentTree.NodeIdAutoIncrement, new Rect(Position, _nodeSize),
				type);
			_currentDialogue.CurrentTree.NodeIdAutoIncrement++;
			return newNode;
		}

		bool CheckDialogueExists()
		{
			if (_currentDialogue?.Trees == null)
			{
				return false;
			}
			var cur = _currentDialogue.CurrentTree;
			if (cur == null ||
			    cur.FirstNode >= cur.Nodes.Count ||
			    cur.FirstNode < 0 ||
			    cur.Nodes[cur.FirstNode] == null) return false;

			if (cur.FirstNode == -562 ||
			    cur.Nodes[cur.FirstNode].Connections == null)
			{
				return false;
			}

			return true;
		}

		Node FindPreviousNode(Node winToFind)
		{
			if (_currentDialogue.CurrentTree.FirstNode == winToFind.ID) return null;

			var currentNodes = _currentDialogue.CurrentTree.Nodes;

			for (int i = 0; i < currentNodes.Count; i++)
			{
				var connections = currentNodes[i].Connections;
				for (int j = 0; j < connections.Count; j++)
				{
					Node curr = _currentDialogue.CurrentTree.GetNode(connections[j]);
					if (curr == winToFind)
						return _currentDialogue.CurrentTree.Nodes[i];
				}
			}
			return null;
		}

		List<Node> FindPreviousNodes(Node winToFind)
		{
			if (_currentDialogue.CurrentTree.FirstNode == winToFind.ID) return null;
			var result = new List<Node>();
			var currentNodes = _currentDialogue.CurrentTree.Nodes;
			for (int i = 0; i < currentNodes.Count; i++)
			{
				var connections = currentNodes[i].Connections;
				for (int j = 0; j < connections.Count; j++)
				{
					Node curr = _currentDialogue.CurrentTree.GetNode(connections[j]);
					if (curr == winToFind)
						result.Add(_currentDialogue.CurrentTree.Nodes[i]);
				}
			}
			return result;
		}

		bool Find(Node win, Node winToFind)
		{
			for (int i = 0; i < _currentDialogue.CurrentTree.Nodes.Count; i++)
			{
				if (_currentDialogue.CurrentTree.Nodes[i] == winToFind)
					return true;
			}
			return false;
		}

		public void MakeTextures()
		{
			_backgroundTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
			_backgroundTexture.SetPixel(0, 0, new Color(0.35f, 0.35f, 0.35f));
			_backgroundTexture.Apply();

			_panelTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
			_panelTexture.SetPixel(0, 0, new Color(0.65f, 0.65f, 0.65f));
			_panelTexture.Apply();
			

			var p = AssetDatabase.GUIDToAssetPath("894e43cc57c641143a1f8c75a33b41b5");
			_timerTexture = (Texture2D) AssetDatabase.LoadAssetAtPath(p, typeof(Texture2D));
		}

		private Vector2 RotateVector2(Vector2 aPoint, Vector2 aPivot, float aDegree)
		{
			Vector3 pivot = aPivot;
			return pivot + Quaternion.Euler(0, 0, aDegree) * (aPoint - aPivot);
		}

		private float AngleBetweenVector2(Vector2 a, Vector2 b)
		{
			Vector2 difference = b - a;
			float sign = (b.y < a.y) ? -1f : 1f;
			return Vector2.Angle(Vector2.right, difference) * sign;
		}

		bool FindCurrentDialogue()
		{
			if (Selection.activeTransform)
			{
				_currentDialogue = Selection.activeTransform.GetComponent<Dialogues>();
				if (_currentDialogue == null)
				{
					//
				}
				return true;
			}
			else
			{
				_currentDialogue = null;
				GUILayout.Label("No object is currently selected");
				return false;
			}
		}

		void AddNodeWrapper(object data)
		{
			AddNode(data);
		}

		Node AddNode(object dataParam)
		{
			Undo.RecordObject(_currentDialogue, "Dialogue");

			NodeCreateData data = (NodeCreateData) dataParam;

			//Creates the new window
			Node newlyCreatedNode = CreateNewNode(data.MousePos, data.Type);

			//Checks if this is the first node
			if (data.PrevNode == null)
			{			
				_currentDialogue.CurrentTree.FirstNode = newlyCreatedNode.ID;
			}
			else
			{
				data.PrevNode.Connections.Add(newlyCreatedNode.ID);
			}
			_currentDialogue.CurrentTree.Nodes.Add(newlyCreatedNode);
			return newlyCreatedNode;
		}

		private void AddConditionNodeBefore(Node node)
		{
			Undo.RecordObject(_currentDialogue, "Dialogue");

			Node prevNode = FindPreviousNode(node);
			if (prevNode.Type == NodeType.Conditional)
			{
				// Remove
				RemoveNode(prevNode);
			}
			else
			{
				// Add
				var data = new NodeCreateData
				{
					MousePos = node.Rect.position - new Vector2(160, 0),
					Type = NodeType.Conditional,
					PrevNode = prevNode
				};
				var newlyAdded = AddNode(data);
				newlyAdded.Rect = new Rect(newlyAdded.Rect.x, newlyAdded.Rect.y, 120f, 60f);
				newlyAdded.Condition = newlyAdded.Condition ?? new Condition();
				newlyAdded.Connections.Add(node.ID);
				prevNode.Connections.Remove(node.ID);
			}
		}


		void AddNodeAfter(object win)
		{
			Undo.RecordObject(_currentDialogue, "Dialogue");

			Node curr = (Node) win;

			Node newlyCreatedNode = CreateNewNode(curr.Rect.position + new Vector2(160, 0));
			if (curr.Type == NodeType.Fork || curr.Type == NodeType.Switch)
			{
				curr.Connections.Add(newlyCreatedNode.ID);
			}
			else
			{
				for (int i = 0; i < curr.Connections.Count; i++)
				{
					newlyCreatedNode.Connections.Add(curr.Connections[i]);
				}
				curr.Connections.Clear();
				curr.Connections.Add(newlyCreatedNode.ID);
			}
			_currentDialogue.CurrentTree.Nodes.Add(newlyCreatedNode);
		}

		void RemoveNode(object win)
		{
			Undo.RecordObject(_currentDialogue, "Remove Node");
			Node curr = (Node) win;
			_ids.Clear();

			if (curr.ID == _currentDialogue.CurrentTree.FirstNode)
			{
				if (curr.Connections.Count == 0)
					_currentDialogue.CurrentTree.FirstNode = -562;
				return;
			}

			var prevNodes = FindPreviousNodes(curr);
			if (prevNodes.Count > 0)
			{
				foreach (var p in prevNodes)
				{
					p.Connections.Remove(curr.ID);
				}	
				if (curr.Connections.Count != 0)
				{
					//We go through it's connections, and add them to the previous window
					for (int i = 0; i < curr.Connections.Count; i++)
					{
						foreach (var p in prevNodes)
						{
							var connectionId = curr.Connections[i];
							if (p.ID == connectionId) continue;
							if (p.Connections.Count <= 0 || p.Type == NodeType.Switch || p.Type == NodeType.Fork)
							{
								p.Connections.Add(connectionId);
							}
						}
					}
				}
			}
			_currentDialogue.CurrentTree.Nodes.Remove(curr);
			_ids.Clear();
			EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); //to actual mark scene as dirty
		}

		void RemoveNodeAndChildren(object win)
		{
			Undo.RecordObject(_currentDialogue, "Remove Node And Children");
			Node curr = (Node) win;
			//If this is the first node, removes everything
			if (curr.ID == _currentDialogue.CurrentTree.FirstNode)
			{
				if (curr.Connections.Count == 0)
					_currentDialogue.CurrentTree.FirstNode = -562;
				return;
			}
			
			//remove all connections to this node
			var prevNodes = FindPreviousNodes(curr);
			foreach (var prevNode in prevNodes)
			{
				prevNode.Connections.Remove(curr.ID);
			}

			//gather all connecting nodes and delete
			var nodesToDelete = new List<int>();
			nodesToDelete.Add(curr.ID);
			GatherNodesRecursive(nodesToDelete, curr.Connections);
			foreach (var i in nodesToDelete)
			{
				_currentDialogue.CurrentTree.Nodes.Remove(_currentDialogue.CurrentTree.GetNode(i));
			}
		}

		private void GatherNodesRecursive(List<int> nodesToDelete, List<int> currConnections)
		{
			for (int i = 0; i < currConnections.Count; i++)
			{
				var connId = currConnections[i];
				if (!nodesToDelete.Contains(connId))
				{
					nodesToDelete.Add(connId);
					var nextNode = _currentDialogue.CurrentTree.GetNode(connId);
					GatherNodesRecursive(nodesToDelete, nextNode.Connections);
				}
			}
		}

		void CheckConnections()
		{
		/*	for (int i = 0; i < _currentDialogue.CurrentTree.Nodes.Count; i++)
			{
				var curNode = _currentDialogue.CurrentTree.Nodes[i];
				List<Node> previousNodes = FindPreviousNodes(curNode);
				if ((previousNodes == null || previousNodes.Count == 0) && curNode.Type != NodeType.Start &&
				    _currentDialogue.CurrentTree.Nodes.Count > 1)
				{
					RemoveNode(_currentDialogue.CurrentTree.Nodes[i]);
				}
			}*/
		}

		void StartConnection(object win)
		{
			_connecting = true;
			_connectingCurrent = (Node) win;
		}

		void CreateNodeMenuFunc(object win)
		{
			Undo.RecordObject(_currentDialogue, "Dialogue Node Added");
			Node newlyCreatedNode = CreateNewNode((Vector2) win);
			_currentDialogue.CurrentTree.Nodes.Add(newlyCreatedNode);
		}

		void CreateConnection(object win)
		{
			Node curr = (Node) win;
			if (curr == _connectingCurrent) return;
			Undo.RecordObject(_currentDialogue, "Connection Added");
			if (Find(curr, _connectingCurrent) || _connectingCurrent.Connections.Contains(curr.ID))
			{
				if (!_connectingCurrent.Connections.Contains(curr.ID)) // && ConnectingCurrent.Type == WindowTypes.Choice)
				{
					_connectingCurrent.Connections.Add(curr.ID);
				}
				_connecting = false;

				if (curr.Connections.Count > 1 && curr.Type != NodeType.Fork && curr.Type != NodeType.Switch)
				{
					curr.Type = NodeType.Fork;
				}

				return;
			}

			for (int i = 0; i < _connectingCurrent.Connections.Count; i++)
			{
				if (_connectingCurrent.Connections[i] == curr.ID)
				{
					_connecting = false;
					return;
				}
			}


			_connectingCurrent.Connections.Add(curr.ID);
			_connecting = false;
		}

		void EstablishNewConnection(object dataParam)
		{
			NodeCreateData data = (NodeCreateData) dataParam;
			data.PrevNode = _connectingCurrent;
			CreateConnection(AddNode(data));
		}

		void RemoveConnection(object dataParam)
		{
			ConnectionRemoveData data = (ConnectionRemoveData) dataParam;
			Undo.RecordObject(_currentDialogue, "Removing connection");
			//CurrentDialogue.Manager.CurrentTree.Windows.Remove(CurrentDialogue.Manager.CurrentTree.GetWindow(ToRemove));
			bool remove = true;
			var currentNodes = _currentDialogue.CurrentTree.Nodes;
			for (int i = 0; i < currentNodes.Count; i++)
			{
				for (int j = 0; j < currentNodes[i].Connections.Count; j++)
				{
					if (currentNodes[i].Connections[j] == data.Connection && data.Node.ID != currentNodes[i].Connections[j])
					{
						remove = false;
					}
				}
			}
			if (remove)
				currentNodes.Remove(_currentDialogue.CurrentTree.GetNode(data.Connection));
			data.Node.Connections.Remove(data.Connection);
		}

		void CancelConnection()
		{
			_connecting = false;
		}

		void Convert(object win)
		{
			Undo.RecordObject(_currentDialogue, "Change node type");
			Node curr = (Node) win;
			if (curr.Type == NodeType.Script)
			{
				curr.Rect = new Rect(curr.Rect.x, curr.Rect.y, curr.Rect.width, _nodeSize.y);
				curr.Type = NodeType.Video;
				return;
			}

			if (curr.Type == NodeType.Switch)
			{
				curr.Type = NodeType.Video;
				return;
			}
			
			if (curr.Type == NodeType.Fork || curr.Type == NodeType.Connector)
				curr.Type = NodeType.Video;
			else
				curr.Type = NodeType.Fork;
		}

		void ConvertToScript(object win)
		{
			Undo.RecordObject(_currentDialogue, "Change node type");
			Node curr = (Node) win;
			if (curr.Type == NodeType.Video)
			{
				curr.Type = NodeType.Script;
				curr.Rect = new Rect(curr.Rect.x, curr.Rect.y, _nodeSize.x, _nodeSize.y * 0.6f);
			}
		}
	
		void ConvertToSwitch(object win)
		{
			Undo.RecordObject(_currentDialogue, "Change node type");
			Node curr = (Node) win;
			if (curr.Type == NodeType.Video)
			{
				curr.Type = NodeType.Switch;
			}
		}
		
		void ConvertToConnector(object win)
		{
			Undo.RecordObject(_currentDialogue, "Change node type");
			Node curr = (Node) win;
			if (curr.Type == NodeType.Video)
			{
				curr.Type = NodeType.Connector;
			}
		}


		void CheckPosition(Node win)
		{
			Vector2 newPos = win.Rect.position;
			if (win.Rect.center.x < 0)
				win.Rect.position = new Vector2(0, newPos.y);
			if (win.Rect.center.x > _workSize.x)
				win.Rect.position = new Vector2(_workSize.x - (win.Rect.width), newPos.y);
			if (win.Rect.center.y < 0)
				win.Rect.position = new Vector2(newPos.x, 0);
			if (win.Rect.center.y > _workSize.y)
				win.Rect.position = new Vector2(newPos.x, _workSize.y - (win.Rect.height));
		}
		
		void DrawConnections()
		{
			if (!CheckDialogueExists()) return;
			var screenBoundary = new Rect(_scrollPosition.x - 100, _scrollPosition.y - 100, position.width + 200, position.height + 200);
			for (int j = 0; j < _currentDialogue.CurrentTree.Nodes.Count; j++)
			{
				Node nodeStart = _currentDialogue.CurrentTree.Nodes[j];
				Color connColor = nodeStart.Type == NodeType.Fork ? Color.green : Color.white;
				int numberOfConnections = nodeStart.Connections.Count;
				for (int i = 0; i < numberOfConnections; i++)
				{

					var nodeEnd = _currentDialogue.CurrentTree.GetNode(nodeStart.Connections[i]);
					if (nodeEnd == null)
					{
						Debug.LogWarning("Bad connection on " + nodeStart + " to " + nodeStart.Connections[i]);
						continue;
					}
					var scaler = _zoomIn ? ZoomScale : 1f;
					var rectFrom = nodeStart.Rect;
					var rectTo = nodeEnd.Rect;

					if (!screenBoundary.Contains(rectFrom.center*scaler) && !screenBoundary.Contains(rectTo.center*scaler)) continue;
					if (nodeStart.Type == NodeType.Switch)
					{
						connColor = _connectionColors[i];
						var percent = i / (float)numberOfConnections;
						
						rectFrom = new Rect(rectFrom.x < rectTo.x ? rectFrom.xMax-3 : rectFrom.xMin+3, 6+ rectFrom.yMax - (1-percent) * rectFrom.height*0.8f, 3, 3);
					}
					DrawConnection(rectFrom, rectTo, connColor);
				}
			}

			if (_connecting)
			{
				var mousePos = Event.current.mousePosition;
				DrawConnection(_connectingCurrent.Rect, new Rect(mousePos.x - 3, mousePos.y - 3, 6, 6), Color.magenta);
			}
		}

		private void DrawConnection(Rect rectFrom, Rect rectTo, Color color)
		{
			var scaler = _zoomIn ? ZoomScale : 1f;

			Vector2 start, end, tanStart, tanEnd;
			GetBezierPointsFor(rectFrom,rectTo, out start, out end, out tanStart, out tanEnd);

			// Draw the line
			start *= scaler;
			end *= scaler;
			tanStart *= scaler;
			tanEnd *= scaler;
			Handles.DrawBezier(start, end, tanStart, tanEnd, color, null, 5f);

			//Draws arrows
			var points = Handles.MakeBezierPoints(start, end, tanStart, tanEnd, 9);
			var numArrows = Mathf.RoundToInt( (start - end).magnitude / 160f);
			Handles.color = color;
			switch (numArrows)
			{
				case 0: 
				case 1: DrawConnectionArrow(points[3], points[4]);
					break;
				case 2: 
					DrawConnectionArrow(points[2], points[3]);
					DrawConnectionArrow(points[4], points[5]);
					break;
				default:
					DrawConnectionArrow(points[2], points[3]);
					DrawConnectionArrow(points[4], points[5]);
					DrawConnectionArrow(points[6], points[7]);
					break;
			}
		}

		private void GetBezierPointsFor(Rect rectFrom, Rect rectTo, out Vector2 start, out Vector2 end, out Vector2 tanStart, out Vector2 tanEnd)
		{
			start = rectFrom.center;
			end = rectTo.center;
			var diff = rectFrom.center - rectTo.center;
			if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y)) // connect horizontal
			{
				start.x = diff.x > 0 ? rectFrom.xMin : rectFrom.xMax;
				end.x = diff.x > 0 ? rectTo.xMax : rectTo.xMin;
				tanStart = start;
				tanEnd = end;
				tanStart.x += diff.x > 0 ?  -50f : 50f;
				tanEnd.x += diff.x > 0 ? 50 : -50f;
			}
			else // connect vertical
			{
				start.y = diff.y > 0 ? rectFrom.yMin : rectFrom.yMax;
				end.y = diff.y > 0 ? rectTo.yMax : rectTo.yMin;
				tanStart = start;
				tanEnd = end;
				tanStart.y += diff.y > 0 ?  -50f : 50f;
				tanEnd.y += diff.y > 0 ? 50 : -50f;
			}

			var len = (start - end).magnitude;
			if (len < 70)
			{
				tanStart = start;
				tanEnd = end;
			}
		}
		
		private void DrawConnectionArrow(Vector2 posA, Vector2 posB, bool flipArrow = true)
		{
			var size = _zoomIn ? 3 : 5;
			var sizeVec = new Vector2(size, size);
			var middle = new Vector2((posA.x + posB.x) / 2, (posA.y + posB.y) / 2) - sizeVec;
			var top = new Vector2(middle.x - 2*size, middle.y);
			var bottom = new Vector2(middle.x + 2*size, middle.y);
			var side = new Vector2(middle.x, middle.y + 3*size);

			float additionAngle = flipArrow ? -90 : 90;
			var angl = AngleBetweenVector2(posA, posB);
			var rotationTop = RotateVector2(top, middle, angl + additionAngle) + sizeVec;
			var rotationBottom = RotateVector2(bottom, middle, angl + additionAngle) + sizeVec;
			var rotationSide = RotateVector2(side, middle, angl + additionAngle) + sizeVec;
			Handles.DrawAAConvexPolygon(rotationTop, rotationBottom, rotationSide);
		}
		
		private bool IsOverLine(Vector2 point, Rect rectFrom, Rect rectTo)
		{
			Vector2 start, end, tanStart, tanEnd;
			GetBezierPointsFor(rectFrom,rectTo, out start, out end, out tanStart, out tanEnd);

			var maxdist = 28f;
			// get distance from line points
			var points = Handles.MakeBezierPoints(start, end, tanStart, tanEnd, 12);
			for (var index = 1; index < points.Length-2; index++)
			{
				var v1 = points[index];
				var v2 = points[index + 1];
				var vMid = (v1 + v2) * 0.5f;
				var vMid2 = (v1 + vMid) * 0.5f;
				var vMid3 = (v2 + vMid) * 0.5f;
				if ( Vector2.Distance(v1, point) < maxdist) return true;
				if ( Vector2.Distance(vMid, point) < maxdist) return true;
				if ( Vector2.Distance(vMid2, point) < maxdist) return true;
				if ( Vector2.Distance(vMid3, point) < maxdist) return true;
			}

			return false;
		}

		//private static GUIStyle nodeStyle;
		void BuildNodes()
		{
			if (FindCurrentDialogue() && !CheckDialogueExists())
			{
				if (_currentDialogue?.CurrentTree?.Nodes != null)
				{
					if (_currentDialogue.CurrentTree.Nodes.Count == 0)
					{
						Node newlyCreatedNode = CreateNewNode(new Vector2(150,150), NodeType.Start);
						newlyCreatedNode.ReadableID = "Start";
						_currentDialogue.CurrentTree.FirstNode = newlyCreatedNode.ID;
						_currentDialogue.CurrentTree.Nodes.Add(newlyCreatedNode);
					}
				}
				return;
			}

			if (_currentDialogue?.CurrentTree?.Nodes == null)
			{
				Debug.Log("Dialog Editor Failed: currentDialogue?.CurrentTree?.Nodes");
				return;
			}

			RemoveTreeOffset(_currentDialogue.CurrentTree.Nodes);
			var screenBoundary = new Rect(_scrollPosition.x - 100, _scrollPosition.y - 100, position.width + 200, position.height + 200);
			for (int j = 0; j < _currentDialogue.CurrentTree.Nodes.Count; j++)
			{
				Node node = _currentDialogue.CurrentTree.Nodes[j];
				if (node.Connections.Count == 2)
				{
					if (node.Connections[0] == node.Connections[1])
					{
						node.Connections.RemoveAt(1);
					}
				}
				// Attemps to fix "illogical" wirings
				List<Node> prevNodes = FindPreviousNodes(node);
				if (prevNodes != null)
				{
					for (int i = 0; i < prevNodes.Count; i++)
					{
						if (prevNodes[i].Type == NodeType.Fork)
						{
							if (node.Type != NodeType.ChoiceAnswer && node.Type != NodeType.Conditional)
							{
								node.Type = NodeType.ChoiceAnswer;
								node.AnswerOrder = prevNodes[i].Connections.Count-1;
								node.Text = "-";
								break;
							}
						}

						if (prevNodes[i].Type == NodeType.ChoiceAnswer && node.Type == NodeType.ChoiceAnswer)
							node.Type = NodeType.Video;
						if (prevNodes[i].Type == NodeType.Video && node.Type == NodeType.ChoiceAnswer)
							node.Type = NodeType.Video;
					}
				}

				if (node.Type == NodeType.Conditional && node.Connections.Count == 0)
				{
					node.Type = NodeType.ChoiceAnswer;
				}
				// ========

				string boxName = Enum.GetName(typeof(NodeType), node.Type);
				if (node.Type == NodeType.ChoiceAnswer)
				{
					boxName = "Answer";
				}
				else if (node.Type == NodeType.Conditional)
				{
					boxName = _zoomIn? "C" : "Condition";
				}else if (node.Type == NodeType.Video && _currentDialogue.TextMode)
				{
					boxName = "Text";
				}
			
				if (Dialogues.CurrentlyPlayedNode == node)
				{
					boxName = "••• " + boxName + " •••";
				}

				if (node.ID == _currentDialogue.CurrentTree.FirstNode)
					node.Type = NodeType.Start;
				else if( node.Type == NodeType.Start)
					node.Type = NodeType.Video;

				//Sets up the ID for the window
				object[] vals = {node};
				if (!_ids.ContainsKey(node.ID))
					_ids.Add(node.ID, vals);

				//Set node background styl
				string style = "flow node 0";
				if (node.Type == NodeType.Fork) style = "flow node 3";
				if (node.Type == NodeType.Start) style = "flow node 1";
				if (node.Type == NodeType.ChoiceAnswer) style = "flow node 2";
				if (node.Type == NodeType.Connector) style = "flow node 5";
				if (node.Type == NodeType.Switch) style = "flow node 4";
				//Error states
				string errorStyle = "flow node 6";
				if ( (node.Type == NodeType.Fork || node.Type == NodeType.Switch) && node.Connections.Count == 0 ) style = errorStyle;
				if(node.Type == NodeType.Video && node.Video == null && !_currentDialogue.TextMode) style = errorStyle;
				if (node.Type == NodeType.Conditional && !node.Condition.IsValid()) style = errorStyle;
			
				GUIStyle finalStyle = new GUIStyle(style);
				finalStyle.fontSize = 14;
				finalStyle.contentOffset = new Vector2(0, -30);
				CheckPosition(node);

				if (_zoomIn)
				{
					GUI.Window(node.ID, MulRect( node.Rect, ZoomScale ), NodeFunctionZoom, boxName, finalStyle);
				}
				else
				{
					if (screenBoundary.Contains(node.Rect.center)) // do not draw rect outside of view
					{
						var scaleX = _currentDialogue.TextMode && node.Maximized ? 3 : 1;
						var scaleY = _currentDialogue.TextMode && node.Type == NodeType.Video? 1.28f : 1;
						var oldheight = node.Rect.height;
						var oldwidth = node.Rect.width;
						node.Rect = GUI.Window(node.ID, new Rect( node.Rect.x, node.Rect.y, oldwidth*scaleX, oldheight*scaleY ), NodeFunction, boxName, finalStyle);
						node.Rect.width = oldwidth;
						node.Rect.height = oldheight;
					}
				}
			}
		}

		private Rect MulRect(Rect rect, float a)
		{
			return new Rect(rect.x*a, rect.y*a, rect.width*a, rect.height*a);
		}

		private void RemoveTreeOffset(List<Node> currentTreeNodes)
		{
			var minX = 9999f;
			foreach (var node in currentTreeNodes)
			{
				if (node.Rect.x < minX) minX = node.Rect.x;
			}
			if (minX > 1000)
			{
				foreach (var node in currentTreeNodes)
				{
					node.Rect.x -= 900;
				}
			}

			var minY = 9999f;
			foreach (var node in currentTreeNodes)
			{
				if (node.Rect.y < minY) minY = node.Rect.y;
			}
			if (minY > 800)
			{
				foreach (var node in currentTreeNodes)
				{
					node.Rect.y -= 700;
				}
			}

			if (minY < 10)
			{
				foreach (var node in currentTreeNodes)
				{
					node.Rect.y += 5;
				}
			}
			
			if (minX < 10)
			{
				foreach (var node in currentTreeNodes)
				{
					node.Rect.x += 5;
				}
			}
		}

		void BuildMenus(GenericMenu menu)
		{
			Vector2 adjustedMousePosition = Event.current.mousePosition + _scrollPosition - new Vector2(50, 50);
			Vector2 adjustedMousePositionLine = Event.current.mousePosition + _scrollPosition;

			if (!CheckDialogueExists())
			{
				var data = new NodeCreateData {Type = NodeType.Video, MousePos = adjustedMousePosition};
				menu.AddItem(new GUIContent("Create First Node"), false, AddNodeWrapper, data);
				return;
			}

			var isOverNode = false;
			
			for (int j = 0; j < _currentDialogue.CurrentTree.Nodes.Count; j++)
			{
				Node treeNode = _currentDialogue.CurrentTree.Nodes[j];

				//Accounts for the area when the user has scrolled
				Rect adjustedArea = new Rect(treeNode.Rect.position - _scrollPosition, treeNode.Rect.size);

				//Checks if the mouse is close enough to a line
				for (int i = 0; i < treeNode.Connections.Count; i++)
				{
					var nod = _currentDialogue.CurrentTree.GetNode(treeNode.Connections[i]);

					if (IsOverLine(adjustedMousePositionLine, nod.Rect, treeNode.Rect))
					{
						ConnectionRemoveData data = new ConnectionRemoveData
						{
							Node = treeNode, Connection = treeNode.Connections[i]
						};
						menu.AddItem(new GUIContent("Remove Connection"), false, RemoveConnection, data);
					}
				}
				
				//Checks if the mouse is on the current box
				if (adjustedArea.Contains(Event.current.mousePosition))
				{
					isOverNode = true;
					if( _connecting ){
						bool startConnectCondition =(_connectingCurrent.Type == NodeType.Start && _connectingCurrent.Connections.Count == 0) || 
						                            (_connectingCurrent.Type == NodeType.Video && _connectingCurrent.Connections.Count == 0) ||
						                            (_connectingCurrent.Type == NodeType.ChoiceAnswer && _connectingCurrent.Connections.Count == 0) ||
						                            _connectingCurrent.Type == NodeType.Fork ||
						                            _connectingCurrent.Type == NodeType.Switch ||
						                            _connectingCurrent.Type == NodeType.Script ||
						                            _connectingCurrent.Type == NodeType.Connector;
						bool endConnectCondition = treeNode.Type != NodeType.Start && _connectingCurrent != treeNode && (treeNode.Type != NodeType.ChoiceAnswer || _connectingCurrent.Type == NodeType.Fork );
						if ( startConnectCondition && endConnectCondition )
						{
							menu.AddItem(new GUIContent("Make Connection"), false, CreateConnection, treeNode);
						}
					}
					else
					{
						if (treeNode.Type == NodeType.Video)
						{
							menu.AddItem(new GUIContent("Convert To Fork Node"), false, Convert, treeNode);
							menu.AddItem(new GUIContent("Convert To Script Node"), false, ConvertToScript, treeNode);
							menu.AddItem(new GUIContent("Convert To Switch Node"), false, ConvertToSwitch, treeNode);
							menu.AddItem(new GUIContent("Convert To Connector"), false, ConvertToConnector, treeNode);
						}
						else if (treeNode.Connections.Count <= 1 && treeNode.Type != NodeType.ChoiceAnswer &&
						         treeNode.Type != NodeType.Conditional && treeNode.Type != NodeType.Start)
						{
							menu.AddItem(new GUIContent("Convert To Video Node"), false, Convert, treeNode);
						}
						else if (treeNode.Type != NodeType.ChoiceAnswer && treeNode.Type != NodeType.Start)
						{
							menu.AddDisabledItem(new GUIContent("Convert To Video Node"));
						}

						if (treeNode.Type != NodeType.ChoiceAnswer)
						{
							menu.AddSeparator("");
						}

						if (treeNode.Type == NodeType.Video && treeNode.Connections.Count == 0 ||
						    treeNode.Type == NodeType.ChoiceAnswer && treeNode.Connections.Count == 0 ||
						    treeNode.Type == NodeType.Fork || treeNode.Type == NodeType.Switch || treeNode.Type == NodeType.Script ||
						    (treeNode.Type == NodeType.Start && treeNode.Connections.Count == 0))
						{
							menu.AddItem(new GUIContent("Connect"), false, StartConnection, treeNode);
						}

						if (treeNode.Type != NodeType.Start)
						{
							menu.AddItem(new GUIContent("Remove Node"), false, RemoveNode, treeNode);
							if (treeNode.Connections.Count > 0)
							{
								menu.AddItem(new GUIContent("Remove Node and Children"), false, RemoveNodeAndChildren, treeNode);
							}
						}
					}
				}
			}

			if (_connecting)
			{
				var mp = adjustedMousePosition;
				if (!isOverNode && (_connectingCurrent.Type == NodeType.Video && _connectingCurrent.Connections.Count == 0 ||
				    _connectingCurrent.Type == NodeType.ChoiceAnswer && _connectingCurrent.Connections.Count == 0 ||
				    _connectingCurrent.Type == NodeType.Fork || _connectingCurrent.Type == NodeType.Switch))
				{
					NodeCreateData createInfoVideo = new NodeCreateData {MousePos = mp, Type = NodeType.Video};
					NodeCreateData createInfoChoice = new NodeCreateData {MousePos = mp, Type = NodeType.Fork};
					NodeCreateData createInfoSwitch = new NodeCreateData {MousePos = mp, Type = NodeType.Switch};
					NodeCreateData createConnectorAnswer = new NodeCreateData {MousePos = mp, Type = NodeType.Connector};
					
					menu.AddItem(new GUIContent("Create Video Node"), false, EstablishNewConnection,
						createInfoVideo);
					menu.AddItem(new GUIContent("Create Switch Node"), false, EstablishNewConnection,
						createInfoSwitch);
					menu.AddItem(new GUIContent("Create Fork Node"), false, EstablishNewConnection,
						createInfoChoice);
					menu.AddItem(new GUIContent("Create Connector Node"), false, EstablishNewConnection,
						createConnectorAnswer);
				}

				menu.AddItem(new GUIContent("Cancel"), false, CancelConnection);
			}
			else if (!isOverNode && menu.GetItemCount() == 0)
			{
				menu.AddItem(new GUIContent("Create Node"), false, CreateNodeMenuFunc, adjustedMousePosition);
			}
		}

		private static GUIStyle WordWrapStyle;
		private static GUIStyle ZoomedOutIDStyle;
		private static GUIStyle AnswerTextStyle;

		void NodeFunctionZoom(int nodeID)
		{
			if (!_ids.ContainsKey(nodeID)) return;
			Node node = (Node) _ids[nodeID][0];
			if (string.IsNullOrWhiteSpace(node.ReadableID) == false)
			{
				if (ZoomedOutIDStyle == null)
				{
					ZoomedOutIDStyle = new GUIStyle();
					ZoomedOutIDStyle.alignment = TextAnchor.UpperCenter;
					ZoomedOutIDStyle.wordWrap = true;
				}
				GUI.Label(new Rect(0, 15, 70, 30), node.ReadableID, ZoomedOutIDStyle);
			}
			if (WordWrapStyle == null)
			{
				WordWrapStyle = new GUIStyle();
				WordWrapStyle.fontSize = 8;
				WordWrapStyle.alignment = TextAnchor.MiddleCenter;
				WordWrapStyle.wordWrap = true;
			}
			if (node.Type == NodeType.ChoiceAnswer && !string.IsNullOrWhiteSpace(node.Text))
			{
#if CE_USE_I2Loc
				var text = LocalizationManager.GetTranslation(node.Text);
#else
			var text = node.Text;
#endif
				if (text != null && text.Length > 35)
				{
					text = text.Substring(0, 35) + "...";
				}
				GUI.Label(new Rect(5, 18, 70, 40), text, WordWrapStyle);
			}

			if (node.Type == NodeType.Video && node.Video != null && !_currentDialogue.TextMode)
			{
				var vidName = node.Video.name;
				if (vidName.Length > 35)
				{
					vidName = vidName.Substring(0, 35) + "...";
				}

				vidName += " (" + Math.Round(node.Video.length) + "s)";
				GUI.Label(new Rect(5, 18, 68, 40), vidName, WordWrapStyle);
			}
			if (node.Type == NodeType.Video && _currentDialogue.TextMode)
			{
				GUI.Label(new Rect(5, 18, 68, 40), node.Text, WordWrapStyle);
			}

			if (node.Type == NodeType.Script && node.Script != null )
			{
				GUI.Label(new Rect(5, 12, 70, 20), node.Script.gameObject.name, WordWrapStyle);
			}
		
			if (node.Type == NodeType.Connector && !string.IsNullOrWhiteSpace(node.ConnectorNode))
			{
				GUI.Label(new Rect(5, 15, 70, 30), "→ " + node.ConnectorTree + "/" + node.ConnectorNode, WordWrapStyle);
			}
		}

		void NodeFunction(int nodeID)
		{
			if (!_ids.ContainsKey(nodeID)) return;
			Node node = (Node) _ids[nodeID][0];


			if (node.Type == NodeType.Connector)
			{
				node.ConnectorTreeIndex = EditorGUILayout.Popup(node.ConnectorTreeIndex, _currentDialogue.TabNames);
				if (node.ConnectorTreeIndex < _currentDialogue.TabNames.Length)
				{
					node.ConnectorTree = _currentDialogue.TabNames[node.ConnectorTreeIndex];
				}
				if (node.ConnectorTreeIndex != _currentTab)
				{
					EditorGUIUtility.labelWidth = 20;
					var namedNodes = new List<string> {"-"};
					var tree = _currentDialogue.Trees[node.ConnectorTreeIndex];
					var ii = 0;
					foreach (var tNode in tree.Nodes)
					{
						if (!string.IsNullOrEmpty(tNode.ReadableID))
						{
							if (node.ConnectorNode == tNode.ReadableID)
							{
								ii = namedNodes.Count;
							}
							namedNodes.Add(tNode.ReadableID);
						}
					}

					var nodesAr = namedNodes.ToArray();
					int newIi = EditorGUILayout.Popup(ii, nodesAr);
					if (ii != newIi)
					{
						Undo.RecordObject(_currentDialogue, "Changed ConnectorNode");
						node.ConnectorNode = nodesAr[newIi];
					}
					EditorGUIUtility.labelWidth = 0;
				}
			}
			else if (node.Type != NodeType.Conditional && !node.Maximized)
			{
				if (GUI.Button(new Rect(130, 0, 20, 15), "+")) AddNodeAfter(node);
			}

			if (node.Type == NodeType.ChoiceAnswer)
			{
				Node prevNode = FindPreviousNode(node);
				if (prevNode != null)
				{
					if (GUI.Button(new Rect(0, 0, 22, 17), prevNode.Type == NodeType.Conditional ? "¢" : "C"))
					{
						AddConditionNodeBefore(node);
					}
				}
			}

			//--- CONDITION -------//
			if (node.Type == NodeType.Conditional)
			{
				if (node.Condition != null)
				{
					GUI.Label(new Rect(0, 14, 120, 20), node.Condition.ToString(), EditorStyles.centeredGreyMiniLabel);
				}
				if (!Application.isPlaying)
				{
					if (GUI.Button(new Rect(20, 34, 80, 20), "edit"))
					{
						ConditionEditor window = (ConditionEditor) GetWindow(typeof(ConditionEditor), false, "Condition Editor");
						window.Condition = node.Condition;
						window.TargetObject = _currentDialogue;
						window.Show();
					}
				}
				else if( node.Condition != null )
				{
					bool condEval = node.Condition.Eval();
					var style = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
					style.normal.textColor = condEval ? Color.green : Color.red;
					GUI.Label(new Rect(0, 34, 120, 20), condEval.ToString(), style);
				}
			}

			string errorMsg = null;


			//--- ID FIELD -------//
			if (node.Type == NodeType.Video || node.Type == NodeType.Fork || node.Type == NodeType.Start)
			{
				EditorGUIUtility.labelWidth = 20;
				EditorGUI.BeginChangeCheck();
				string readableid = EditorGUILayout.TextField("ID", node.ReadableID);
				var foundNode = _currentDialogue.CurrentTree.GetNodeFromName(readableid);
				if (foundNode != null && foundNode != node)
				{
					errorMsg = "Duplicate ID";
				}
				if (!Application.isPlaying && EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(_currentDialogue.gameObject, "Changed ID");
					EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); //to actual mark scene as dirty
					node.ReadableID = readableid;
				}
				EditorGUIUtility.labelWidth = 0;
			}

			//--- VIDEO FIELD -------//
			if (node.Type == NodeType.Video || node.Type == NodeType.Fork)
			{
				if (!_currentDialogue.TextMode)
				{
					EditorGUIUtility.labelWidth = 22;
					EditorGUI.BeginChangeCheck();
					VideoClip clip = (VideoClip) EditorGUILayout.ObjectField("Vid", node.Video, typeof(VideoClip), true);
					if (EditorGUI.EndChangeCheck())
					{
						if (!Application.isPlaying)
						{
							Undo.RecordObject(_currentDialogue, "Changed video");
							EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); //to actual mark scene as dirty
						}

						node.Video = clip;
					}

					EditorGUIUtility.labelWidth = 0;
				}
				else
				{
					string newText = EditorGUILayout.TextArea(node.Text, GUILayout.Height(node.Type == NodeType.Video ? 60 : 20));
					if (node.Text != newText)
					{
						node.Text = newText;
						EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); //to actual mark scene as dirty
					}
					EditorGUIUtility.labelWidth = 42;
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("Image");
					Sprite sp = (Sprite) EditorGUILayout.ObjectField(node.Sprite, typeof(Sprite), false);
					EditorGUILayout.EndHorizontal();
					if (node.Sprite != sp)
					{
						node.Sprite = sp;
						EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); //to actual mark scene as dirty
					}
					
				}
			}

			//--- ORDER FIELD -------//
			if (node.Type == NodeType.ChoiceAnswer)
			{
				EditorGUIUtility.labelWidth = 50;
				var newanswerorder = EditorGUILayout.IntField("Order", node.AnswerOrder);
				if (newanswerorder != node.AnswerOrder)
				{
					node.AnswerOrder = newanswerorder;
					EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
				}
				if (node.AnswerOrder < 0) node.AnswerOrder = 0;
				EditorGUIUtility.labelWidth = 0;
			}

			//--- VISITED VAR FIELD -------//
			if (node.Type == NodeType.ChoiceAnswer || node.Type == NodeType.Fork || node.Type == NodeType.Video)
			{
				if (Application.isPlaying)
				{
					if (node.VisitedVariable != null)
					{
						EditorGUILayout.LabelField("Visited: " + node.VisitedVariable.RuntimeValue);
					}
				}
				else
				{
					EditorGUIUtility.labelWidth = 42;
					EditorGUILayout.BeginHorizontal();
					EditorGUI.BeginChangeCheck();
					BoolVariable visitedVar =
						(BoolVariable) EditorGUILayout.ObjectField(new GUIContent("Visited","This variable will be switched to true, when the game shows this node."), node.VisitedVariable, typeof(BoolVariable),
							true);
					if (EditorGUI.EndChangeCheck())
					{
						if (!Application.isPlaying)
						{
							Undo.RecordObject(_currentDialogue, "Changed visited var");
							EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); //to actual mark scene as dirty
						}
						node.VisitedVariable = visitedVar;
					}
					EditorGUIUtility.labelWidth = 0;
					if (node.VisitedVariable == null && GUILayout.Button(new GUIContent("+", "Create Variable"),GUILayout.Width(20),GUILayout.Height(18)))
					{
						var scene = SceneManager.GetActiveScene();
						var prev = FindPreviousNode(node);
						var prevName = prev == null ? "missing!" : prev.ReadableID;
						if (string.IsNullOrWhiteSpace(prevName))
						{
							prevName = "unnamed";
						} 
						var readableId = node.Type == NodeType.ChoiceAnswer ? prevName + "_" + node.AnswerOrder : node.ReadableID;
						var varname = string.IsNullOrWhiteSpace(readableId) ? node.ID + "_" + _currentDialogue.TabNames[_currentTab] : readableId;
						varname += "_visited.asset";
						var scenepath = scene.path;
						var dirpath = Path.GetDirectoryName(scenepath);
						var varspath = dirpath + "/variables";
						if (!Directory.Exists(varspath))
						{
							Directory.CreateDirectory(varspath);
						}
						node.VisitedVariable = CreateBoolVariable(varspath + "/" + varname);
					}
					EditorGUILayout.EndHorizontal();
				}
			}

			//--- CUSTOM SCRIPT FIELD -------//
			if (node.Type == NodeType.Script)
			{
				EditorGUI.BeginChangeCheck();
				var scriptValue = (CEScript) EditorGUILayout.ObjectField(node.Script, typeof(CEScript), true);
				if (EditorGUI.EndChangeCheck())
				{
					if (!Application.isPlaying)
					{
						Undo.RecordObject(_currentDialogue, "Changed script");
						EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); //to actual mark scene as dirty
					}
					node.Script = scriptValue;
				}

				if (node.Script == null)
				{
					if (GUI.Button(new Rect(50, 56, 55, 17), "Create"))
					{
						var go = new GameObject("Script");
						var scriptsRoot = GameObject.Find("Scripts");
						go.transform.parent = scriptsRoot.transform;
						node.Script = go.AddComponent<MultiScript>();
					}
				}
			}

			//--- TEXT AREA FIELD -------//
			if (node.Type == NodeType.ChoiceAnswer)
			{
#if CE_USE_I2Loc
				if (!string.IsNullOrWhiteSpace(node.Text) && node.Text != "-")
				{

					var localizedText = LocalizationManager.GetTranslation(node.Text);
					if (localizedText != null && localizedText.Length > 52)
					{
						localizedText = localizedText.Substring(0, 52)+"...";
					}
					if ( AnswerTextStyle == null )
					{
						AnswerTextStyle = new GUIStyle();
						AnswerTextStyle.alignment = TextAnchor.MiddleCenter;
						AnswerTextStyle.wordWrap = true;						
						//AnswerTextStyle.normal.textColor = EditorStyles.centeredGreyMiniLabel.normal.textColor;
						AnswerTextStyle.fontSize = 9;
					}		
					EditorGUILayout.LabelField(localizedText,AnswerTextStyle);
				}
				if (!Application.isPlaying)
				{			
					GUI.changed = false;
					string prefix = _currentDialogue.CurrentTree.LocalizationPrefix;
					var termList = LocalizationManager.GetTermsList(prefix);
					termList.Add("-");
					var terms = termList.ToArray();

					int index = (node.Text == "-" || node.Text == "") ? terms.Length - 1 : Array.IndexOf(terms, node.Text);
					if (index < 0)
					{
						index = terms.Length - 1;
					}

					int newIndex = EditorGUILayout.Popup(index, terms);
					if (newIndex != index)
					{
						node.Text = terms[newIndex];
					}

					if (GUI.changed)
					{
						Undo.RecordObject(_currentDialogue.gameObject, "Changed script");
						EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); //to actual mark scene as dirty
					}
				}
				else
				{
					EditorGUILayout.LabelField(node.Text);
				}
#else
			string newText = EditorGUILayout.TextArea(node.Text, GUILayout.Height(45));
			if (node.Text != newText)
			{
				node.Text = newText;
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); //to actual mark scene as dirty
			}
#endif

#if CE_USE_I2Loc
				if (GUI.Button(new Rect(_nodeSize.x - 18, 110, 18, 15), "L"))
				{
					//Select the prefab
					UnityEngine.Object GO = Resources.Load<ScriptableObject>(LocalizationManager.GlobalSources[0]);
					if (GO == null)
						Debug.Log(
							"Unable to find Global Language at Assets/Resources/" + LocalizationManager.GlobalSources[0] + ".prefab");

					Selection.activeObject = GO;
				}
#endif			
			}

			//--  VIDEO TIMERS -- //
			if (node.Type == NodeType.Video && !_currentDialogue.TextMode)
			{
				GUILayout.Space(10);
			
				GUILayout.BeginArea (new Rect(55, 95 , 45, 45));
				string sign = null;
				if (node.TimedEvents != null && node.TimedEvents.Count > 0)
				{
					sign = node.TimedEvents.Count.ToString();
				}
				if (GUILayout.Button(new GUIContent(sign,_timerTexture,"Timers"), GUILayout.Width(45)))
				{
					VideoTimerEditor window = (VideoTimerEditor) GetWindow(typeof(VideoTimerEditor));
					window.VideoNode = node;
					window.Show();
				}
				GUILayout.EndArea ();
			}

			//-- MAXIMIZE BUTTON --//
			if (node.Type == NodeType.Video && _currentDialogue.TextMode)
			{
				if (GUI.Button(new Rect(1, 1, 20, 20), new GUIContent("[]",node.Maximized ? "Minimize" : "Maximize")))
				{
					node.Maximized = !node.Maximized;
				}
			}

			//-- SWITCH --//
			if (node.Type == NodeType.Switch)
			{
				int diff = node.SwitchConditions.Count - node.Connections.Count;
				if (diff > 0)
				{
					node.SwitchConditions.RemoveRange(node.SwitchConditions.Count-diff,diff);
				}
				while (diff < 0 )
				{
					node.SwitchConditions.Add(new Condition());
					diff++;
				}

				var vpadding = node.Connections.Count < 4 ? 30 : 24f;
				for (var i = 0; i < node.Connections.Count; i++)
				{
					var style = new GUIStyle();
					style.normal.textColor = _connectionColors[i%_connectionColors.Length];
					style.fontSize = EditorStyles.centeredGreyMiniLabel.fontSize;
					GUI.Label(new Rect(5, 30 + i*vpadding, 120, 20), node.SwitchConditions[i].ToString(), style);
					if (!Application.isPlaying)
					{
						if ( GUI.Button(new Rect(125, 27 + i * vpadding, 24, 20), "✎") )
						{
							ConditionEditor window = (ConditionEditor) GetWindow(typeof(ConditionEditor));
							window.Condition = node.SwitchConditions[i];
							window.Show();
						}
					}
				}
			}

			//-- START --//
			if (node.Type == NodeType.Start)
			{
#if CE_USE_I2Loc
				var prefix = _currentDialogue.CurrentTree.LocalizationPrefix;
				if (string.IsNullOrWhiteSpace(prefix))
				{
					prefix = _currentDialogue.CurrentTree.Name;
					_currentDialogue.CurrentTree.LocalizationPrefix = prefix;
				}
				GUI.Label(new Rect(0, 60, _nodeSize.x, 20), "Localization prefix:");
				_currentDialogue.CurrentTree.LocalizationPrefix = GUI.TextArea(new Rect(0, 80, _nodeSize.x, 25), prefix);
#endif
			}

			if (errorMsg != null)
			{
				// --- WARNING TEXT ----
				var guistyle = new GUIStyle();
				guistyle.fontStyle = FontStyle.Bold;
				guistyle.normal.textColor = new Color(0.6f, 0.15f, 0.15f);
				guistyle.alignment = TextAnchor.UpperCenter;
				GUI.Label(new Rect(0, 110, 150, 25), errorMsg, guistyle);
			}

			if (IsDebug)
			{
				// --- WARNING TEXT ----
				var guistyle = new GUIStyle();
				guistyle.fontStyle = FontStyle.Bold;
				guistyle.normal.textColor = new Color(0.6f, 0.15f, 0.15f);
				guistyle.alignment = TextAnchor.UpperLeft;
				GUI.Label(new Rect(0, 0, 150, 25), node.ID.ToString(),guistyle);
				
				if (Application.isPlaying && node.Type == NodeType.Fork)
				{
					if (node.ForkType != null && node.ForkType != ForkNodeClassic.Instance)
					{
						GUI.Label(new Rect(2, 80, 110, 25), node.ForkType.GetType().Name, guistyle);
					}
				}

			}

			GUI.DragWindow();
		}

		private BoolVariable CreateBoolVariable(string path)
		{
			var asset = ScriptableObject.CreateInstance<BoolVariable>();
			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			return asset;
		}


		void AddNewTree(int nodeID)
		{
			GUI.Label(new Rect(20, 52, 200, 20), "Tree Name");
			_newTreeName = GUI.TextField(new Rect(100, 50, 200, 20), _newTreeName);
			GUI.Label(new Rect(100, 25, 200, 20), _newTreeInfo);

			if (GUI.Button(new Rect(150, 100, 100, 40), "Add") ||
			    (Event.current.isKey && Event.current.keyCode == KeyCode.Return))
			{
				if (string.IsNullOrEmpty(_newTreeName) || _newTreeName == " ")
					_newTreeInfo = "Must Enter a Name";
				else
				{
					_currentDialogue.CurrentTree.Name = _newTreeName;
				}
			}
			GUI.DragWindow();
		}
	}
}
#endif