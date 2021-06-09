using HananokiEditor.Extensions;
using HananokiEditor.SharedModule;
using HananokiRuntime;
using HananokiRuntime.Extensions;
using System;
using UnityEditor;
using UnityEngine;
using E = HananokiEditor.CustomProjectBrowser.SettingsEditor;
using SS = HananokiEditor.SharedModule.S;


namespace HananokiEditor.CustomProjectBrowser {

	[Serializable]
	public class SettingsEditor {

		[HananokiSettingsRegister]
		public static SettingsItem RegisterSettings() {
			return new SettingsItem() {
				//mode = 1,
				displayName = Package.nameNicify,
				version = Package.version,
				gui = DrawGUI,
			};
		}


		public bool Enable;

		#region Flags

		public int flag;
		const int SHOW_EXTENSION       = ( 1 << 0 );
		const int ICON_CLICK_CONTEXT   = ( 1 << 1 );
		const int ENABLE_EXTENSION_RUN = ( 1 << 2 );
		const int CUSTOM_DOCKPANE      = ( 1 << 3 );
		const int CUSTOM_TOOLBAR       = ( 1 << 4 );
		const int PROJECT_PATH_OPEN    = ( 1 << 5 );
		const int EXTERNAL_LINK        = ( 1 << 6 );


		public bool showExtension {
			get => flag.Has( SHOW_EXTENSION );
			set => flag.Toggle( SHOW_EXTENSION, value );
		}
		public bool iconClickContext {
			get => flag.Has( ICON_CLICK_CONTEXT );
			set => flag.Toggle( ICON_CLICK_CONTEXT, value );
		}
		public bool enableExtensionRun {
			get => flag.Has( ENABLE_EXTENSION_RUN );
			set => flag.Toggle( ENABLE_EXTENSION_RUN, value );
		}
		public bool customDockpane {
			get => flag.Has( CUSTOM_DOCKPANE );
			set => flag.Toggle( CUSTOM_DOCKPANE, value );
		}
		public bool customToolbar {
			get => flag.Has( CUSTOM_TOOLBAR );
			set => flag.Toggle( CUSTOM_TOOLBAR, value );
		}
		public bool projectPathOpen {
			get => flag.Has( PROJECT_PATH_OPEN );
			set => flag.Toggle( PROJECT_PATH_OPEN, value );
		}
		public bool externalLink {
			get => flag.Has( EXTERNAL_LINK );
			set => flag.Toggle( EXTERNAL_LINK, value );
		}

		#endregion


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



		/////////////////////////////////////////
		public static void Load() {
			if( i != null ) return;
			i = EditorPrefJson<E>.Get( Package.editorPrefName );
		}



		/////////////////////////////////////////
		public static void Save() {
			EditorPrefJson<E>.Set( Package.editorPrefName, i );
		}



		/////////////////////////////////////////
		public static void DrawGUI() {
			E.Load();
			ScopeChange.Begin();

			E.i.Enable = HEditorGUILayout.ToggleLeft( SS._Enable, E.i.Enable );
			EditorGUI.indentLevel++;
			GUILayout.Space( 8f );


			float _barOffset;


			ScopeDisable.Begin( !E.i.Enable );

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
			E.i.iconClickContext = HEditorGUILayout.ToggleLeft( SS._ContextMenuWithIconClick, E.i.iconClickContext );
			if( UnitySymbol.Has( "UNITY_EDITOR_WIN" ) ) {
				using( new EditorGUI.DisabledGroupScope( !E.i.showExtension ) ) {
					E.i.enableExtensionRun = HEditorGUILayout.ToggleLeft( S._Clickontheextensiontorunitinthefiler, E.i.enableExtensionRun );
				}
			}
			else {
				E.i.enableExtensionRun = false;
			}

			var _customDockpane  = HEditorGUILayout.ToggleLeft( "DockPane (UNITY_2019_1_OR_NEWER)", E.i.customDockpane );
			var _customToolbar   = HEditorGUILayout.ToggleLeft( "Toolbar (UNITY_2019_1_OR_NEWER)", E.i.customToolbar );
			var _projectPathOpen = HEditorGUILayout.ToggleLeft( "Project Path Open Button", E.i.projectPathOpen );
			var _externalLink    = HEditorGUILayout.ToggleLeft( "External Link Test", E.i.externalLink );
			

			GUILayout.Space( 8f );


			/////////////////////////
			//HEditorGUILayout.HeaderTitle( $"* Obsolete" );
			//_guidNotify = HEditorGUILayout.ToggleLeft( "GUID (UNITY_2019_3_OR_NEWER)", E.i.guidNotify );

			//GUILayout.Space( 8f );



			/////////////////////////
			HEditorGUILayout.HeaderTitle( $"* Debug" );

			E.i.debug = HEditorGUILayout.ToggleLeft( "Icon-ClickArea DrawRect", E.i.debug );
			E.i.size = EditorGUILayout.FloatField( nameof( E.size ).nicify(), E.i.size );

			ScopeDisable.End();
			EditorGUI.indentLevel--;


			GUILayout.Space( 8f );


			if( ScopeChange.End() ) {
				
				E.i.projectPathOpen = _projectPathOpen;
				E.i.externalLink = _externalLink;
#if UNITY_2019_1_OR_NEWER
				if( E.i.customDockpane != _customDockpane ) {
					E.i.customDockpane = _customDockpane;
					if( E.i.customDockpane ) {
						Utils.AttachDockPane();
					}
					else {
						Utils.DetachDockPane();
					}
				}
				if( E.i.customToolbar != _customToolbar ) {
					E.i.customToolbar = _customToolbar;
					if( E.i.customToolbar ) {
						Utils.AttachToolbar();
					}
					else {
						Utils.DetachToolbar();
					}
				}
#endif

				E.Save();
				EditorApplication.RepaintProjectWindow();
			}

			GUILayout.Space( 8f );
		}
	}
}

