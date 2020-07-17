//#define TEST

using System.IO;
using System;
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
		public class Styles {
			//public GUIStyle label;
			public Color lineColor;

			public Styles() {
				//label = new GUIStyle( EditorStyles.label );
				//label.alignment = TextAnchor.MiddleLeft;
				//label.padding.bottom = 0;
			}
		}

		public static Styles s_styles;


		static CustomProjectBrowser() {
			E.Load();
			EditorApplication.projectWindowItemOnGUI += ProjectWindowItemCallback;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="guid"></param>
		/// <param name="selectionRect"></param>
		static void ProjectWindowItemCallback( string guid, Rect selectionRect ) {

			if( !E.i.Enable ) return;

			if( s_styles == null ) {
				s_styles = new Styles();
				s_styles.lineColor = E.i.lineColor;
			}

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
				r.width = 16;
				//EditorGUI.DrawRect( r, new Color( 0, 0, 1, 0.5f ) );
				if( EditorHelper.HasMouseClick( r ) ) {
					var m = new GenericMenu();
					m.AddItem( SS._OpenInNewInspector, _guid => EditorHelper.ShowNewInspector( GUIDUtils.LoadAssetAtGUID( (string) _guid ) ), guid );
					m.AddItem( S._DuplicateAsset, _guid => EditorHelper.DuplicateAsset( GUIDUtils.LoadAssetAtGUID( (string) _guid ) ), guid );
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

			EditorGUI.DrawRect( pos, s_styles.lineColor );
		}


		static bool IsDetails( Rect rect ) {
			return rect.width > rect.height;
		}
	}


}
