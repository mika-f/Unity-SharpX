// ------------------------------------------------------------------------------------------
//  Copyright (c) Natsuneko. All rights reserved.
//  Licensed under the MIT License. See LICENSE in the project root for license information.
// ------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using NatsunekoLaboratory.CsprojHooks.Features.Abstractions;

using UnityEditor;

using UnityEditorInternal;

using UnityEngine;

using Random = System.Random;

namespace NatsunekoLaboratory.SharpX
{
    public class CustomCsprojHook : ICsprojHooksSlnFeature, ICsprojHooksFeature, ICsprojHooksConfigurableFeature
    {
        public const string GlobalId = "NatsunekoLaboratory.SharpX.CustomCsprojHook";

        private const string Net6CSharpLibraryGuid = "{9A19103F-16F7-4668-BE54-9A1E7A4F7556}";
        private SharpXConfiguration _configuration;
        private ReorderableList _reorderableReferenceAssemblyList;
        private ReorderableList _reorderableReferenceProjectList;
        private ReorderableList _reorderableWorkspaceList;
        private Action _saveCallback;

        public Type T => typeof(SharpXConfiguration);

        public string Id => GlobalId;

        public void Initialize(object obj, Action save)
        {
            var config = obj as SharpXConfiguration;
            if (config == null)
                return;

            _configuration = config;
            _saveCallback = save;

            var workspaces = new List<string>(config.Workspaces ?? new List<string>());
            _reorderableWorkspaceList = new ReorderableList(workspaces, typeof(string), true, false, true, true)
            {
                drawNoneElementCallback = rect => EditorGUI.LabelField(rect, "SharpX workspaces are not registered"),
                drawElementCallback = (rect, index, active, focus) =>
                {
                    using (var scope = new EditorGUI.ChangeCheckScope())
                    {
                        var item = workspaces[index];

                        const int controlGap = 4;
                        const int buttonWidth = 32;

                        rect.width -= buttonWidth + controlGap;

                        EditorGUI.LabelField(rect, item);

                        rect.x += rect.width + controlGap;
                        rect.width = buttonWidth;

                        if (GUI.Button(rect, "..."))
                        {
                            var path = EditorUtility.OpenFolderPanel("Add Additional Workspace", Path.GetDirectoryName(Application.dataPath), "");
                            if (string.IsNullOrEmpty(path))
                                return;
                            if (!path.EndsWith("~"))
                            {
                                EditorUtility.DisplayDialog("Error", "SharpX Workspace must be ends with ~, because Unity must be exclude compile shaders", "Ok");
                                return;
                            }

                            workspaces[index] = path.Replace($"{Application.dataPath}/", "");
                            GUI.changed = true;
                        }

                        if (scope.changed)
                        {
                            config.Workspaces = workspaces;
                            save();
                        }
                    }
                },
                onChangedCallback = _ =>
                {
                    config.Workspaces = workspaces;
                    save();
                }
            };

            var assemblies = new List<string>(config.ReferenceAssemblies ?? new List<string>());
            _reorderableReferenceAssemblyList = new ReorderableList(assemblies, typeof(string), true, false, true, true)
            {
                drawNoneElementCallback = rect => EditorGUI.LabelField(rect, "SharpX reference assemblies (primitives) are not registered"),
                drawElementCallback = (rect, index, active, focus) =>
                {
                    using (var scope = new EditorGUI.ChangeCheckScope())
                    {
                        var item = assemblies[index];

                        const int controlGap = 4;
                        const int buttonWidth = 32;

                        rect.width -= buttonWidth + controlGap;

                        EditorGUI.LabelField(rect, item);

                        rect.x += rect.width + controlGap;
                        rect.width = buttonWidth;

                        if (GUI.Button(rect, "..."))
                        {
                            var path = EditorUtility.OpenFilePanelWithFilters("Add Additional Assembly Reference", Path.GetDirectoryName(Application.dataPath), new[] { "Dynamic Link Library (*.dll)", "dll" });
                            if (string.IsNullOrEmpty(path))
                                return;

                            assemblies[index] = path.Replace($"{Application.dataPath}/", "");
                            GUI.changed = true;
                        }

                        if (scope.changed)
                        {
                            config.ReferenceAssemblies = assemblies;
                            save();
                        }
                    }
                },
                onChangedCallback = _ =>
                {
                    config.ReferenceAssemblies = assemblies;
                    save();
                }
            };


            var projects = new List<string>(config.ReferenceProjects ?? new List<string>());
            _reorderableReferenceProjectList = new ReorderableList(projects, typeof(string), true, false, true, true)
            {
                drawNoneElementCallback = rect => EditorGUI.LabelField(rect, "SharpX reference projects are not registered"),
                drawElementCallback = (rect, index, active, focus) =>
                {
                    using (var scope = new EditorGUI.ChangeCheckScope())
                    {
                        var item = projects[index];

                        const int controlGap = 4;
                        const int buttonWidth = 32;

                        rect.width -= buttonWidth + controlGap;

                        EditorGUI.LabelField(rect, item);

                        rect.x += rect.width + controlGap;
                        rect.width = buttonWidth;

                        if (GUI.Button(rect, "..."))
                        {
                            var path = EditorUtility.OpenFilePanelWithFilters("Add Additional Assembly Reference", Path.GetDirectoryName(Application.dataPath), new[] { "C# project file (*.csproj)", "csproj" });
                            if (string.IsNullOrEmpty(path))
                                return;

                            var root = Path.GetFullPath(Path.Combine(Application.dataPath, "..")).Replace("\\", "/");
                            projects[index] = path.Replace($"{root}/", "");
                            GUI.changed = true;
                        }

                        if (scope.changed)
                        {
                            config.ReferenceProjects = projects;
                            save();
                        }
                    }
                },
                onChangedCallback = _ =>
                {
                    config.ReferenceProjects = projects;
                    save();
                }
            };
        }

