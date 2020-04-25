// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using XRTK.Attributes;
using XRTK.Definitions.Utilities;
using XRTK.Inspectors.Extensions;
using XRTK.Inspectors.Utilities.SymbolicLinks;
using XRTK.Utilities.Editor;

namespace XRTK.Inspectors
{
    [CreateAssetMenu(menuName = "Mixed Reality Toolkit/Project Settings", fileName = "XRTKProjectSettings", order = (int)CreateProfileMenuItemIndices.Settings)]
    public class XRTKProjectSettings : ScriptableObject
    {
        public static void ProjectSettingsErrorMsg() { Debug.LogError("XRTK Project Settings not found, please create a new object and set it in Project>XRTK"); }

        static XRTKProjectSettings()
        {
            // Preferences aren't guaranteed to hydrate before static constructors or methods.
            MixedRealityPreferences.OnPreferencesLoaded += () =>
            {
                if (projectSettings == null && !string.IsNullOrWhiteSpace(projectSettingsGuid))
                {
                    Debug.Log($"Trying to load project asset... ({projectSettingsGuid})");
                    var loadedProjectSettings = AssetDatabase.LoadAssetAtPath<XRTKProjectSettings>(AssetDatabase.GUIDToAssetPath(projectSettingsGuid));

                    if (loadedProjectSettings == null)
                    {
                        ProjectSettingsErrorMsg();
                    }
                    else
                    {
                        Debug.Log($"Project asset loaded! ({projectSettingsGuid})");
                        projectSettings = new SerializedObject(loadedProjectSettings);
                    }
                }
            };
        }

        private const string ProjectSettingsGUIDKey = "XRTK_SettingsGUID";
        [EditorPreference(Key = ProjectSettingsGUIDKey, DefaultValue = "Assets/ProjectSettings.asset", ApplicationPrefix = true)]
        private static string projectSettingsGuid;
        /// <summary>
        /// Relative path to the project settings asset.
        /// </summary>
        public static string ProjectSettingsGUID
        {
            get => projectSettingsGuid;
            set => EditorPreferences.Set(ProjectSettingsGUIDKey, projectSettingsGuid = value);
        }

        private static SerializedObject projectSettings;

        public static SerializedObject ProjectSettings => projectSettings;

        public static XRTKProjectSettings ProjectSettingsObject => projectSettings?.targetObject as XRTKProjectSettings;

        [SerializeField]
        private SceneAsset startupScene;

        /// <summary>
        /// The <see cref="StartSceneAsset"/> for the global start scene.
        /// </summary>
        public SceneAsset StartupScene
        {
            get => startupScene;
            set => startupScene = value;
        }

        [SerializeField]
        private SymbolicLinkSettings symbolicLinkSettings;

        /// <summary>
        /// The symbolic link settings for this project.
        /// </summary>
        public SymbolicLinkSettings SymbolicLinkSettings
        {
            get => symbolicLinkSettings;
            set => symbolicLinkSettings = value;
        }

        [SerializeField]
        private bool enableCanvasUtilityDialog;

        /// <summary>
        /// Should the <see cref="Canvas"/> utility dialog show when updating the <see cref="RenderMode"/> settings on that component?
        /// </summary>
        public bool EnableCanvasUtilityDialog { get => enableCanvasUtilityDialog; set => enableCanvasUtilityDialog = value; }

        [SerializeField]
        private string profileGenerationPath = "Assets/XRTK.Generated/Profiles";

        public string ProfileGenerationPath
        {
            get => profileGenerationPath;
            set => profileGenerationPath = value;
        }

        public XRTKProjectSettings() { }

        [SettingsProvider]
        public static SettingsProvider ProjectSettingsProvider()
        {
            return new SettingsProvider("Project/XRTK", SettingsScope.Project)
            {
                activateHandler = ProjectSettingsElements,
            };
        }

        private static void ProjectSettingsElements(string context, VisualElement root)
        {
            // root.Add(new XRTKLogo());

            var projectSettings = new ObjectField
            {
                label = "Project Settings Object: ",
                allowSceneObjects = false,
                objectType = typeof(XRTKProjectSettings),
                value = ProjectSettingsObject
            };
            projectSettings.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue != null && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(evt.newValue, out var psGuid, out long localId))
                {
                    XRTKProjectSettings.projectSettings = new SerializedObject(evt.newValue);
                    ProjectSettingsGUID = psGuid;
                }
                else
                {
                    XRTKProjectSettings.projectSettings = null;
                    ProjectSettingsGUID = "";
                }

