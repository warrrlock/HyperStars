using System;
using System.IO;
using System.Text;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Tinylytics {

	//The idea here is that at buildtime, it creates a script which bakes in the time, which can then be referenced freely.
	//https://forum.unity.com/threads/build-date-or-version-from-code.59134/#post-4088761

	public class BuildInfoProcessor : IPreprocessBuildWithReport {
		public int callbackOrder { get { return 0; } }
		public void OnPreprocessBuild(BuildReport report) {
			StringBuilder sb = new StringBuilder();
			sb.Append("namespace Tinylytics { public static class BuildInfo");
			sb.Append("{");
			sb.Append("public static string BUILD_TIME = \"");
			sb.Append(DateTime.Now.ToString());
			sb.Append("\";");
			sb.Append("}}");
			//this could also contain other build info from report.summary
			//https://docs.unity3d.com/ScriptReference/Build.Reporting.BuildSummary.html
			//.platform, .totalSize, 
			using (System.IO.StreamWriter file =
				new System.IO.StreamWriter(@"Assets/Tinylytics_AnalyticsTool/Scripts/BuildInfo.cs")) {
				file.WriteLine(sb.ToString());
			}
		}
	}
}