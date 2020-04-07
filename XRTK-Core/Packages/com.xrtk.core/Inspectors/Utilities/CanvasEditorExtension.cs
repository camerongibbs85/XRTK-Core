﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEditor;
using UnityEngine;
using XRTK.Extensions;
using XRTK.Interfaces.InputSystem;
using XRTK.Services;
using XRTK.Utilities;

namespace XRTK.Inspectors.Utilities
{
    /// <summary>
    /// Helper class to assign the UIRaycastCamera when creating a new canvas object and assigning the world space render mode.
    /// </summary>
    [CustomEditor(typeof(Canvas))]
    public class CanvasEditorExtension : Editor
    {
        private const string DialogText = "Hi there, we noticed that you've changed this canvas to use WorldSpace.\n\n" +
                                          "In order for the InputManager to work properly with uGUI raycasting we'd like to update this canvas' " +
                                          "WorldCamera to use the FocusProvider's UIRaycastCamera.\n";

        private Canvas canvas;

        private bool hasUtility = false;

        private static bool IsUtilityValid =>
            MixedRealityToolkit.Instance != null &&
            MixedRealityToolkit.HasActiveProfile &&
            MixedRealityToolkit.Instance.ActiveProfile.IsInputSystemEnabled &&
            MixedRealityToolkit.GetService<IMixedRealityInputSystem>(false) != null;

        private bool CanUpdateSettings
        {
            get
            {
                if (!MixedRealityToolkit.IsInitialized ||
                    XRTKProjectSettings.ProjectSettingsObject?.EnableCanvasUtilityDialog == true)
                {
                    return false;
                }

                var utility = canvas.GetComponent<CanvasUtility>();

                hasUtility = utility != null;

                return hasUtility && !IsUtilityValid ||
                       !hasUtility && IsUtilityValid;
            }
        }

        private void OnEnable()
        {
            canvas = (Canvas)target;

            if (CanUpdateSettings)
            {
                UpdateCanvasSettings();
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck() && CanUpdateSettings)
            {
                UpdateCanvasSettings();
            }
        }

        private void UpdateCanvasSettings()
        {
            bool removeUtility = false;

            // Update the world camera if we need to.
            if (IsUtilityValid &&
                canvas.isRootCanvas &&
                canvas.renderMode == RenderMode.WorldSpace &&
                canvas.worldCamera != MixedRealityToolkit.InputSystem.FocusProvider.UIRaycastCamera)
            {
                var selection = EditorUtility.DisplayDialogComplex("Attention!", DialogText, "OK", "Cancel", "Dismiss Forever");
                switch (selection)
                {
                    case 0:
                        canvas.worldCamera = MixedRealityToolkit.InputSystem.FocusProvider.UIRaycastCamera;
                        break;
                    case 1:
                        removeUtility = true;
                        break;
                    case 2:
                        XRTKProjectSettings.ProjectSettingsObject.EnableCanvasUtilityDialog = false;
                        XRTKProjectSettings.ProjectSettings.ApplyModifiedProperties();
                        removeUtility = true;
                        break;
                }
            }

            // Add the Canvas Helper if we need it.
            if (IsUtilityValid &&
                canvas.isRootCanvas &&
                canvas.renderMode == RenderMode.WorldSpace &&
                canvas.worldCamera == MixedRealityToolkit.InputSystem.FocusProvider.UIRaycastCamera)
            {
                var helper = canvas.gameObject.EnsureComponent<CanvasUtility>();
                helper.Canvas = canvas;
            }

            // Reset the world canvas if we need to.
            if (IsUtilityValid &&
                canvas.isRootCanvas &&
                canvas.renderMode != RenderMode.WorldSpace &&
                canvas.worldCamera == MixedRealityToolkit.InputSystem.FocusProvider.UIRaycastCamera)
            {
                // Sets it back to MainCamera default.
                canvas.worldCamera = null;
            }

            var utility = canvas.GetComponent<CanvasUtility>();

            // Remove the helper if we don't need it.
            if (removeUtility || !IsUtilityValid)
            {
                if (utility != null)
                {
                    canvas.worldCamera = null;
                    EditorApplication.delayCall += () => DestroyImmediate(utility);
                }

                hasUtility = false;
            }
            else
            {
                if (canvas.renderMode == RenderMode.WorldSpace)
                {
                    Debug.Assert(utility != null);
                    hasUtility = true;
                }
            }
        }
    }
}