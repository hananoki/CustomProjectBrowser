
using HananokiRuntime;
using UnityEditor;
using UnityEngine;

using E = HananokiEditor.CustomProjectBrowser.SettingsEditor;

namespace HananokiEditor.CustomProjectBrowser {

	[System.Serializable]
	public class SettingsEditor {

		public bool Enable;

		public bool showExtension;
		public bool IconClickContext;

		public bool enableExtensionRun;
		public bool adressableSupport;

		public bool guidNotify;
		public bool toolbarOverride;
		public float barOffset = 50;
		public bool debug;
		public float size = 16;

		public bool showLineColor = true;
		public Color lineColorPersonal = new Color( 0, 0, 0, 0.05f );
		public Color lineColorProfessional = new Color( 1, 1, 1, 0.05f );

		public Color extBackColorPersonal = ColorUtils.RGB( 242 );
		public Color extTextColorPersonal = ColorUtils.RGB( 72 );

		public Color extBackColorProfessional = ColorUtils.RGB( 41 );
		public Color extTextColorProfessional = ColorUtils.RGB( 173 );

		public static E i;

		public Color extBackColor {
			get {
				return EditorGUIUtility.isProSkin ? extBackColorProfessional : extBackColorPersonal;
			}
			set {
				if( EditorGUIUtility.isProSkin ) extBackColorProfessional = value;
				else extBackColorPersonal = value;
			}
		}
		public Color extTextColor {
			get {
				return EditorGUIUtility.isProSkin ? extTextColorProfessional : extTextColorPersonal;
			}
			set {
				if( EditorGUIUtility.isProSkin ) extTextColorProfessional = value;
				else extTextColorPersonal = value;
			}
		}
		public Color lineColor {
			get {
				return EditorGUIUtility.isProSkin ? lineColorProfessional : lineColorPersonal;
			}
			set {
				if( EditorGUIUtility.isProSkin ) lineColorProfessional = value;
				else lineColorPersonal = value;
			}
		}


		public static void Load() {
			if( i != null ) return;
			i = EditorPrefJson<E>.Get( Package.editorPrefName );
		}



		public static void Save() {
			EditorPrefJson<E>.Set( Package.editorPrefName, i );
		}
	}

}

