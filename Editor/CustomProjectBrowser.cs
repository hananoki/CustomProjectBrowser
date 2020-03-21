//#define TEST

using System.IO;
using System;
using UnityEditor;
using UnityEngine;
using Hananoki.UnityReflection;

using Hananoki.SharedModule;

using UnityObject = UnityEngine.Object;
using Settings = Hananoki.CustomProjectBrowser.SettingsEditor;
using SS = Hananoki.SharedModule.S;

namespace Hananoki.CustomProjectBrowser {
	[InitializeOnLoad]
	public static class CustomProjectBrowser {
		public class Styles {
			public GUIStyle label;
			public Color lineColor;

			public Styles() {
				label = new GUIStyle( EditorStyles.label );
				label.alignment = TextAnchor.MiddleLeft;
				label.padding.bottom = 0;
			}
		}

		public static Styles s_styles;


		static CustomProjectBrowser() {
			Settings.Load();
			EditorApplication.projectWindowItemOnGUI += ProjectWindowItemCallback;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="guid"></param>
		/// <param name="selectionRect"></param>
		static void ProjectWindowItemCallback( string guid, Rect selectionRect ) {

			if( !Settings.i.Enable ) return;

			if( s_styles == null ) {
				s_styles = new Styles();
				s_styles.lineColor = Settings.i.lineColor;
			}

			if( !IsDetails( selectionRect ) ) return;

			if( Settings.i.showLineColor ) {
				DrawBackColor( selectionRect, 0x00 );
			}

			//showContextMenu( ContextTargetWindow.Project );

			if( Settings.i.showExtension ) {
				var path = AssetDatabase.GUIDToAssetPath( guid );
				if( AssetDatabase.IsValidFolder( path ) ) return;
				
				var ext = Path.GetExtension( path );
				if( string.IsNullOrEmpty( ext ) ) return;

				var label = EditorStyles.label;
				var content = EditorHelper.TempContent( ext );
				var width = s_styles.label.CalcSize( content ).x;
				var rc = selectionRect;
				rc.x = rc.xMax - width - 12;
				rc.x += 10;
				rc.width = width;

				var rc2 = rc;
				rc2.y += 2;
#if UNITY_2019_1_OR_NEWER
				rc.x -= 4;
				rc2.x -= 4;
				rc2.width += 2;
#endif
				rc2.height -= 4;
				EditorGUI.DrawRect( rc2, Settings.i.extBackColor );
				if(EditorHelper.HasMouseClick( rc2 ) ) {
					System.Diagnostics.Process.Start( "explorer.exe", $"{Environment.CurrentDirectory}/{path}".Replace("/","\\") );
					Event.current.Use();
				}
				rc.y -= 1;
				s_styles.label.normal.textColor = Settings.i.extTextColor;
				GUI.Label( rc, ext, s_styles.label );

			}


			if( Settings.i.IconClickContext && UnityEditorProjectBrowser.IsTwoColumns() ) {
				var uobj = GUIDUtils.LoadAssetAtGUID<UnityObject>( guid );

				var r = selectionRect;
				r.x += 3;
				r.width = 16;
				//EditorGUI.DrawRect( r, new Color( 0, 0, 1, 0.5f ) );
				if( EditorHelper.HasMouseClick( r ) ) {
					var m = new GenericMenu();
					m.AddItem( SS._OpenInNewInspector, false, _uobj => EditorHelper.ShowNewInspector( _uobj.ToCast<UnityObject>() ), uobj );
					m.AddItem( S._DuplicateAsset, false, _uobj => EditorHelper.DuplicateAsset<UnityObject>( _uobj.ToCast<UnityObject>() ), uobj );

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
