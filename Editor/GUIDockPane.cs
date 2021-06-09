using HananokiEditor.Extensions;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityReflection;
using UnityEditor.ProjectWindowCallback;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#endif
using E = HananokiEditor.CustomProjectBrowser.SettingsEditor;


namespace HananokiEditor.CustomProjectBrowser {
	public class GUIDockPane {

		static EditorWindow projectBrowser => UnityEditorProjectWindowUtil.GetProjectBrowserIfExists() as EditorWindow;

		static EditorWindow attachedProjectBrowser;

		static string[] s_folderName = ( new string[] {
			"Animations",
			"Audio",
			"Editor",
			"Fonts",
			"Materials",
			"Prefabs",
			"Resources",
			"Scenes",
			"Scripts",
			"Shaders",
			"Textures",
			//"Tests",
			} ).OrderBy( x => x ).ToArray();

		internal static object s_IMGUIContainer;


		/////////////////////////////////////////
		public static bool Attach() {
#if UNITY_2019_1_OR_NEWER
			if( attachedProjectBrowser !=null){
				if( attachedProjectBrowser.rootVisualElement.parent.childCount != 3 ) {
					s_IMGUIContainer = null;
				}
			}
#endif
			if( s_IMGUIContainer != null ) return true;
			if( projectBrowser == null ) return false;

			s_IMGUIContainer = Activator.CreateInstance( UnityTypes.UnityEngine_UIElements_IMGUIContainer, new object[] { (Action) OnGUIHandler } );
#if UNITY_2019_1_OR_NEWER
			IMGUIContainer con = (IMGUIContainer) s_IMGUIContainer;
			con.style.height = 19;
			con.style.marginRight = 42;
			con.style.width = 160 + 8;
			con.style.alignSelf = Align.FlexEnd;
#endif
			projectBrowser.AddIMGUIContainer( s_IMGUIContainer, true );
			attachedProjectBrowser = projectBrowser;
			return true;
		}


		/////////////////////////////////////////
		public static void Dettach() {
			projectBrowser?.RemoveIMGUIContainer( s_IMGUIContainer, true );
			s_IMGUIContainer = null;
		}



		/////////////////////////////////////////
		static void OnGUIHandler() {
			GUILayout.BeginArea( new Rect( 0, 0, projectBrowser.position.width, 20 ) );

			ScopeHorizontal.Begin();

			if( HEditorGUILayout.IconButton( EditorIcon.folder, "Folder" ) ) {
				var m = new GenericMenu();
				m.AddItem( "New Folder", () => {

					var t = EditorHelper.GetTypeFromString( "DoCreateFolder",
						インナークラスの検索を有効にする: true );
					var dirpath = AssetDatabase.GenerateUniqueAssetPath( $"{ProjectBrowserUtils.activeFolderPath}/New Folder" );
					ProjectWindowUtil.StartNameEditingIfProjectWindowExists( 0, (EndNameEditAction) ScriptableObject.CreateInstance( t ), dirpath, EditorIcon.folderEmpty, null );
				} );
				m.AddSeparator( "" );

				foreach( var p in s_folderName ) m.AddItem( p, () => ProjectBrowserUtils.CreateFolder( p ) );

				m.DropDownPopupRect( HEditorGUI.lastRect );
				//ProjectWindowUtil.StartNameEditingIfProjectWindowExists( 0, ScriptableObject.CreateInstance<DoCreateFolder>(), "New Folder", EditorGUIUtility.IconContent( EditorResources.folderIconName ).image as Texture2D, null );
				//var a = R.Type( "UnityEditor.Experimental.EditorResources" );
				////Debug.Log( a.GetProperty( "folderIconName" ).GetValue(null) );
			}
			if( HEditorGUILayout.IconButton( EditorIcon.icons_processed_cs_script_icon_asset, "C# Script" ) ) {
				EditorApplication.ExecuteMenuItem( "Assets/Create/C# Script" );
			}
			if( ExternalPackages.ScriptableObjectManager.enabled ) {
				if( HEditorGUILayout.IconButton( EditorIcon.scriptableobject ) ) {
					ExternalPackages.ScriptableObjectManager.ShowCreateMenu();
				}
			}
			if( HEditorGUILayout.IconButton( EditorIcon.icons_processed_unityengine_material_icon_asset, "Material" ) ) {
				EditorApplication.ExecuteMenuItem( "Assets/Create/Material" );
			}
			if( HEditorGUILayout.IconButton( EditorIcon.icons_processed_unityeditor_animations_animatorcontroller_icon_asset, "Animator Controller" ) ) {
				EditorApplication.ExecuteMenuItem( "Assets/Create/Animator Controller" );
			}
			if( HEditorGUILayout.IconButton( EditorIcon.icons_processed_unityengine_animationclip_icon_asset, "Animation" ) ) {
				EditorApplication.ExecuteMenuItem( "Assets/Create/Animation" );
			}
			if( HEditorGUILayout.IconButton( EditorIcon.icons_processed_unityeditorinternal_assemblydefinitionasset_icon_asset, "Assembly Definition" ) ) {
				EditorApplication.ExecuteMenuItem( "Assets/Create/Assembly Definition" );
			}
			if( HEditorGUILayout.IconButton( EditorIcon.icons_processed_unityengine_u2d_spriteatlas_icon_asset, "Sprite Atlas" ) ) {
				EditorApplication.ExecuteMenuItem( "Assets/Create/Sprite Atlas" );
			}
			if( HEditorGUILayout.IconButton( EditorIcon.icons_processed_unityengine_shadervariantcollection_icon_asset, "Shader Variant Collection" ) ) {
				EditorApplication.ExecuteMenuItem( "Assets/Create/Shader/Shader Variant Collection" );
			}
			if( HEditorGUILayout.IconButton( EditorIcon.icons_processed_unityengine_rendertexture_icon_asset, "Render Texture" ) ) {
				EditorApplication.ExecuteMenuItem( "Assets/Create/Render Texture" );
			}

#if UNITY_2019_3_OR_NEWER
			if( UnityProject.VFX ) {
				if( HEditorGUILayout.IconButton( EditorIcon.icons_processed_unityengine_vfx_visualeffect_icon_asset, "VFX" ) ) {
					var menu = Unsupported.GetSubmenus( "Assets" ).Where( x => x.Contains( "Assets/Create/Visual Effects" ) );
					var menuName = menu.Select( x => (x, x.Replace( "Assets/Create/Visual Effects/", "" )) );
					var m = new GenericMenu();
					//m.AddItem( "New Folder", () => EditorApplication.ExecuteMenuItem( "Assets/Create/Folder" ) );
					//m.AddSeparator( "" );

					foreach( var p in menuName ) {
						m.AddItem( p.Item2, ( context ) => EditorApplication.ExecuteMenuItem( (string) context ), p.x );
					}

					m.DropDownPopupRect( HEditorGUI.lastRect );
				}
			}
#endif
			GUILayout.Space( E.i.barOffset );
			ScopeHorizontal.End();
			GUILayout.EndArea();
		}
	}
}
