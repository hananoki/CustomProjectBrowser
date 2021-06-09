
using UnityEditor;

namespace HananokiEditor.CustomProjectBrowser {
  public static class Package {
    public const string reverseDomainName = "com.hananoki.custom-project-browser";
    public const string name = "CustomProjectBrowser";
    public const string nameNicify = "Custom Project Browser";
    public const string editorPrefName = "Hananoki.CustomProjectBrowser";
    public const string version = "0.6.0";
		[HananokiEditorMDViewerRegister]
		public static string MDViewerRegister() {
			return "68cd53ef6b462bc48a6292d6e84fdfa4";
		}
  }
}
