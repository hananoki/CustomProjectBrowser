//#define TEST

using System.IO;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Hananoki.Reflection;
using Hananoki.Extensions;

using UnityObject = UnityEngine.Object;
using E = Hananoki.CustomProjectBrowser.SettingsEditor;
using SS = Hananoki.SharedModule.S;

namespace Hananoki.CustomProjectBrowser {
	[InitializeOnLoad]
	public static class CustomProjectBrowser {

		internal static EditorWindow _window;
		internal static object _IMGUIContainer;
		internal static string _guid = "---";

		internal static object _IMGUIContainerToolbar;

		static CustomProjectBrowser() {
			E.Load();
			EditorApplication.projectWindowItemOnGUI += ProjectWindowItemCallback;
			Selection.selectionChanged += OnSelectionChanged;
		}

		static void OnDrawToolbar() {
			GUILayout.BeginArea( new Rect( 0, 0, _window.position.width, 20 ) );
			HGUIScope.Horizontal( _ );
			void _() {
				GUILayout.Space( 120 );
				if( HEditorGUILayout.IconButton( EditorIcon.folder, "Folder" ) ) {
					EditorApplication.ExecuteMenuItem( "Assets/Create/Folder" );
				}
				if( HEditorGUILayout.IconButton( EditorIcon.icons_processed_unityengine_material_icon_asset, "Material" ) ) {
					EditorApplication.ExecuteMenuItem( "Assets/Create/Material" );
				}
				if( HEditorGUILayout.IconButton( EditorIcon.icons_processed_unityeditor_animations_animatorcontroller_icon_asset, "Animator Controller" ) ) {
					EditorApplication.ExecuteMenuItem( "Assets/Create/Animator Controller" );
				}
				if( HEditorGUILayout.IconButton( EditorIcon.icons_processed_unityengine_animationclip_icon_asset, "Animation" ) ) {
					EditorApplication.ExecuteMenuItem( "Assets/Create/Animation" );
				}
				if( HEditorGUILayout.IconButton( EditorIcon.icons_processed_unityeditorinternal_assemblydefinitionasset_icon_asset , "Assembly Definition" ) ) {
					EditorApplication.ExecuteMenuItem( "Assets/Create/Assembly Definition" );
				}
				if( HEditorGUILayout.IconButton( EditorIcon.icons_processed_unityengine_u2d_spriteatlas_icon_asset, "Sprite Atlas" ) ) {
					EditorApplication.ExecuteMenuItem( "Assets/Create/Sprite Atlas" );
				}
				if( HEditorGUILayout.IconButton( EditorIcon.icons_processed_unityengine_shadervariantcollection_icon_asset, "Shader Variant Collection" ) ) {
					EditorApplication.ExecuteMenuItem( "Assets/Create/Shader/Shader Variant Collection" );
				}
				if( HEditorGUILayout.IconButton( EditorIcon.icons_processed_unityengine_rendertexture_icon_asset, "Render Texture" ) ) {
					EditorApplication.ExecuteMenuItem( "Assets/Create/Render Texture" );
				}
			}
			GUILayout.EndArea();
		}

		static void OnSelectionChanged() {
			if( Selection.assetGUIDs.Length == 0 ) {
				_guid = "---";
			}
			else {
				_guid = Selection.assetGUIDs[ 0 ];
			}
			EditorApplication.RepaintProjectWindow();
		}


		static void OnDrawDockPane() {
			GUILayout.BeginArea( new Rect( 0, 0, _window.position.width, 20 ) );

			var size = _guid.CalcSizeFromLabel();
			var cont = EditorHelper.TempContent( _guid );
			var rect = GUILayoutUtility.GetRect( cont, EditorStyles.label );

			var rr = rect.AlignR( size.x + 16 + 16 + 8 );
			rr.y += 2;
			GUI.Label( rr, _guid );

			if( EditorHelper.HasMouseClick( rr, EventMouseButton.L ) ) {
				var m = new GenericMenu();
				m.AddItem( new GUIContent( SharedModule.S._Copytoclipboard ), false, delegate {
					GUIUtility.systemCopyBuffer = ( _guid );
				} );
				m.DropDownPopupRect( rr );
				Event.current.Use();
			}

			GUILayout.EndArea();
		}


		static void ImporterAction( object context, Action<TextureImporter> action ) {
			var path = GUIDUtils.GetAssetPath( (string) context );
			var files = DirectoryUtils.GetFiles( path, "*", SearchOption.AllDirectories ).Where( x => x.GetExtension() != ".meta" ).ToArray();
			using( new AssetEditingScope() ) {
				foreach( var p in files ) {
					var importer = AssetImporter.GetAtPath( p ) as TextureImporter;
					if( importer == null ) continue;

					action.Invoke( importer );
				}
			}
		}



