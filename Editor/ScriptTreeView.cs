using HananokiEditor.Extensions;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;


namespace HananokiEditor.CustomProjectBrowser {

	using Item = ScriptTreeView.Item;

	public class ScriptTreeView : HTreeView<Item> {

		public const string k_GenericDragID = "ScriptTreeView.GenericData";


		public class Item : TreeViewItem {
			public int index;
			public MonoScript monoScript;
			public string guid;
		}


		/////////////////////////////////////////
		public ScriptTreeView() : base( new TreeViewState() ) {
			//E.Load();

			showAlternatingRowBackgrounds = true;
			RegisterFiles();
		}


		/////////////////////////////////////////
		public void RegisterFiles() {
			InitID();

			MakeRoot();

			var sc = AssetDatabaseUtils.FindAssetsAndLoad<MonoScript>().OrEmptyIfNull();
#if false
			foreach( var a in sc ) {
				var type = a.GetClass();

				if( type != null ) {
					//if( !aa.IsSubclassOf( typeof( MonoBehaviour ) ) ) continue;
					if( type.FullName.Contains( "Unity." ) ) continue;
					if( type.FullName.Contains( "UnityEngine." ) ) continue;
				}

				var path2 = a.ToAssetPath().DirectoryName().separatorToSlash();
				var path = path2.Split( '/' );
				Item dirItem = m_root;
				int depth = 0;
				foreach( var p in path ) {
					var find = dirItem.children != null ? dirItem.children.Find( x => x.displayName == p ) : null;
					if( find == null ) {
						var item = new Item {
							displayName = p,
							id = GetID(),
							icon = EditorIcon.folder,
							//monoScript = a,
							depth = depth,
							guid = path2.ToGUID(),
						};
						dirItem.AddChild( item );
						dirItem = item;
					}
					else {
						dirItem = (Item) find;
					}

					depth++;
				}

				var it = new Item {
					displayName = a.ToAssetPath().FileNameWithoutExtension(),
					id = GetID(),
					icon = EditorIcon.cs_script,
					monoScript = a,
					guid = a.ToGUID(),
					depth = depth,
				};

				dirItem.AddChild( it );
			}

			void sort( Item item ) {
				if( item.children == null ) return;
				//item.children = item.children.OrderBy( x => x.displayName ).ThenBy( x => ( (Item) x ).monoScript == null ).ToList();
				item.children = item.children.OrderBy( x => ( (Item) x ).monoScript != null ).ToList();
				foreach( var citem in item.children ) {
					sort( (Item) citem );
				}
			}
			sort( m_root );
#else
			foreach( var a in sc ) {
				var type = a.GetClass();
				if( type == null ) continue;
				if( !type.IsSubclassOf( typeof( MonoBehaviour ) ) ) continue;
				if( type.FullName.Contains( "Unity." ) ) continue;
				if( type.FullName.Contains( "UnityEngine." ) ) continue;

				var it = new Item {
					displayName = a.ToAssetPath().FileNameWithoutExtension(),
					id = GetID(),
					icon = EditorIcon.cs_script,
					monoScript = a,
					guid = a.ToGUID(),
				};
				m_root.AddChild( it );
			}
			m_root.children = m_root.children.OrderBy( x => x.displayName ).ToList();

#endif

			ReloadAndSorting();
			//ExpandAll();
		}



		/////////////////////////////////////////
		public void ReloadAndSorting() {
			Reload();
		}

		/////////////////////////////////////////
		//protected override void OnContextClickedItem( int id ) {
		//	var item = ToItem( id );

		//	Selection.activeObject = item.guid.LoadAsset<Object>();
		//	var m = new GenericMenu();
		//	foreach( var s in Unsupported.GetSubmenus( "Assets" ) ) {
		//		var mm = L10n.TrPath( s.Remove( 0, 7 ) );
		//		m.AddItem( mm, context => EditorApplication.ExecuteMenuItem( (string) context ), s );
		//	}
		//	m.DropDownAtMousePosition();
		//}


		/////////////////////////////////////////
		protected override void OnSelectionChanged( Item[] items ) {
			Selection.activeObject = items[0].monoScript;
		}


		/////////////////////////////////////////
		protected override void DoubleClickedItem( int id ) {
			var item = ToItem( id );
			if( item.monoScript == null ) return;
			AssetDatabase.OpenAsset( item.monoScript );
		}



		/////////////////////////////////////////
		protected override void OnRowGUI( RowGUIArgs args ) {
			var item = (Item) args.item;

			//if( item.id == 1 && !args.selected ) {
			//	HEditorStyles.sceneTopBarBg.Draw( args.rowRect );
			//}

			DefaultRowGUI( args );
			var rcc = args.rowRect;
			var w = item.depth * 14 + 16;
			rcc = rcc.TrimL( w );
			EditorApplication.projectWindowItemOnGUI( item.guid, rcc );
		}



		protected override void SetupDragAndDrop( SetupDragAndDropArgs args ) {
			if( args.draggedItemIDs == null ) return;

			DragAndDrop.PrepareStartDrag();

			var items = ToItems( args.draggedItemIDs );
			//var selected = new List<Item>();
			//foreach( var id in args.draggedItemIDs ) {
			//	var item = FindItem( id, rootItem ) as Item;
			//	selected.Add( item );
			//}
			//var ss = selected.Select( x => x.guid.ToAssetPath() ).ToArray(); ;
			//DragAndDrop.objectReferences = new UnityObject[] { selected[ 0 ].guid.LoadAsset() };
			DragAndDrop.objectReferences = items.Select( x => x.monoScript ).ToArray();
			DragAndDrop.paths = null;
			//DragAndDrop.paths = new string[10];
			//DragAndDrop.paths[ 0 ] = "aaa";
			//Debug.Log( $"SetupDragAndDrop: {k_GenericDragID}" );
			//DragAndDrop.SetGenericData( k_GenericDragID, ToItems( args.draggedItemIDs ).ToList() );
			DragAndDrop.visualMode = DragAndDropVisualMode.None;
			DragAndDrop.StartDrag( dragID );
		}

		protected override bool CanStartDrag( CanStartDragArgs args ) {
			//if( !HasSelection() ) return false;
			return true;
		}
	}
}
