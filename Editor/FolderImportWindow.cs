using Hananoki.Extensions;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

using UnityEditor.Presets;

namespace Hananoki {

	public class FolderImportWindow : PopupWindowContent {
		//[MenuItem( "Tools/FolderImport" )]
		//public static void Open() {
		//	var window = GetWindow<FolderImportWindow>();
		//	window.SetTitle( new GUIContent( "FolderImport" ) );
		//}

		//string text;
		Vector2 windowSize;
		Preset m_preset;
		//string[] dirPopup;

		static public DefaultAsset s_folder;
		static TextureImporterType m_textureType = TextureImporterType.Default;

		static bool m_alphaIsTransparency;

		static int m_spriteMode = 0;
		int spriteMode => m_spriteMode + 1;

		static SpriteMeshType m_meshType = SpriteMeshType.Tight;
		static float m_pixelsPerUnit = 100;
		static bool m_generatePhysicsShape = true;

		static bool m_9slice = false;
		static Vector4 m_9sliceV;

		public override Vector2 GetWindowSize() => windowSize;

		public static Vector2 size = new Vector2( 350, 160 );
		Vector2 m_scroll;

		string[] spriteModeNames = { "Single", "Multiple", "Polygon" };

		public FolderImportWindow() {
			windowSize = size;
		}


		public override void OnGUI( Rect rect ) {
			if( s_folder != null ) {
				GUILayout.Label( AssetDatabase.GetAssetPath( s_folder ) );
			}
			else {
				GUILayout.Label( "None" );
			}
			using( var sc = new GUILayout.ScrollViewScope( m_scroll ) ) {
				m_scroll = sc.scrollPosition;
				m_preset = (Preset) EditorGUILayout.ObjectField( "Preset", m_preset, typeof( Preset ), false );
				using( new EditorGUI.DisabledGroupScope( m_preset != null ) ) {
					m_textureType = (TextureImporterType) EditorGUILayout.EnumPopup( nameof( m_textureType ).nicify(), m_textureType );

					GUILayout.Space( 4 );
					if( m_textureType == TextureImporterType.Default ) {
						m_alphaIsTransparency = EditorGUILayout.Toggle( nameof( m_alphaIsTransparency ).nicify(), m_alphaIsTransparency );
					}
					if( m_textureType == TextureImporterType.Sprite ) {
						m_spriteMode = EditorGUILayout.Popup( nameof( m_spriteMode ).nicify(), m_spriteMode, spriteModeNames );
						EditorGUI.indentLevel++;
						m_meshType = (SpriteMeshType) EditorGUILayout.EnumPopup( nameof( m_meshType ).nicify(), m_meshType );
						m_pixelsPerUnit = EditorGUILayout.FloatField( nameof( m_pixelsPerUnit ).nicify(), m_pixelsPerUnit );
						m_generatePhysicsShape = EditorGUILayout.Toggle( nameof( m_generatePhysicsShape ).nicify(), m_generatePhysicsShape );


						EditorGUI.indentLevel--;

						//HGUIScope.Vertical( EditorStyles.helpBox, () => {
						m_9slice = EditorGUILayout.ToggleLeft( nameof( m_9slice ).nicify(), m_9slice );
						if( m_9slice ) {
							m_9sliceV = EditorGUILayout.Vector4Field( "Border", m_9sliceV );
						}
						//} );

					}
				}
			}

			//TextureImporterType.Default
			GUILayout.FlexibleSpace();
			HGUIScope.Horizontal();
			GUILayout.FlexibleSpace();
			if( GUILayout.Button( "Apply" ) ) {
				ImporterAction( AssetDatabase.GetAssetPath( s_folder ), importer => {
					bool changed = false;

					if( m_preset != null ) {
						m_preset.ApplyTo( importer );
						importer.SaveAndReimport();
					}
					else {
						if( importer.textureType != m_textureType ) {
							importer.textureType = m_textureType;
							changed = true;
						}

						if( importer.textureType == TextureImporterType.Default ) {

							if( importer.alphaIsTransparency != m_alphaIsTransparency ) {
								importer.alphaIsTransparency = m_alphaIsTransparency;
								changed = true;
							}
						}
						if( importer.textureType == TextureImporterType.Sprite ) {
							var tis = new TextureImporterSettings();
							importer.ReadTextureSettings( tis );


							if( tis.spriteMode != spriteMode ) {
								tis.spriteMode = spriteMode;
								changed = true;
							}
							if( tis.spriteMeshType != m_meshType ) {
								tis.spriteMeshType = m_meshType;
								changed = true;
							}
							if( tis.spritePixelsPerUnit != m_pixelsPerUnit ) {
								tis.spritePixelsPerUnit = m_pixelsPerUnit;
								changed = true;
							}
							if( tis.spriteGenerateFallbackPhysicsShape != m_generatePhysicsShape ) {
								tis.spriteGenerateFallbackPhysicsShape = m_generatePhysicsShape;
								changed = true;
							}

							if( m_9slice ) {
								if( tis.spriteBorder != m_9sliceV ) {
									tis.spriteBorder = m_9sliceV;
									changed = true;
								}
							}

							importer.SetTextureSettings( tis );
						}
						if( changed ) {
							importer.SaveAndReimport();
						}
					}
				} );
			}
			HGUIScope.End();
		}


		void ImporterAction( string path, Action<TextureImporter> action ) {
			//var path = GUIDUtils.GetAssetPath( (string) context );
			var files = DirectoryUtils.GetFiles( path, "*", SearchOption.AllDirectories ).Where( x => x.GetExtension() != ".meta" ).ToArray();
			using( new AssetEditingScope() ) {
				foreach( var p in files ) {
					var importer = AssetImporter.GetAtPath( p ) as TextureImporter;
					if( importer == null ) continue;

					action.Invoke( importer );
				}
			}
		}
	}
}