                root.Clear();
                EditorApplication.delayCall += () => ProjectSettingsElements("", root);
            });
            root.Add(projectSettings);

            if (XRTKProjectSettings.projectSettings == null) { return; }

            var startupSceneField = new ObjectField
            {
                label = "Startup Scene: ",
                tooltip = "When pressing play in the editor, a prompt will ask you if you want to switch to this start scene.\n\nThis setting only applies to the currently running project.",
                allowSceneObjects = false,
                objectType = typeof(SceneAsset)
            };
            startupSceneField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue != null && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(evt.newValue, out var guid, out long localId))
                {
                    var scene = new EditorBuildSettingsScene(guid, true);
                    EditorBuildSettings.scenes = EditorBuildSettings.scenes.Where(s => s.guid != new GUID(guid)).Prepend(scene).ToArray();
                }
            });
            startupSceneField.BindProperty(XRTKProjectSettings.projectSettings.FindProperty(nameof(startupScene)));

            var symbolicLinksField = new ObjectField
            {
                label = "Symbolic Links: ",
                tooltip = "Symbolic Link settings for this project.",
                allowSceneObjects = false,
                objectType = typeof(SymbolicLinkSettings)
            };
            symbolicLinksField.BindProperty(XRTKProjectSettings.projectSettings.FindProperty(nameof(symbolicLinkSettings)));

            var enableCanvasDialog = new Toggle{label = "Enable Canvas dialog utility"};
            enableCanvasDialog.BindProperty(XRTKProjectSettings.projectSettings.FindProperty(nameof(enableCanvasUtilityDialog)));

            var profileGenerationPathField = new TextField
            {
                label = "Profile Generation Path: "
            };
            profileGenerationPathField.BindProperty(XRTKProjectSettings.projectSettings.FindProperty(nameof(profileGenerationPath)));

            root.Add(startupSceneField);
            root.Add(symbolicLinksField);
            root.Add(enableCanvasDialog);
            root.Add(profileGenerationPathField);
        }
    }

    public static class MixedRealityPreferences
    {
        private static readonly string[] XRTK_Keywords = { "XRTK", "Mixed", "Reality" };

        #region Preference Hydration

        public delegate void OnPreferencesLoadedDelegate();

        public static event OnPreferencesLoadedDelegate OnPreferencesLoaded;

        [InitializeOnLoadMethod]
        public static void HydratePreferences()
        {
            var keyPreferenceTypes =
                from asm in AppDomain.CurrentDomain.GetAssemblies()
                from type in asm.GetTypes() // ouch
                from field in type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public) // ouch 2x
                where field.IsStatic   
                let keyAttribute = field.GetCustomAttribute<KeyPreferenceAttribute>()
                where keyAttribute != null && !string.IsNullOrWhiteSpace(keyAttribute.Key)
                select (keyAttribute, field);

            
            foreach (var (keyAttribute, field) in keyPreferenceTypes)
            {
                if (keyAttribute is EditorPreferenceAttribute)
                {
                    field.SetValue(null, HydrateEditorPreference(keyAttribute, field.FieldType));
                }
            }

            EditorApplication.delayCall += () => OnPreferencesLoaded?.Invoke();
        }

        private static object HydrateEditorPreference(KeyPreferenceAttribute keyPreference, Type typeCode)
        {
            switch (Type.GetTypeCode(typeCode))
            {
                case TypeCode.Boolean:
                    return EditorPreferences.Get(keyPreference.Key, (bool) keyPreference.DefaultValue,
                        keyPreference.ApplicationPrefix);
                case TypeCode.Int32:
                    return EditorPreferences.Get(keyPreference.Key, (int) keyPreference.DefaultValue,
                        keyPreference.ApplicationPrefix);
                case TypeCode.Single:
                    return EditorPreferences.Get(keyPreference.Key, (float) keyPreference.DefaultValue,
                        keyPreference.ApplicationPrefix);
                case TypeCode.String:
                    return EditorPreferences.Get(keyPreference.Key, (string) keyPreference.DefaultValue,
                        keyPreference.ApplicationPrefix);
                default:
                    return keyPreference.DefaultValue;
            }
        }


        private static void HydrateSessionPreference(KeyPreferenceAttribute keyPreference, Type type)
        {
            var key = keyPreference.ApplicationPrefix ? $"{Application.productName}_{keyPreference.Key}" : keyPreference.Key;
            if (type == typeof(Vector3)) SessionState.SetVector3(key, (Vector3)keyPreference.DefaultValue);
            else if (type == typeof(int[])) SessionState.SetIntArray(key, (int[])keyPreference.DefaultValue);
            else switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    SessionState.SetBool(key, (bool)keyPreference.DefaultValue);
                    return;
                case TypeCode.Int32:
                    SessionState.SetInt(key, (int)keyPreference.DefaultValue);
                    return;
                case TypeCode.Single:
                    SessionState.SetFloat(key, (float)keyPreference.DefaultValue);
                    return;
                case TypeCode.String:
                    SessionState.SetString(key, (string)keyPreference.DefaultValue);
                    return;
            }
        }