		static void ShowImp( object context ) {
			var item = (System.ValueTuple<string, Rect>) context;
			var folder = GUIDUtils.LoadAssetAtGUID<DefaultAsset>( item.Item1 );
			FolderImportWindow.s_folder = folder;


			var rr2 = GUIUtility.ScreenToGUIRect( item.Item2 );
			var rr = new Rect();
			rr.x = rr2.x - ( FolderImportWindow.size.x * 0.5f );
			rr.y = rr2.y - FolderImportWindow.size.y;
			var content = new FolderImportWindow();
			//Debug.Log( rr2 );
			PopupWindow.Show( rr, content );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="guid"></param>
		/// <param name="selectionRect"></param>
		static void ProjectWindowItemCallback( string guid, Rect selectionRect ) {

			if( _IMGUIContainer == null ) {
				_IMGUIContainer = Activator.CreateInstance( UnityTypes.IMGUIContainer, new object[] { (Action) OnDrawDockPane } );

				if( E.i.guidNotify ) {
					_window = HEditorWindow.Find( UnityTypes.ProjectBrowser );
					_window?.AddIMGUIContainer( _IMGUIContainer, true );

				}
				//_IMGUIContainer.SetProperty<Rect>( "layout",  new Rect(100,0,200,20)  );
			}
			if( _IMGUIContainerToolbar == null ) {
				_IMGUIContainerToolbar = Activator.CreateInstance( UnityTypes.IMGUIContainer, new object[] { (Action) OnDrawToolbar } );
				if( E.i.toolbarOverride ) {
					_window = HEditorWindow.Find( UnityTypes.ProjectBrowser );
					_window?.AddIMGUIContainer( _IMGUIContainerToolbar, true );
				}
			}

			if( !E.i.Enable ) return;

			if( !IsDetails( selectionRect ) ) return;

			if( E.i.showLineColor ) {
				DrawBackColor( selectionRect, 0x00 );
			}

			//showContextMenu( ContextTargetWindow.Project );
			float maxx = 50;

			if( E.i.showExtension ) {
				maxx = ShowExtention( guid, selectionRect );
			}

			if( IsAdressableSupport() && IsAdressableAssets( guid ) ) {
				var ff = maxx - 20;
				var rcb = new Rect( ff, selectionRect.y, 16, 16 );
				rcb.y += 2;
				GUI.Label( rcb, Icon.Get( "AssetLabelIcon" ), HEditorStyles.iconButton );
				if( EditorHelper.HasMouseClick( rcb ) ) {
					EditorApplication.ExecuteMenuItem( "Window/Asset Management/Addressables/Groups" );
					Event.current.Use();
				}
			}

			if( E.i.IconClickContext && UnityEditorProjectBrowser.IsTwoColumns() ) {

				var r = selectionRect;
				r.x += 3;
				r = r.W( E.i.size ).AlignCenterH( E.i.size );
				if( E.i.debug ) {
					EditorGUI.DrawRect( r, new Color( 0, 0, 1, 0.1f ) );
				}
				if( EditorHelper.HasMouseClick( r ) ) {
					var m = new GenericMenu();
					m.AddItem( SS._OpenInNewInspector, _guid => EditorHelper.ShowNewInspectorWindow( GUIDUtils.LoadAssetAtGUID( (string) _guid ) ), guid );
					m.AddItem( S._DuplicateAsset, _guid => EditorHelper.DuplicateAsset( GUIDUtils.LoadAssetAtGUID( (string) _guid ) ), guid );
					m.AddItem( "TextureImporter", ShowImp, (guid, HGUIUtility.GUIToScreenRect( r )) );
					//m.AddItem( "TextureImporter/Default", ImpDefault, guid );
					//m.AddItem( "TextureImporter/Sprite", ImpSpr, guid );

					//m.AddItem( "TextureImporter/Full Rect", ImpFullRect, guid );
					//m.AddItem( "TextureImporter/ImpSpriteBorder", ImpSpriteBorder, guid );

					if( IsAdressableSupport() ) {
						m.AddSeparator( "" );
						if( IsAdressableAssets( guid ) ) {
							m.AddDisabledItem( S._AddtoAddressable );
						}
						else {
							m.AddItem( S._AddtoAddressable, _guid => {
								UnityAddressableAssetInspectorGUI.SetAaEntry( UnityAddressableAssetSettingsDefaultObject.GetSettings( true ), new UnityObject[] { GUIDUtils.LoadAssetAtGUID( (string) _guid ) }, true );
							}, guid );
						}
					}
#if TEST
					var path = AssetDatabase.GUIDToAssetPath( guid );
					if( !AssetDatabase.IsValidFolder( path ) && Path.GetExtension( path ) == ".mp3" ) {
						//var m = new GenericMenu();
						m.AddItem( "Convert Wav", false, obj => {
							var proc = new System.Diagnostics.Process();

							//proc.WaitForExit();
							proc.StartInfo.FileName = Hananoki.Shared.SharedPreference.i.ffmpeg;
							proc.StartInfo.Arguments = $"-i {path} -y {path.Replace( ".mp3", ".wav" )}";

							proc.EnableRaisingEvents = true;
							//proc.Exited += BuildNspProcess_Exited;
							proc.Start();

							AssetDatabase.SaveAssets();
							AssetDatabase.Refresh();
							return;
						}, guid );
					}
#endif
					m.DropDown( r );
					Event.current.Use();
				}
			}


#if false
			{
				selectionRect.x -= 12;
				selectionRect.width = 16;
				var path = AssetDatabase.GUIDToAssetPath( guid );
				if( path.GetExtension() == ".prefab" ) {
					if(  GUI.Button( selectionRect, Icon.Get( "Toolbar Plus" ),GUIStyle.none  ) ) {
						GameObject.Instantiate( AssetDatabase.LoadAssetAtPath<GameObject>( path ) );
						Debug.Log("aa");
					}
				}
			}
#endif
		}



		/// <summary>
		/// 拡張子を表示します
		/// </summary>
		/// <param name="guid"></param>
		/// <param name="selectionRect"></param>
		/// <returns></returns>
		static float ShowExtention( string guid, Rect selectionRect ) {
			var path = AssetDatabase.GUIDToAssetPath( guid );
			if( AssetDatabase.IsValidFolder( path ) ) return selectionRect.xMax;

			var ext = Path.GetExtension( path );
			if( string.IsNullOrEmpty( ext ) ) return selectionRect.xMax;

			//var label = EditorStyles.label;
			var content = EditorHelper.TempContent( ext );
			var width = HEditorStyles.versionLabel.CalcSize( content ).x;
			var rc = selectionRect;
			rc.x = rc.xMax - width - 12;
			rc.x += 10;
			rc.width = width;

			var rc2 = rc;
			rc2.y += 2;
			if( UnitySymbol.Has( "UNITY_2019_1_OR_NEWER" ) ) {
				rc.x -= 4;
				rc2.x -= 4;
				rc2.width += 2;
			}
			rc2.height -= 4;
			EditorGUI.DrawRect( rc2, SharedModule.SettingsEditor.i.versionBackColor );
			if( E.i.enableExtensionRun && EditorHelper.HasMouseClick( rc2 ) ) {
				System.Diagnostics.Process.Start( "explorer.exe", $"{Environment.CurrentDirectory}/{path}".Replace( "/", "\\" ) );
				Event.current.Use();
			}
			rc.y -= 1;
			HEditorStyles.versionLabel.normal.textColor = SharedModule.SettingsEditor.i.versionTextColor;
			GUI.Label( rc, ext, HEditorStyles.versionLabel );

			return rc.x;
			//EditorGUI.DrawRect( rc, new Color(0,0,1,0.25f) );
		}


		static bool IsAdressableSupport() {
			if( !E.i.adressableSupport ) return false;
			if( R.LoadAssembly( "Unity.Addressables.Editor" ) == null ) return false;
			return true;
		}

		static bool IsAdressableAssets( string guid ) {
			var aaSettings = UnityAddressableAssetSettingsDefaultObject.Settings;
			//return false;
			//var aaSettings = AddressableAssetSettingsDefaultObject.Settings;
			//AddressableAssetEntry entry = null;
			UnityAddressableAssetEntry entry = null;

			if( aaSettings.m_instance != null ) {
				entry = aaSettings.FindAssetEntry( guid );
				if( entry.m_instance != null ) {
					return true;
				}
			}
			return false;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="selectionRect"></param>
		/// <param name="mask"></param>
		static void DrawBackColor( Rect selectionRect, int mask ) {
			var index = ( (int) selectionRect.y ) >> 4;

			if( ( index & 0x01 ) == mask ) return;

			var pos = selectionRect;
			pos.x = 0;
			pos.xMax = selectionRect.xMax;

			EditorGUI.DrawRect( pos, E.i.lineColor );
		}


		static bool IsDetails( Rect rect ) {
			return rect.width > rect.height;
		}
	}


}
