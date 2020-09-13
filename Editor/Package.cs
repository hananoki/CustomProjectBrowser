
using UnityEditor;

namespace Hananoki.CustomProjectBrowser {
  public static class Package {
    public const string name = "CustomProjectBrowser";
    public const string editorPrefName = "Hananoki.CustomProjectBrowser";
    public const string version = "0.5.10-preview";
  }
  
#if UNITY_EDITOR
  [EditorLocalizeClass]
  public class LocalizeEvent {
    [EditorLocalizeMethod]
    public static void Changed() {
      foreach( var filename in DirectoryUtils.GetFiles( AssetDatabase.GUIDToAssetPath( "6488b518303641d44811f8850e0540e8" ), "*.csv" ) ) {
        if( filename.Contains( EditorLocalize.GetLocalizeName() ) ) {
          EditorLocalize.Load( Package.name, AssetDatabase.AssetPathToGUID( filename ), "a1a169733f3f26b4da616b36befd7b13" );
        }
      }
    }
  }
#endif
}