#endregion

        private const string DebugSymbolicInfoKey = "EnablePackageDebug";
        [EditorPreference(Key = DebugSymbolicInfoKey, DefaultValue = false)]
        private static bool debugSymbolicInfo;
        /// <summary>
        /// Enabled debugging info for the xrtk symbolic linking.
        /// </summary>
        public static bool DebugSymbolicInfo { get => debugSymbolicInfo; set => EditorPrefs.SetBool(DebugSymbolicInfoKey, debugSymbolicInfo = value);  }

        private const string AutoloadSymbolicLinksKey = "_AutoLoadSymbolicLinks";
        [EditorPreference(Key = AutoloadSymbolicLinksKey, DefaultValue = true)]
        private static bool autoloadSymbolicLinks;
        /// <summary>
        /// Should the project automatically load symbolic links?
        /// </summary>
        public static bool AutoLoadSymbolicLinks { get => autoloadSymbolicLinks; set => EditorPrefs.SetBool(AutoloadSymbolicLinksKey, autoloadSymbolicLinks = value); }

        private const string IgnoreSettingsDialogKey = "_MixedRealityToolkit_Editor_IgnoreSettingsPrompts";
        [EditorPreference(Key = IgnoreSettingsDialogKey, DefaultValue = false)]
        private static bool ignoreSettingsDialog;
        /// <summary>
        /// Should the settings prompt show on startup?
        /// </summary>
        public static bool IgnoreSettingsDialog { get => ignoreSettingsDialog; set => EditorPrefs.SetBool(IgnoreSettingsDialogKey, ignoreSettingsDialog = value); }

        [SettingsProvider]
        private static SettingsProvider Preferences()
        {
            return new SettingsProvider("Preferences/XRTK", SettingsScope.User, XRTK_Keywords)
            {
                label = "XRTK",
                activateHandler = XRTKPreferences,
                keywords = new HashSet<string>(XRTK_Keywords)
            };
        }

        private static void XRTKPreferences(string searchContext, VisualElement root)
        {
            var debugSymbolicInfoField = new Toggle {label = "Debug Symbolic Info", value = debugSymbolicInfo, tooltip = "Enable or disable the debug information for symbolic linking.\n\nThis setting only applies to the currently running project."};
            debugSymbolicInfoField.RegisterValueChangedCallback(evt => { DebugSymbolicInfo = evt.newValue; });

            var autoloadSymbolicLinksField = new Toggle {label = "Autoload Symbolic Links", value = autoloadSymbolicLinks};
            autoloadSymbolicLinksField.RegisterValueChangedCallback(evt => { AutoLoadSymbolicLinks = evt.newValue; });;

            var settingsDialogField = new Toggle
            {
                label = "Ignore settings prompt on startup",
                tooltip = "Prevents settings dialog popup from showing on startup.\n\nThis setting applies to all projects using XRTK.",
                value = ignoreSettingsDialog,
            };
            settingsDialogField.RegisterValueChangedCallback(evt => IgnoreSettingsDialog = evt.newValue);

            root.Add(debugSymbolicInfoField);
            root.Add(autoloadSymbolicLinksField);
            root.Add(settingsDialogField);
        }
    }
}
