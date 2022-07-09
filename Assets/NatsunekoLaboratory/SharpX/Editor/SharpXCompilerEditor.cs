using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using NatsunekoLaboratory.CsprojHooks.Features.Abstractions;

using UnityEditor;

using UnityEditorInternal;

using UnityEngine;

using Debug = UnityEngine.Debug;

namespace NatsunekoLaboratory.SharpX
{
    internal class SharpXCompilerEditor : EditorWindow
    {
        private static object _dictionary;
        private ReorderableList _reorderable;

        [InitializeOnLoadMethod]
        public static void InitializeOnLoad()
        {
            var t = typeof(ICsprojHooksFeature).Assembly.GetType("NatsunekoLaboratory.CsprojHooks.CsprojHooksSettingsStore");
            var instance = t.InvokeMember("Instance", BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Static, null, null, null);
            _dictionary = t.InvokeMember("Store", BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, instance, null);
        }

        [MenuItem("Window/Natsuneko Laboratory/SharpX")]
        public static void ShowWindow()
        {
            var window = GetWindow<SharpXCompilerEditor>("SharpX");
            window.Show();
        }

        private void OnGUI()
        {
            if (_dictionary != null)
            {
                var items = JsonUtility.FromJson<SharpXConfiguration>(GetItems() ?? "");
                _reorderable ??= new ReorderableList(items.Workspaces, typeof(string), false, false, false, false);

                EditorGUILayout.LabelField("SharpX Workspaces");
                using (new EditorGUI.DisabledScope(true))
                    _reorderable.DoLayoutList();

                if (GUILayout.Button("Compile All Items"))
                {
                    var compiler = Path.GetFullPath(AssetDatabase.GUIDToAssetPath("27a42347b4e64cc4b8112fc39fac9d23"));

                    foreach (var (w, i) in items.Workspaces.Select((w, i) => (w, i)))
                        try
                        {
                            EditorUtility.DisplayProgressBar("Compiling SharpX C# items...", $"Compiling {w}...", i / (float)items.Workspaces.Count);

                            var psi = new ProcessStartInfo("dotnet", compiler)
                            {
                                Arguments = $"{compiler}",
                                CreateNoWindow = true,
                                RedirectStandardError = true,
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                                WorkingDirectory = Path.GetFullPath(Path.Combine("Assets", w))
                            };

                            var process = new Process
                            {
                                EnableRaisingEvents = true,
                                StartInfo = psi
                            };

                            process.Start();
                            process.OutputDataReceived += (sender, args) =>
                            {
                                if (!string.IsNullOrEmpty(args.Data))
                                    Debug.Log(args.Data);
                            };
                            process.ErrorDataReceived += (sender, args) =>
                            {
                                if (!string.IsNullOrEmpty(args.Data))
                                    Debug.Log(args.Data);
                            };
                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();
                            process.WaitForExit();
                        }
                        catch
                        {
                            // ignored
                        }

                    AssetDatabase.Refresh();
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        private static string GetItems()
        {
            if (_dictionary == null)
                return null;

            var indexer = new[] { typeof(string) };
            var t = _dictionary.GetType();
            var items = t.GetProperty(t.GetCustomAttribute<DefaultMemberAttribute>().MemberName, indexer);
            if (items == null)
                return null;

            return items.GetMethod.Invoke(_dictionary, new object[] { CustomCsprojHook.GlobalId }) as string;
        }
    }
}
