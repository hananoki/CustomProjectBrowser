using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HananokiEditor.Extensions;
using HananokiRuntime.Extensions;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

using E = HananokiEditor.CustomProjectBrowser.SettingsEditor;
using SS = HananokiEditor.SharedModule.S;


namespace HananokiEditor.CustomProjectBrowser {
	public class Utils  {
		static void _DeleyAttachDockPane() {
			if( !GUIDockPane.Attach() ) return;

			//EditorApplication.update -= _DeleyAttachDockPane;
		}


		public static void AttachDockPane() {
			EditorApplication.update -= _DeleyAttachDockPane;
			EditorApplication.update += _DeleyAttachDockPane;
		}


		public static void DetachDockPane() {
			GUIDockPane.Dettach();
			EditorApplication.update -= _DeleyAttachDockPane;
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