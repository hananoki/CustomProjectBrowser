// warning CS0618: 'PrefabUtility.GetPrefabParent(Object)' is obsolete: 'Use GetCorrespondingObjectFromSource.'
#pragma warning disable 618

//#define TEST

using HananokiEditor.Extensions;
using HananokiRuntime;
using HananokiRuntime.Extensions;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using E = HananokiEditor.CustomProjectBrowser.SettingsEditor;
using SS = HananokiEditor.SharedModule.S;



namespace HananokiEditor.CustomProjectBrowser {
	[InitializeOnLoad]
	public static class CustomProjectBrowser {

		internal static string s_guid = "---";
		internal static bool s_isTwoColumns;

		internal static string s_lastGuid = "---";
		internal static int s_lastSub;

		internal static bool s_test;

		/////////////////////////////////////////
		static CustomProjectBrowser() {
			E.Load();

			EditorApplication.projectWindowItemOnGUI -= ProjectWindowItemCallback;
			EditorApplication.projectWindowItemOnGUI += ProjectWindowItemCallback;

			Selection.selectionChanged -= OnSelectionChanged;
			Selection.selectionChanged += OnSelectionChanged;

			s_isTwoColumns = ProjectBrowserUtils.isTwoColumns;

			if( E.i.customDockpane ) Utils.AttachDockPane();
		}



		/////////////////////////////////////////
		static void OnSelectionChanged() {
			s_isTwoColumns = ProjectBrowserUtils.isTwoColumns;

			if( Selection.assetGUIDs.Length == 0 ) {
				s_guid = "---";
			}
			else {
				s_guid = Selection.assetGUIDs[ 0 ];
			}
			//EditorApplication.RepaintProjectWindow();
		}



		/////////////////////////////////////////
		static void ProjectWindowItemCallback( string guid, Rect selectionRect ) {
			s_test = true;
			DrawProjectItemCallback( guid, 0, selectionRect );
			s_test = false;
		}


		/////////////////////////////////////////
		static void DrawProjectItemCallback( string guid, long localID, Rect selectionRect ) {
			if( !E.i.Enable ) return;

			if( !IsDetails( selectionRect ) ) return;

			if( E.i.showLineColor ) DrawBackColor( guid, localID, selectionRect, 0x00 );


			var assetPath = guid.ToAssetPath();

			// memo 
			// FavoriteとPackagesの判定
			// guidは""なのでアセットパスが取れない
			// Favoriteが必ず一番上に来る前提とするとrect.yは0のはずなので
			// それで分岐するぐらいしか手が思いつかない

			if( !guid.IsEmpty() ) {
				if( E.i.showExtension ) 拡張子を表示する( guid, assetPath, selectionRect );
				if( E.i.showAssetType ) アセットタイプを表示する( guid, localID, assetPath, selectionRect );

				if( E.i.projectPathOpen && assetPath == "Assets" ) {
					var r = selectionRect.AlignR( 16 );
					if( HEditorGUI.IconButton( r, EditorIcon.folder ) ) ShellUtils.OpenDirectory( fs.currentDirectory );
				}

				if( E.i.iconClickContext && s_isTwoColumns ) {
					コンテキストメニューを表示する( guid, assetPath, selectionRect );
				}

				// フォーカスインスペクタ
				if( E.i.focusedInspectorsButton && !AssetDatabase.IsValidFolder( assetPath ) ) {
					//Debug.Log($"{selectionRect.x} {assetPath}" );
					//float si = 20;
					if( 0 != localID ) {
						var asset = AssetDatabaseCache.LoadAssetAtGUIDAndLocalID( guid, localID );

						var rc = selectionRect;
						var size = asset.unityObject.name.CalcSize( HEditorStyles.treeViewLine );
						rc.x += size.x + 16 + 8;
						//rc.x = 0; //サブアセットfoldと干渉する
						rc.width = 16;
						if( HEditorGUI.IconButton( rc, EditorIcon.tab_next ) ) {
							EditorContextHandler.ShowNewInspectorWindow( guid );
						}
					}
					else {
						if( !ProjectBrowserUtils.isSearching ) {
							if( s_lastGuid == guid ) {
								var objs = guid.LoadAllSubAssets().Where( x => !x.hideFlags.HasFlag( HideFlags.HideInHierarchy ) ).ToArray();
								if( s_lastSub < objs.Length ) {
									var obj = objs[ s_lastSub ];

									var rc = selectionRect;
									var fname = obj.name;
									var size = fname.CalcSize( HEditorStyles.treeViewLine );
									rc.x += size.x + 16 + 8;

									rc.width = 16;
									if( HEditorGUI.IconButton( rc, EditorIcon.tab_next ) ) {
										EditorContextHandler.ShowNewInspectorWindow( obj );
									}
								}
								else {
								}
								s_lastSub++;
							}
							else {
								s_lastGuid = guid;
								s_lastSub = 0;
								var rc = selectionRect;
								var fname = assetPath.FileNameWithoutExtension();
								var size = fname.CalcSize( HEditorStyles.treeViewLine );
								rc.x += size.x + 16 + 8;
								//rc.x = 0; //サブアセットfoldと干渉する
								rc.width = 16;
								if( HEditorGUI.IconButton( rc, EditorIcon.tab_next ) ) {
									EditorContextHandler.ShowNewInspectorWindow( guid );
								}
							}
						}
					}
				}
			}
			else {
				// Favorite or Packages
				s_isTwoColumns = ProjectBrowserUtils.isTwoColumns;
			}


			if( E.i.externalLink && s_test ) {
				ExternalPackages.PBcall?.Invoke( assetPath, guid, ref selectionRect );
			}


			if( E.i.notifyPrefabParent ) {
				if( Selection.activeGameObject != null ) {
					var prt = PrefabUtility.GetPrefabParent( Selection.activeGameObject );
					if( prt != null && prt.ToAssetPath() == assetPath ) {
						var cc = Color.green;
						cc.a = 0.2f;

						var x = selectionRect.xMax;
						selectionRect.x = 0;
						selectionRect.xMax = x;
						EditorGUI.DrawRect( selectionRect, cc );
					}

				}
			}
		}


