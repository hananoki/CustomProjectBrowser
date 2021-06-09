using HananokiEditor.Extensions;
using System;
using UnityEditor;
using UnityEngine;
using UnityReflection;

#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;



namespace HananokiEditor.CustomProjectBrowser {

	public class GUIToolbar {

		static EditorWindow projectBrowser => UnityEditorProjectWindowUtil.GetProjectBrowserIfExists() as EditorWindow;

		internal static object s_IMGUIContainer;
		internal static object s_IMGUIContainer2;


		static ScriptTreeView m_ScriptTreeView;

		static SessionStateBool s_script = new SessionStateBool( Package.editorPrefName + "GUIToolbar" );


		/////////////////////////////////////////
		public static bool Attach() {
			if( !UnitySymbol.UNITY_2019_1_OR_NEWER ) return false;
			if( s_IMGUIContainer != null ) return false;
			if( projectBrowser == null ) return false;

			s_IMGUIContainer = Activator.CreateInstance( UnityTypes.UnityEngine_UIElements_IMGUIContainer, new object[] { (Action) DrawToolbar } );
#if UNITY_2019_1_OR_NEWER
			IMGUIContainer con = (IMGUIContainer) s_IMGUIContainer;
			con.style.width = 28;
			con.style.height = 20;
			con.style.top = 0;
			con.style.right = 32;
			con.style.position = Position.Absolute;
			con.style.alignSelf = Align.FlexEnd;
#endif
			projectBrowser?.AddIMGUIContainer( s_IMGUIContainer );


			s_IMGUIContainer2 = Activator.CreateInstance( UnityTypes.UnityEngine_UIElements_IMGUIContainer, new object[] { (Action) DrawViewArea } );
#if UNITY_2019_1_OR_NEWER
			IMGUIContainer con2 = (IMGUIContainer) s_IMGUIContainer2;
			con2.style.top = 21;
			con2.style.right = 1;
			con2.style.left = 1;
			con2.style.bottom = 2;
			con2.style.fontSize = 11;
			con2.style.position = Position.Absolute;
			con2.style.visibility = s_script ? Visibility.Visible : Visibility.Hidden;
#endif
			projectBrowser?.AddIMGUIContainer( s_IMGUIContainer2 );

			m_ScriptTreeView = new ScriptTreeView();
			return true;
		}



		/////////////////////////////////////////
		public static void Dettach() {
			projectBrowser?.RemoveIMGUIContainer( s_IMGUIContainer2 );
			projectBrowser?.RemoveIMGUIContainer( s_IMGUIContainer );
			s_IMGUIContainer = null;
		}


		/////////////////////////////////////////
		static void DrawViewArea() {
			if( !s_script ) return;

			GUILayout.BeginArea( new Rect( 0, 0, projectBrowser.position.width, projectBrowser.position.height ) );


			//var rrr = new Rect( 0, 0, projectBrowser.position.width, projectBrowser.position.height-20 );
			//EditorGUI.DrawRect(rrr,Color.blue);
			m_ScriptTreeView.DrawLayoutGUI();

			GUILayout.EndArea();
		}


		/////////////////////////////////////////
		static void DrawToolbar() {
			GUILayout.BeginArea( new Rect( 0, 0, 28, 20 ) );

			//			EditorGUI.DrawRect( , new Color(0,0,1.0f,0.5f) );
			HGUIToolbar.Begin();
			//ScopeHorizontal.Begin( EditorStyles.toolbar );
			if( HGUIToolbar.Toggle( s_script, EditorIcon.cs_script ) ) {
				s_script.Value = !s_script.Value;

				IMGUIContainer con2 = (IMGUIContainer) s_IMGUIContainer2;
				con2.style.visibility = s_script ? Visibility.Visible : Visibility.Hidden;
			}
			HGUIToolbar.End();
			GUILayout.EndArea();

		}
	}
}

#endif