        public void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("SharpX Custom Compilation");
                _reorderableWorkspaceList.DoLayoutList();

                EditorGUILayout.Space(5);

                EditorGUILayout.LabelField("SharpX External Reference Assemblies");
                _reorderableReferenceAssemblyList.DoLayoutList();

                EditorGUILayout.Space(5);

                EditorGUILayout.LabelField("SharpX External Reference Projects");
                _reorderableReferenceProjectList.DoLayoutList();

                EditorGUILayout.Space(5);

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (var scope = new EditorGUI.ChangeCheckScope())
                    {
                        _configuration.Location = EditorGUILayout.TextField("Assembly Location", _configuration.Location);
                        if (GUILayout.Button("...", GUILayout.Width(32)))
                        {
                            var path = EditorUtility.OpenFolderPanel("Select SharpX Assembly Location", Path.GetDirectoryName(Application.dataPath), "");
                            if (string.IsNullOrEmpty(path))
                                return;

                            _configuration.Location = path.Replace($"{Application.dataPath}/", "");
                            GUI.changed = true;
                        }

                        if (scope.changed)
                            _saveCallback();
                    }
                }
            }
        }

        public string OnGeneratedCSProject(string path, string content)
        {
            return content;
        }

        public string OnGeneratedSlnSolution(string path, string content)
        {
            if (_configuration == null)
                return content;

            if (_configuration.Workspaces.Any())
            {
                var @namespace = (XNamespace)"http://schemas.microsoft.com/developer/msbuild/2003";

                foreach (var workspace in _configuration.Workspaces)
                {
                    // https://docs.microsoft.com/ja-jp/dotnet/core/project-sdk/overview
                    // https://docs.microsoft.com/ja-jp/visualstudio/extensibility/internals/solution-dot-sln-file
                    var assemblyName = workspace.Replace("/", ".").Replace("\\", ".");
                    var projectAttribute = new XAttribute("Sdk", "Microsoft.NET.Sdk");
                    var project = new XElement(@namespace + "Project", projectAttribute);
                    var propertyGroup = new XElement(
                        @namespace + "PropertyGroup",
                        new XElement(@namespace + "TargetFramework", "net6.0"),
                        new XElement(@namespace + "ImplicitUsings", "enable"),
                        new XElement(@namespace + "Nullable", "enable"),
                        new XElement(@namespace + "DefaultItemExcludes", "$(DefaultItemExcludes);**/*.meta"),
                        new XElement(@namespace + "EnableDefaultCompileItems", "false"),
                        new XElement(@namespace + "EnableDefaultEmbeddedResourceItems", "false"),
                        new XElement(@namespace + "EnableDefaultNoneItems", "false")
                    );
                    var itemGroup = new XElement(@namespace + "ItemGroup");

                    foreach (var assembly in _configuration.ReferenceAssemblies)
                    {
                        var include = new XAttribute("Include", Path.GetFileNameWithoutExtension(assembly));
                        var hint = new XElement(@namespace + "HintPath", Path.Combine("Assets", assembly));
                        itemGroup.Add(new XElement(@namespace + "Reference", include, hint));
                    }

                    foreach (var reference in _configuration.ReferenceProjects)
                    {
                        var include = new XAttribute("Include", reference);
                        itemGroup.Add(new XElement(@namespace + "ProjectReference", include));
                    }

                    itemGroup.Add(new XElement(@namespace + "Compile", new XAttribute("Include", $"Assets/{workspace}/**/*.cs")));

                    project.Add(propertyGroup);
                    project.Add(itemGroup);

                    var document = new XDocument(project);

                    using (var sw = new StreamWriter($"{assemblyName}.csproj"))
                        sw.WriteLine(document.ToString());

                    var sb = new StringBuilder();
                    sb.AppendLine($"Project(\"{Net6CSharpLibraryGuid}\") = \"{assemblyName}\", \"{assemblyName}.csproj\", \"{{{FromSeed(assemblyName)}}}\"");
                    sb.AppendLine("EndProject");

                    content += sb.ToString();
                }
            }

            return content;
        }

        private static Guid FromSeed(string seed)
        {
            var rnd = new Random(seed.GetHashCode());
            var guid = new byte[16];
            rnd.NextBytes(guid);

            return new Guid(guid);
        }
    }
}