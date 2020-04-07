// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEditor;
using UnityEngine;

namespace XRTK.Utilities.Editor
{
    /// <summary>
    /// Convenience class for setting Editor Preferences with <see cref="Application.productName"/> as key prefix.
    /// </summary>
    public static class EditorPreferences
    {
        /// <summary>
        /// Set the saved <see cref="string"/> from to <see cref="EditorPrefs"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Set(string key, string value, bool applicationPrefix = true)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(key));
            var prefKey = applicationPrefix ? $"{Application.productName}_{key}" : key;
            EditorPrefs.SetString(prefKey, value);
        }

        /// <summary>
        /// Set the saved <see cref="bool"/> from to <see cref="EditorPrefs"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Set(string key, bool value, bool applicationPrefix = true)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(key));
            var prefKey = applicationPrefix ? $"{Application.productName}_{key}" : key;
            EditorPrefs.SetBool(prefKey, value);
        }

        /// <summary>
        /// Set the saved <see cref="float"/> from the <see cref="EditorPrefs"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Set(string key, float value, bool applicationPrefix = true)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(key));
            var prefKey = applicationPrefix ? $"{Application.productName}_{key}" : key;
            EditorPrefs.SetFloat(prefKey, value);
        }

        /// <summary>
        /// Set the saved <see cref="int"/> from the <see cref="EditorPrefs"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Set(string key, int value, bool applicationPrefix = true)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(key));
            var prefKey = applicationPrefix ? $"{Application.productName}_{key}" : key;
            EditorPrefs.SetInt(prefKey, value);
        }

        /// <summary>
        /// Get the saved <see cref="string"/> from the <see cref="EditorPrefs"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        public static string Get(string key, string defaultValue, bool applicationPrefix = true)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(key));
            var prefKey = applicationPrefix ? $"{Application.productName}_{key}" : key;
            if (EditorPrefs.HasKey(prefKey))
            {
                return EditorPrefs.GetString(prefKey);
            }

            EditorPrefs.SetString(prefKey, defaultValue);
            return defaultValue;
        }

        /// <summary>
        /// Get the saved <see cref="bool"/> from the <see cref="EditorPrefs"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        public static bool Get(string key, bool defaultValue, bool applicationPrefix = true)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(key));
            var prefKey = applicationPrefix ? $"{Application.productName}_{key}" : key;
            if (EditorPrefs.HasKey(prefKey))
            {
                return EditorPrefs.GetBool(prefKey);
            }

            EditorPrefs.SetBool(prefKey, defaultValue);
            return defaultValue;
        }

        /// <summary>
        /// Get the saved <see cref="float"/> from the <see cref="EditorPrefs"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        public static float Get(string key, float defaultValue, bool applicationPrefix = true)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(key));
            var prefKey = applicationPrefix ? $"{Application.productName}_{key}" : key;
            if (EditorPrefs.HasKey(prefKey))
            {
                return EditorPrefs.GetFloat(prefKey);
            }

            EditorPrefs.SetFloat(prefKey, defaultValue);
            return defaultValue;
        }

        /// <summary>
        /// Get the saved <see cref="int"/> from the <see cref="EditorPrefs"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        public static int Get(string key, int defaultValue, bool applicationPrefix = true)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(key));
            var prefKey = applicationPrefix ? $"{Application.productName}_{key}" : key;
            if (EditorPrefs.HasKey(prefKey))
            {
                return EditorPrefs.GetInt(prefKey);
            }

            EditorPrefs.SetInt(prefKey, defaultValue);
            return defaultValue;
        }
    }
}