		/////////////////////////////////////////
		static void コンテキストメニューを表示する( string guid, string assetPath, Rect selectionRect ) {
			var r = selectionRect;
			r.x += 3;
			r = r.W( E.i.size ).AlignCenterH( E.i.size );
			if( E.i.debug ) {
				EditorGUI.DrawRect( r, new Color( 0, 0, 1, 0.1f ) );
			}
			if( EditorHelper.HasMouseClick( r ) ) {
				var m = new GenericMenu();
				//m.AddDisabledItem( guid );
				if( assetPath.HasExtention( ".asmdef" ) && ExternalPackages.AsmdefGraph.enabled ) {
					m.AddItem( "Asmdef Editor で編集する", () => ExternalPackages.AsmdefGraph.OpenAsName( guid ) );
					m.AddSeparator();
				}
				if( guid.GetAssetType() == typeof( Font ) ) {
					var tmp = "Window/TextMeshPro/Font Asset Creator";
					if( EditorHelper.HasMenuItem( tmp ) ) {
						m.AddItem( tmp.FileNameWithoutExtension(), () => EditorApplication.ExecuteMenuItem( tmp ) );
					}
					m.AddSeparator();
				}

				//m.AddItem( SS._OpenInNewInspector, EditorContextHandler.ShowNewInspectorWindow, guid );
				m.AddItem( S._DuplicateAsset, EditorContextHandler.DuplicateAsset, guid );
				m.AddItem( SS._CopyGUIDToClipboard, ( context ) => {
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
						m.AddItem( SS._UpgradeSelectedMaterialstoUniversalRPMaterials, GraphicsSettingsUtils.選択中のマテリアルをアップグレードする );
					}
					else if( guid.ToAssetPath().IsExistsDirectory() ) {
						m.AddItem( SS._UpgradeSelectedMaterialstoUniversalRPMaterials, () => {
							var files = AssetDatabase.FindAssets( "t:Material", new string[] { guid.ToAssetPath() } ).Select( x => x.LoadAsset() ).ToArray();
							Selection.objects = files;
							GraphicsSettingsUtils.選択中のマテリアルをアップグレードする();
						} );
					}
				}
				else if( UnityProject.HDRP ) {
					if( guid.LoadAsset().GetType() == typeof( Material ) ) {
						Selection.activeObject = guid.LoadAsset();
						m.AddItem( SS._UpgradeSelectedMaterialstoHighDefinitionMaterials, GraphicsSettingsUtils.選択中のマテリアルをアップグレードする );
					}

				}

				//PreferBinarySerialization
				m.AddItem( SS._ForceReserializeAssets, EditorContextHandler.ForceReserializeAssets, guid );

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



		/////////////////////////////////////////
		static void アセットタイプを表示する( string guid, long localID, string assetPath, Rect selectionRect ) {
			var asset = AssetDatabaseCache.LoadAssetAtGUIDAndLocalID( guid, localID );
			if( asset != null ) {
				var type = asset.unityObject.GetTypeSafe();
				asset.label = assetPath.Extension();

				if( type == typeof( MonoScript ) && asset.monoScript.GetClass().指定クラスを含む( typeof( MonoBehaviour ) ) ) {
					asset.label = "MonoBehaviour";
				}
				else if( type == typeof( MonoScript ) && asset.monoScript.GetClass().指定クラスを含む( typeof( ScriptableObject ) ) ) {
					asset.label = "ScriptableObject";
				}
				//else if( type == typeof( TextAsset ) || type == typeof( AudioClip ) ) {
				//	str = assetPath.Extension();
				//}
				else if( type == typeof( Material ) ) {
					var ss = asset.material.shader.name.Split( '/' ).ToList();
					ss.RemoveAt( 0 );
					asset.label = string.Join( "/", ss );
				}
				else if( AssetDatabase.IsValidFolder( assetPath ) ) {
					asset.label = "";
				}
				else if( asset.unityObject.IsSubAsset() ) {
					asset.label = "";
				}
			}
			if( asset.label.IsEmpty() ) return;

			var rect = HEditorGUI.MiniLabelR( selectionRect, asset.label );

			if( E.i.enableExtensionRun && EditorHelper.HasMouseClick( rect ) ) {
				ShellUtils.Start( "explorer.exe", $"{fs.currentDirectory}/{assetPath}".separatorToOS() );
				Event.current.Use();
			}
		}


		/////////////////////////////////////////
		static void 拡張子を表示する( string guid, string assetPath, Rect selectionRect ) {
			if( AssetDatabase.IsValidFolder( assetPath ) ) return;

			var ext = assetPath.Extension();
			if( ext.IsEmpty() ) return;

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
				ShellUtils.Start( "explorer.exe", $"{fs.currentDirectory}/{assetPath}".separatorToOS() );
				Event.current.Use();
			}
			rc.y -= 1;

			HEditorStyles.versionLabel.normal.textColor = SharedModule.SettingsEditor.i.versionTextColor;
			GUI.Label( rc, ext, HEditorStyles.versionLabel );
		}



		/////////////////////////////////////////
		static void DrawBackColor( string guid, long localID, Rect selectionRect, int mask ) {
			var info = AssetDatabaseCache.LoadAssetAtGUIDAndLocalID( guid, localID );
			if( info.missing ) {
				EditorGUI.DrawRect( selectionRect, ColorUtils.RGBA( Color.red, 0.2f ) );
				return;
			}
			var index = ( (int) selectionRect.y ) >> 4;

			if( ( index & 0x01 ) == mask ) return;

			var xMax = selectionRect.xMax;
			selectionRect.x = 0;
			selectionRect.xMax = xMax;

			EditorGUI.DrawRect( selectionRect, E.i.lineColor );
		}


		/////////////////////////////////////////
		static bool IsDetails( Rect rect ) {
			return rect.width > rect.height;
		}




		/////////////////////////////////////////
		// 以下は実験的機能で用意したもの、メンテの対象外


		/////////////////////////////////////////
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


		/////////////////////////////////////////
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
	}
}
