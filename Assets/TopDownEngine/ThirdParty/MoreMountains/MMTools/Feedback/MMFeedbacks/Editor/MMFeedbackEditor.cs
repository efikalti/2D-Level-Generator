using System;
using UnityEngine;
using UnityEditor;

namespace MoreMountains.Tools
{
    /// <summary>
    /// A custom editor for each MMFeedback
    /// </summary>
    [CustomEditor(typeof(MMFeedback), true)]
    public class MMFeedbackEditor : Editor
    {
        public bool showMMFeedback;
        public MMFeedbacksEditor ParentEditor;

        protected bool _active;        
        protected MMFeedback _mmFeedback;
        protected static readonly string[] _dontIncludeMe = new string[] { "m_Script" };
        protected const string _activePropertyName = "Active";
        protected const string _debugActiveName = "DebugActive";
        protected const float _removeButtonWidth = 30f;
        protected const float _checkboxWidth = 30f;
        protected SerializedProperty _m_active;
        protected Editor _editor;
        protected SerializedProperty _debug;

        /// <summary>
        /// On Enable, we grabd our properties and hide the MMFeedback from drawing in the inspector.
        /// Its inspector will be drawn inside the MMFeedbacks editor
        /// </summary>
        private void OnEnable()
        {
            if (target == null)
            {
                return;
            }
            _mmFeedback = (MMFeedback)target;
            _m_active = serializedObject.FindProperty(_activePropertyName);
            _debug = serializedObject.FindProperty(_debugActiveName);
        }

        /// <summary>
        /// Draws a custom, foldable editor, complete with an activation checkbox and Play and Remove test buttons
        /// </summary>
        public virtual void OnCustomInspectorGUI()
        {
            _mmFeedback = (MMFeedback)target;
            serializedObject.Update();
            _debug = serializedObject.FindProperty(_debugActiveName);
            
            if (_debug.boolValue)
            {
                target.hideFlags = HideFlags.None;
            }
            else
            {
                target.hideFlags = HideFlags.HideInInspector;
            }       
            EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                _active = _m_active.boolValue;
                _active = EditorGUILayout.Toggle("", _active, GUILayout.Width(_removeButtonWidth));
                    showMMFeedback = EditorGUILayout.Foldout(showMMFeedback, _mmFeedback.Label);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Play", EditorStyles.miniButtonLeft))
                    {
                        if (_mmFeedback.Owner != null)
                        {
                            _mmFeedback.Play(_mmFeedback.Owner.transform.position, 1.0f);
                        }
                        else
                        {
                            _mmFeedback.Play(Vector3.zero, 1.0f);
                        }                    
                    }
                    if (GUILayout.Button("Remove", EditorStyles.miniButtonRight))
                    {
                        ParentEditor.RemoveFeedback(_mmFeedback);                    
                    }
                    _m_active.boolValue = _active;      
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                if (showMMFeedback)
                {
                    DrawMMFeedback();
                }
                EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }
        
        /// <summary>
        /// Draws the contents of the MMFeedback
        /// </summary>
        protected virtual void DrawMMFeedback()
        {
            DrawPropertiesExcluding(serializedObject, _dontIncludeMe);
        }

        /// <summary>
        /// A public method to put this feedback in debug mode (or not)
        /// </summary>
        /// <param name="status"></param>
        public virtual void Debug(bool status)
        {
            _debug.boolValue = status;
            EditorUtility.SetDirty(target);
            serializedObject.ApplyModifiedProperties();
            Repaint();
        }
    }
}

