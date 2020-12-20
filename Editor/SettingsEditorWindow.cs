#define ENABLE_HANANOKI_SETTINGS

using HananokiEditor.Extensions;
using HananokiEditor.SharedModule;
using UnityEditor;
using UnityEngine;

using E = HananokiEditor.CustomProjectBrowser.SettingsEditor;
using ProjectBrowser = UnityReflection.UnityEditorProjectBrowser;
using SS = HananokiEditor.SharedModule.S;

namespace HananokiEditor.CustomProjectBrowser {

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

			bool _toolbarOverride;
			bool _guidNotify;
			float _barOffset;

			using( new EditorGUI.DisabledGroupScope( !E.i.Enable ) ) {
				E.i.showExtension = HEditorGUILayout.ToggleLeft( S._ShowExtension, E.i.showExtension );

				ScopeDisable.Begin( !E.i.showExtension );

				EditorGUI.indentLevel++;
				E.i.extBackColor = EditorGUILayout.ColorField( SS._BackColor, E.i.extBackColor );
				E.i.extTextColor = EditorGUILayout.ColorField( SS._TextColor, E.i.extTextColor );
				EditorGUI.indentLevel--;

				ScopeDisable.End();

				E.i.showLineColor = HEditorGUILayout.ToggleLeft( SS._Changecolorforeachrow, E.i.showLineColor );

				ScopeDisable.Begin( !E.i.showLineColor );

				EditorGUI.indentLevel++;
				E.i.lineColor = EditorGUILayout.ColorField( SS._Rowcolor, E.i.lineColor );
				EditorGUI.indentLevel--;

				ScopeDisable.End();

				GUILayout.Space( 8f );

				/////////////////////////

				// 実験的
				//EditorGUILayout.LabelField( $"* {SS._Experimental}", EditorStyles.boldLabel );
				HEditorGUILayout.HeaderTitle( $"* {SS._Experimental}" );
				E.i.IconClickContext = HEditorGUILayout.ToggleLeft( SS._ContextMenuWithIconClick, E.i.IconClickContext );
				if( UnitySymbol.Has( "UNITY_EDITOR_WIN" ) ) {
					using( new EditorGUI.DisabledGroupScope( !E.i.showExtension ) ) {
						E.i.enableExtensionRun = HEditorGUILayout.ToggleLeft( S._Clickontheextensiontorunitinthefiler, E.i.enableExtensionRun );
					}
				}
				else {
					E.i.enableExtensionRun = false;
				}

				_toolbarOverride = HEditorGUILayout.ToggleLeft( "Titlebar Override (UNITY_2019_3_OR_NEWER)", E.i.toolbarOverride );

				_barOffset = EditorGUILayout.Slider( "barOffset".nicify(), E.i.barOffset, 0, 500 );


				GUILayout.Space( 8f );



				/////////////////////////

				HEditorGUILayout.HeaderTitle( $"* Obsolete" );
				_guidNotify = HEditorGUILayout.ToggleLeft( "GUID (UNITY_2019_3_OR_NEWER)", E.i.guidNotify );
				E.i.adressableSupport = HEditorGUILayout.ToggleLeft( $"{S._EnablingAddressablesupport}", E.i.adressableSupport );

				GUILayout.Space( 8f );



				/////////////////////////

				HEditorGUILayout.HeaderTitle( $"* Debug" );

				E.i.debug = HEditorGUILayout.ToggleLeft( "Debug", E.i.debug );
				E.i.size = EditorGUILayout.FloatField( nameof( E.size ).nicify(), E.i.size );
			}
			EditorGUI.indentLevel--;


			GUILayout.Space( 8f );


			if( EditorGUI.EndChangeCheck() ) {
				CustomProjectBrowser._window = EditorWindowUtils.Find( UnityTypes.UnityEditor_ProjectBrowser );

				E.i.barOffset = _barOffset;

				var list = ProjectBrowser.GetAllProjectBrowsers();
				foreach( EditorWindow a in list ) {
					if( E.i.toolbarOverride != _toolbarOverride ) {
						E.i.toolbarOverride = _toolbarOverride;
						if( E.i.toolbarOverride ) {
							a.AddIMGUIContainer( CustomProjectBrowser._IMGUIContainerToolbar, true );
						}
						else {
							a.RemoveIMGUIContainer( CustomProjectBrowser._IMGUIContainerToolbar, true );
						}
					}
					if( E.i.guidNotify != _guidNotify ) {
						E.i.guidNotify = _guidNotify;
						if( E.i.guidNotify ) {
							a.AddIMGUIContainer( CustomProjectBrowser._IMGUIContainer, true );
						}
						else {
							a.RemoveIMGUIContainer( CustomProjectBrowser._IMGUIContainer, true );
						}
					}
				}

				E.Save();
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
	public class SettingsEvent {
		[HananokiSettingsRegister]
		public static SettingsItem RegisterSettings() {
			return new SettingsItem() {
				//mode = 1,
				displayName = Package.nameNicify,
				version = Package.version,
				gui = SettingsEditorWindow.DrawGUI,
			};
		}
	}
#endif
}

