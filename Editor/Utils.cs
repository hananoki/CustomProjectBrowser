using System.Diagnostics;
using UnityEditor;


namespace HananokiEditor.CustomProjectBrowser {
	public class Utils {
#if UNITY_2019_1_OR_NEWER
		static void _DeleyAttachDockPane() {
			if( !GUIDockPane.Attach() ) return;

			//EditorApplication.update -= _DeleyAttachDockPane;
		}
#endif

		[Conditional( "UNITY_2019_1_OR_NEWER" )]
		public static void AttachDockPane() {
#if UNITY_2019_1_OR_NEWER
			EditorApplication.update -= _DeleyAttachDockPane;
			EditorApplication.update += _DeleyAttachDockPane;
#endif
		}


		[Conditional( "UNITY_2019_1_OR_NEWER" )]
		public static void DetachDockPane() {
			GUIDockPane.Dettach();
#if UNITY_2019_1_OR_NEWER
			EditorApplication.update -= _DeleyAttachDockPane;
#endif
		}


#if UNITY_2019_1_OR_NEWER
		static void _DeleyAttachToolbar() {
			if( !GUIToolbar.Attach() ) return;

			EditorApplication.update -= _DeleyAttachToolbar;
		}

		public static void AttachToolbar() {
			EditorApplication.update -= _DeleyAttachToolbar;
			EditorApplication.update += _DeleyAttachToolbar;
		}


		public static void DetachToolbar() {
			GUIToolbar.Dettach();
			EditorApplication.update -= _DeleyAttachToolbar;
		}
#endif
	}
}