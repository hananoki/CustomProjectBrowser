//#define ENABLE_LEGACY_PREFERENCE

using UnityEditor;
using UnityEngine;
using Hananoki.SharedModule;
using Hananoki.Reflection;

using Settings = Hananoki.CustomProjectBrowser.SettingsEditor;
using SS = Hananoki.SharedModule.S;

namespace Hananoki.CustomProjectBrowser {

	[System.Serializable]
	public class SettingsEditor {
		
		public bool Enable;

		public bool showExtension;
		public bool IconClickContext;

		public bool showLineColor = true;
		public Color lineColorPersonal = new Color( 0, 0, 0, 0.05f );
		public Color lineColorProfessional = new Color( 1, 1, 1, 0.05f );

		public Color extBackColorPersonal = ColorUtils.RGB( 242 );
		public Color extTextColorPersonal = ColorUtils.RGB( 72 );

		public Color extBackColorProfessional = ColorUtils.RGB( 41 );
		public Color extTextColorProfessional = ColorUtils.RGB( 173 );

		public static Settings i;

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
			i = EditorPrefJson<Settings>.Get( Package.editorPrefName );
		}



		public static void Save() {
			EditorPrefJson<Settings>.Set( Package.editorPrefName, i );
		}
	}



	public class SettingsEditorWindow : HSettingsEditorWindow {

		public static void Open() {
			var window = GetWindow<SettingsEditorWindow>();
			window.SetTitle( new GUIContent( Package.name, Icon.Get( "SettingsIcon" ) ) );
		}

		void OnEnable() {
			drawGUI = DrawGUI;
			Settings.Load();
		}



		/// <summary>
		/// 
		/// </summary>
		static void DrawGUI() {

			EditorGUI.BeginChangeCheck();

			using( new PreferenceLayoutScope() ) {

				Settings.i.Enable = HEditorGUILayout.ToggleLeft( SS._Enable, Settings.i.Enable );

				GUILayout.Space( 8f );

				using( new EditorGUI.DisabledGroupScope( !Settings.i.Enable ) ) {
					Settings.i.showExtension = HEditorGUILayout.ToggleLeft( S._ShowExtension, Settings.i.showExtension );
					using( new EditorGUI.DisabledGroupScope( !Settings.i.showExtension ) ) {
						EditorGUI.indentLevel++;
						Settings.i.extBackColor = EditorGUILayout.ColorField( SS._BackColor, Settings.i.extBackColor );
						Settings.i.extTextColor = EditorGUILayout.ColorField( SS._TextColor, Settings.i.extTextColor );
						EditorGUI.indentLevel--;
					}

					Settings.i.showLineColor = HEditorGUILayout.ToggleLeft( SS._Changecolorforeachrow, Settings.i.showLineColor );

					using( new EditorGUI.DisabledGroupScope( !Settings.i.showLineColor ) ) {
						EditorGUI.indentLevel++;
						Settings.i.lineColor = EditorGUILayout.ColorField( SS._Rowcolor, Settings.i.lineColor );
						EditorGUI.indentLevel--;
					}

					GUILayout.Space( 8f );
					EditorGUILayout.LabelField( $"* {SS._Experimental}", EditorStyles.boldLabel );
					Settings.i.IconClickContext = HEditorGUILayout.ToggleLeft( SS._ContextMenuWithIconClick, Settings.i.IconClickContext );
				}
			}

			GUILayout.Space( 8f );
			

			if( EditorGUI.EndChangeCheck() ) {
				Settings.Save();
				CustomProjectBrowser.s_styles.lineColor = Settings.i.lineColor;
				EditorApplication.RepaintProjectWindow();
			}

			GUILayout.Space( 8f );

		}





#if UNITY_2018_3_OR_NEWER && !ENABLE_LEGACY_PREFERENCE
		static void titleBarGuiHandler() {
			GUILayout.Label( $"{Package.version}", EditorStyles.miniLabel );
		}
		[SettingsProvider]
		public static SettingsProvider PreferenceView() {
			var provider = new SettingsProvider( $"Preferences/Hananoki/{Package.name}", SettingsScope.User ) {
				label = $"{Package.name}",
				guiHandler = PreferencesGUI,
				titleBarGuiHandler = titleBarGuiHandler,
			};
			return provider;
		}
		public static void PreferencesGUI( string searchText ) {
#else
		[PreferenceItem( CustomProjectBrowser.PACKAGE_NAME )]
		public static void PreferencesGUI() {
#endif
			Settings.Load();
			DrawGUI();
		}
	}
}

