using HananokiEditor.Extensions;
using HananokiEditor.SharedModule;
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
		const int SHOW_EXTENSION = ( 1 << 0 );
		const int ICON_CLICK_CONTEXT = ( 1 << 1 );
		const int ENABLE_EXTENSION_RUN = ( 1 << 2 );
		const int CUSTOM_DOCKPANE = ( 1 << 3 );

		const int PROJECT_PATH_OPEN = ( 1 << 5 );
		const int EXTERNAL_LINK = ( 1 << 6 );
		const int FOCUSED_INSPECTORS_BUTTON = ( 1 << 7 );
		const int NOTIFY_PREFAB_PARENT = ( 1 << 8 );
		const int SHOW_ASSET_TYPE = ( 1 << 9 );


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
		public bool projectPathOpen {
			get => flag.Has( PROJECT_PATH_OPEN );
			set => flag.Toggle( PROJECT_PATH_OPEN, value );
		}
		public bool externalLink {
			get => flag.Has( EXTERNAL_LINK );
			set => flag.Toggle( EXTERNAL_LINK, value );
		}
		public bool focusedInspectorsButton {
			get => flag.Has( FOCUSED_INSPECTORS_BUTTON );
			set => flag.Toggle( FOCUSED_INSPECTORS_BUTTON, value );
		}
		public bool notifyPrefabParent {
			get => flag.Has( NOTIFY_PREFAB_PARENT );
			set => flag.Toggle( NOTIFY_PREFAB_PARENT, value );
		}
		public bool showAssetType {
			get => flag.Has( SHOW_ASSET_TYPE );
			set => flag.Toggle( SHOW_ASSET_TYPE, value );
		}

		#endregion


		public float barOffset = 50;
		public bool debug;
		public float size = 16;

		public bool showLineColor = true;
		public Color lineColorPersonal = new Color( 0, 0, 0, 0.05f );
		public Color lineColorProfessional = new Color( 1, 1, 1, 0.05f );

		public static E i;


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
			Load();
			ScopeChange.Begin();

			i.Enable = HEditorGUILayout.ToggleLeft( SS._Enable, i.Enable );
			EditorGUI.indentLevel++;
			GUILayout.Space( 8f );


			//float _barOffset;


			ScopeDisable.Begin( !i.Enable );

			i.showExtension = HEditorGUILayout.ToggleLeft( S._ShowExtension, i.showExtension );
			if( i.showExtension ) {
				i.showAssetType = false;
			}
			i.showAssetType = HEditorGUILayout.ToggleLeft( "Show Asset Type", i.showAssetType );
			if( i.showAssetType ) {
				i.showExtension = false;
			}

			ScopeDisable.Begin( !i.showExtension );

#if false // ShaerdModuleのversionカラーに変更してた
			EditorGUI.indentLevel++;
			E.i.extBackColor = EditorGUILayout.ColorField( SS._BackColor, E.i.extBackColor );
			E.i.extTextColor = EditorGUILayout.ColorField( SS._TextColor, E.i.extTextColor );
			EditorGUI.indentLevel--;
#endif

			ScopeDisable.End();

			i.showLineColor = HEditorGUILayout.ToggleLeft( SS._Changecolorforeachrow, i.showLineColor );

			ScopeDisable.Begin( !i.showLineColor );

			EditorGUI.indentLevel++;
			i.lineColor = EditorGUILayout.ColorField( SS._Rowcolor, i.lineColor );
			EditorGUI.indentLevel--;

			ScopeDisable.End();

			GUILayout.Space( 8f );

			/////////////////////////

			// 実験的
			//EditorGUILayout.LabelField( $"* {SS._Experimental}", EditorStyles.boldLabel );
			HEditorGUILayout.HeaderTitle( $"* {SS._Experimental}" );
			i.iconClickContext = HEditorGUILayout.ToggleLeft( SS._ContextMenuWithIconClick, i.iconClickContext );
			if( UnitySymbol.Has( "UNITY_EDITOR_WIN" ) ) {
				using( new EditorGUI.DisabledGroupScope( !( i.showExtension | i.showAssetType ) ) ) {
					i.enableExtensionRun = HEditorGUILayout.ToggleLeft( S._Clickontheextensiontorunitinthefiler, i.enableExtensionRun );
				}
			}
			else {
				i.enableExtensionRun = false;
			}

			var _customDockpane = HEditorGUILayout.ToggleLeft( "DockPane (UNITY_2019_1_OR_NEWER)", i.customDockpane );

			var _projectPathOpen = HEditorGUILayout.ToggleLeft( "Project Path Open Button", i.projectPathOpen );
			var _externalLink = HEditorGUILayout.ToggleLeft( "External Link Test", i.externalLink );
			var _focusedInspectorsButton = HEditorGUILayout.ToggleLeft( "Focused Inspectors Button", i.focusedInspectorsButton );
			var _notifyPrefabParent = HEditorGUILayout.ToggleLeft( "Notify Prefab Parent", i.notifyPrefabParent );


			GUILayout.Space( 8f );


			/////////////////////////
			//HEditorGUILayout.HeaderTitle( $"* {SS._Deprecated}" );
			//var _customToolbar = HEditorGUILayout.ToggleLeft( "Toolbar (UNITY_2019_1_OR_NEWER)", i.customToolbar );
			//_guidNotify = HEditorGUILayout.ToggleLeft( "GUID (UNITY_2019_3_OR_NEWER)", E.i.guidNotify );

			GUILayout.Space( 8f );



			/////////////////////////
			HEditorGUILayout.HeaderTitle( $"* Debug" );

			i.debug = HEditorGUILayout.ToggleLeft( "Icon-ClickArea DrawRect", i.debug );
			i.size = EditorGUILayout.FloatField( nameof( size ).nicify(), i.size );

			ScopeDisable.End();
			EditorGUI.indentLevel--;


			GUILayout.Space( 8f );


			if( ScopeChange.End() ) {

				i.projectPathOpen = _projectPathOpen;
				i.externalLink = _externalLink;
				i.focusedInspectorsButton = _focusedInspectorsButton;
				i.notifyPrefabParent = _notifyPrefabParent;
#if UNITY_2019_1_OR_NEWER
				if( i.customDockpane != _customDockpane ) {
					i.customDockpane = _customDockpane;
					if( i.customDockpane ) {
						Utils.AttachDockPane();
					}
					else {
						Utils.DetachDockPane();
					}
				}
#endif

				Save();
				EditorApplication.RepaintProjectWindow();
			}

			GUILayout.Space( 8f );
		}
	}
}

