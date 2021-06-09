//#define TEST

using HananokiEditor.Extensions;
using HananokiRuntime.Extensions;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

using E = HananokiEditor.CustomProjectBrowser.SettingsEditor;
using SS = HananokiEditor.SharedModule.S;

#if UNITY_2019_1_OR_NEWER
//using UnityEditor.UIElements;
#endif

namespace HananokiEditor.CustomProjectBrowser {
	[InitializeOnLoad]
	public static class CustomProjectBrowser {

		internal static string _guid = "---";
		internal static bool isTwoColumns;


		/////////////////////////////////////////
		static CustomProjectBrowser() {
			E.Load();
			EditorApplication.projectWindowItemOnGUI += ProjectWindowItemCallback;
			Selection.selectionChanged += OnSelectionChanged;

			isTwoColumns = ProjectBrowserUtils.IsTwoColumns();

#if UNITY_2019_1_OR_NEWER
			if( E.i.customDockpane ) Utils.AttachDockPane();
			if( E.i.customToolbar ) Utils.AttachToolbar();
#endif
		}


		/////////////////////////////////////////
		static void OnSelectionChanged() {
			isTwoColumns = ProjectBrowserUtils.IsTwoColumns();

			if( Selection.assetGUIDs.Length == 0 ) {
				_guid = "---";
			}
			else {
				_guid = Selection.assetGUIDs[ 0 ];
			}
			EditorApplication.RepaintProjectWindow();
		}





		static void ImporterAction( object context, Action<TextureImporter> action ) {
			var path = context.ContextToAssetPath();
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
			var folder = AssetDatabaseUtils.LoadAssetAtGUID<DefaultAsset>( item.Item1 );
			FolderImportWindow.s_folder = folder;


			var rr2 = GUIUtility.ScreenToGUIRect( item.Item2 );
			var rr = new Rect();
			rr.x = rr2.x - ( FolderImportWindow.size.x * 0.5f );
			rr.y = rr2.y - FolderImportWindow.size.y;
			var content = new FolderImportWindow();
			//Debug.Log( rr2 );
			UnityEditor.PopupWindow.Show( rr, content );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="guid"></param>
		/// <param name="selectionRect"></param>
		static void ProjectWindowItemCallback( string guid, Rect selectionRect ) {
			if( !E.i.Enable ) return;

			if( !IsDetails( selectionRect ) ) return;

			if( E.i.showLineColor ) {
				DrawBackColor( selectionRect, 0x00 );
			}

			//showContextMenu( ContextTargetWindow.Project );
			float maxx = 50;

			var assetPath = guid.ToAssetPath();

			// memo 
			// FavoriteとPackagesの判定
			// guidは""なのでアセットパスが取れない
			// Favoriteが必ず一番上に来る前提とするとrect.yは0のはずなので
			// それで分岐するぐらいしか手が思いつかない

			if( E.i.showExtension ) {
				maxx = ShowExtention( guid, selectionRect );
			}

			if( E.i.projectPathOpen && assetPath == "Assets" ) {
				var r = selectionRect.AlignR( 16 );
				if( HEditorGUI.IconButton( r, EditorIcon.folder ) ) ShellUtils.OpenDirectory( fs.currentDirectory );
			}

			if( E.i.iconClickContext && isTwoColumns && !guid.IsEmpty() ) {

				var r = selectionRect;
				r.x += 3;
				r = r.W( E.i.size ).AlignCenterH( E.i.size );
				if( E.i.debug ) {
					EditorGUI.DrawRect( r, new Color( 0, 0, 1, 0.1f ) );
				}
				if( EditorHelper.HasMouseClick( r ) ) {
					var m = new GenericMenu();
					//m.AddDisabledItem( guid );
					if( guid.ToAssetPath().HasExtention( ".asmdef" ) && ExternalPackages.hasAsmdefEditor ) {
						m.AddItem( "Asmdef Editor で編集する", () => ExternalPackages.ExecuteAsmdefEditor( guid.ToAssetPath().FileNameWithoutExtension() ) );
						m.AddSeparator();
					}
					if( guid.LoadAsset().GetType() == typeof( Font ) ) {
						var tmp = "Window/TextMeshPro/Font Asset Creator";
						if( EditorHelper.HasMenuItem( tmp ) ) {
							m.AddItem( tmp.FileNameWithoutExtension(), () => EditorApplication.ExecuteMenuItem( tmp ) );
						}
						m.AddSeparator();
					}

					m.AddItem( SS._OpenInNewInspector, EditorContextHandler.ShowNewInspectorWindow, guid );
					m.AddItem( S._DuplicateAsset, EditorContextHandler.DuplicateAsset, guid );
					m.AddItem( "Copy GUID", ( context ) => {
						var item = (System.ValueTuple<string, Rect>) context;
						var rr = GUIUtility.ScreenToGUIRect( item.Item2 );
						EditorHelper.ShowMessagePop( rr.center, $"Copy GUID\n{item.Item1}" );
					}, (guid, HGUIUtility.GUIToScreenRect( r )) );

					m.AddItem( "TextureImporter", ShowImp, (guid, HGUIUtility.GUIToScreenRect( r )) );
					//m.AddItem( "TextureImporter/Default", ImpDefault, guid );
					//m.AddItem( "TextureImporter/Sprite", ImpSpr, guid );

					//m.AddItem( "TextureImporter/Full Rect", ImpFullRect, guid );
					//m.AddItem( "TextureImporter/ImpSpriteBorder", ImpSpriteBorder, guid );

					if( UnityProject.URP ) {
						if( guid.LoadAsset().GetType() == typeof( Material ) ) {
							Selection.activeObject = guid.LoadAsset();
							m.AddItem( "Upgrade Selected Materials to UniversalRP Materials", GraphicsSettingsUtils.選択中のマテリアルをアップグレードする );
						}
						else if( guid.ToAssetPath().IsExistsDirectory() ) {
							m.AddItem( "Upgrade Selected Materials to UniversalRP Materials", () => {
								var files = AssetDatabase.FindAssets( "t:Material", new string[] { guid.ToAssetPath() } ).Select( x => x.LoadAsset() ).ToArray();
								Selection.objects = files;
								GraphicsSettingsUtils.選択中のマテリアルをアップグレードする();
							} );
						}
					}
					else if( UnityProject.HDRP ) {
						if( guid.LoadAsset().GetType() == typeof( Material ) ) {
							Selection.activeObject = guid.LoadAsset();
							m.AddItem( "Upgrade Selected Materials to High Definition Materials", GraphicsSettingsUtils.選択中のマテリアルをアップグレードする );
						}

					}

					//PreferBinarySerialization
					m.AddItem( "ForceReserializeAssets", EditorContextHandler.ForceReserializeAssets, guid );

					//if( IsAdressableSupport() ) {
					//	m.AddSeparator( "" );
					//	if( IsAdressableAssets( guid ) ) {
					//		m.AddDisabledItem( S._AddtoAddressable );
					//	}
					//	else {
					//		m.AddItem( S._AddtoAddressable, _guid => {
					//			UnityAddressableAssetInspectorGUI.SetAaEntry( UnityAddressableAssetSettingsDefaultObject.GetSettings( true ), new UnityObject[] { AssetDatabaseUtils.LoadAssetAtGUID( (string) _guid ) }, true );
					//		}, guid );
					//	}
					//}

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

			if( E.i.externalLink ) {
				ExternalPackages.PBcall?.Invoke( assetPath, guid, ref selectionRect );
			}

			//EditorGUI.DrawRect( selectionRect , Color.white);
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
			if( UnitySymbol.UNITY_2019_1_OR_NEWER ) {
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
