//#define ENABLE_LEGACY_PREFERENCE

using UnityEditor;
using UnityEngine;
using Hananoki.Extensions;
using Hananoki.SharedModule;

using E = Hananoki.CustomProjectBrowser.SettingsEditor;
using SS = Hananoki.SharedModule.S;

namespace Hananoki.CustomProjectBrowser {

	[System.Serializable]
	public class SettingsEditor {

		public bool Enable;

		public bool showExtension;
		public bool IconClickContext;

		public bool enableExtensionRun;
		public bool adressableSupport;

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



	/// <summary>
	/// EditorWindow
	/// </summary>
	public class SettingsEditorWindow : HSettingsEditorWindow {

		public static void Open() {
			var w = GetWindow<SettingsEditorWindow>();
			w.SetTitle( new GUIContent( "Project Settings", EditorIcon.settings ) );
			w.headerMame = Package.name;
			w.headerVersion = Package.version;
			w.gui = DrawGUI;
		}



		/// <summary>
		/// 
		/// </summary>
		public static void DrawGUI() {
			E.Load();
			EditorGUI.BeginChangeCheck();

			E.i.Enable = HEditorGUILayout.ToggleLeft( SS._Enable, E.i.Enable );
			EditorGUI.indentLevel++;
			GUILayout.Space( 8f );

			using( new EditorGUI.DisabledGroupScope( !E.i.Enable ) ) {
				E.i.showExtension = HEditorGUILayout.ToggleLeft( S._ShowExtension, E.i.showExtension );
				using( new EditorGUI.DisabledGroupScope( !E.i.showExtension ) ) {
					EditorGUI.indentLevel++;
					E.i.extBackColor = EditorGUILayout.ColorField( SS._BackColor, E.i.extBackColor );
					E.i.extTextColor = EditorGUILayout.ColorField( SS._TextColor, E.i.extTextColor );
					EditorGUI.indentLevel--;
				}

				E.i.showLineColor = HEditorGUILayout.ToggleLeft( SS._Changecolorforeachrow, E.i.showLineColor );

				using( new EditorGUI.DisabledGroupScope( !E.i.showLineColor ) ) {
					EditorGUI.indentLevel++;
					E.i.lineColor = EditorGUILayout.ColorField( SS._Rowcolor, E.i.lineColor );
					EditorGUI.indentLevel--;
				}

				GUILayout.Space( 8f );
				EditorGUILayout.LabelField( $"* {SS._Experimental}", EditorStyles.boldLabel );
				E.i.IconClickContext = HEditorGUILayout.ToggleLeft( SS._ContextMenuWithIconClick, E.i.IconClickContext );
				if( UnitySymbol.Has( "UNITY_EDITOR_WIN" ) ) {
					using( new EditorGUI.DisabledGroupScope( !E.i.showExtension ) ) {
						E.i.enableExtensionRun = HEditorGUILayout.ToggleLeft( S._Clickontheextensiontorunitinthefiler, E.i.enableExtensionRun );
					}
				}
				else {
					E.i.enableExtensionRun = false;
				}
				E.i.adressableSupport = HEditorGUILayout.ToggleLeft( S._EnablingAddressablesupport, E.i.adressableSupport );
			}
			EditorGUI.indentLevel--;


			GUILayout.Space( 8f );


			if( EditorGUI.EndChangeCheck() ) {
				E.Save();
				CustomProjectBrowser.s_styles.lineColor = E.i.lineColor;
				EditorApplication.RepaintProjectWindow();
			}

			GUILayout.Space( 8f );
		}


#if !ENABLE_HANANOKI_SETTINGS
#if UNITY_2018_3_OR_NEWER && !ENABLE_LEGACY_PREFERENCE
		[SettingsProvider]
		public static SettingsProvider PreferenceView() {
			var provider = new SettingsProvider( $"Preferences/Hananoki/{Package.name}", SettingsScope.User ) {
				label = $"{Package.name}",
				guiHandler = PreferencesGUI,
				titleBarGuiHandler = () => GUILayout.Label( $"{Package.version}", EditorStyles.miniLabel ),
			};
			return provider;
		}
		public static void PreferencesGUI( string searchText ) {
#else
		[PreferenceItem( Package.name )]
		public static void PreferencesGUI() {
#endif
			using( new LayoutScope() ) DrawGUI();
		}
#endif
	}



#if ENABLE_HANANOKI_SETTINGS
	[SettingsClass]
	public class SettingsEvent {
		[SettingsMethod]
		public static SettingsItem RegisterSettings() {
			return new SettingsItem() {
				//mode = 1,
				displayName = Package.name,
				version = Package.version,
				gui = SettingsEditorWindow.DrawGUI,
			};
		}
	}
#endif
}

