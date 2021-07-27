using System.Diagnostics;
using UnityEditor;


namespace HananokiEditor.CustomProjectBrowser {
	
	public class Utils {
	
		public static bool drawSupress {
			set {
				CustomProjectBrowser.s_supress = value;
			}
		}

	}

}