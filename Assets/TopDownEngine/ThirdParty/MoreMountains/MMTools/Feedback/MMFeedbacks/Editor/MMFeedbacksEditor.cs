using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

namespace MoreMountains.Tools
{
    /// <summary>
    /// A custom editor displaying a foldable list of MMFeedbacks, a dropdown to add more, as well as test buttons to test your feedbacks at runtime
    /// </summary>
    [CustomEditor(typeof(MMFeedbacks))]
    public class MMFeedbacksEditor : Editor
    {
        protected MMFeedbacks _mmFeedbacks;
        protected Type[] _mmFeedbackTypes;
        protected string[] _mmFeedbackTypesNames;
        protected int _selectedIndex;        
        protected Dictionary<MMFeedback, MMFeedbackEditor> _feedbackEditorDictionary = new Dictionary<MMFeedback, MMFeedbackEditor>();
        protected List<MMFeedback> _delayedRemovalList = new List<MMFeedback>();
        protected const string _feedbacksName = "Feedbacks";
        protected const string _autoInitializationModeName = "InitializationMode";
        protected const string _debugActiveName = "DebugActive";        
        protected SerializedProperty _mmFeedbacksFeedbacks;
        protected SerializedProperty _mmFeedbacksInitializationMode;
        protected SerializedProperty _debug;

        /// <summary>
        /// On Enable, grabs properties and initializes the add feedback dropdown's contents
        /// </summary>
        private void OnEnable()
        {
            _mmFeedbacks = (MMFeedbacks)target;

            SetFeedbackNamesArray();
            _mmFeedbacksFeedbacks = serializedObject.FindProperty(_feedbacksName);
            _mmFeedbacksInitializationMode = serializedObject.FindProperty(_autoInitializationModeName);
            _debug = serializedObject.FindProperty(_debugActiveName);
            if (_mmFeedbacks.Feedbacks == null)
            {
                _mmFeedbacks.Feedbacks = new List<MMFeedback>();
                return;
            }
        }

        /// <summary>
        /// On Disable, clears the dictionary and removes all feedbacks scheduled for destruction
        /// </summary>
        private void OnDisable()
        {
            foreach (var kvp in _feedbackEditorDictionary)
            {
                DestroyImmediate(kvp.Value);
            }
            _feedbackEditorDictionary.Clear();            
            RemoveDelayedFeedbacks();
        }
                
        /// <summary>
        /// Draws the inspector, complete with helpbox, init mode selection, list of feedbacks, feedback selection and test buttons
        /// </summary>
        public override void OnInspectorGUI()
        {
            _mmFeedbacks = (MMFeedbacks)target;
            serializedObject.Update();
            EditorGUILayout.Space();
            AddFeedbacksIfNeeded();
            if (_debug.boolValue)
            {
                EditorGUILayout.PropertyField(_mmFeedbacksFeedbacks, true);
            }                
            EditorGUILayout.HelpBox("Select feedbacks from the 'add a feedback' dropdown and customize them. Remember, if you don't use auto initialization, " +
                "you'll need to initialize them via script.", MessageType.None);
            EditorGUILayout.PropertyField(_mmFeedbacksInitializationMode);
            EditorGUILayout.Space();
            if (_mmFeedbacks.Feedbacks.Count > 0)
            {
                for (int i = 0; i<_mmFeedbacks.Feedbacks.Count; i++)
                {
                    if (_mmFeedbacks.Feedbacks[i] != null)
                    {
                        MMFeedbackEditor feedbackEditor = StoreFeedbackEditorInDictionary(_mmFeedbacks.Feedbacks[i]);
                        if (feedbackEditor != null)
                        {
                            (feedbackEditor as MMFeedbackEditor).OnCustomInspectorGUI();
                        }
                    }                                       
                }
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }                        
            DisplayFeedbackTypeSelection();
            EditorGUILayout.Space();
            DisplayTestButtons();
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Stores a new feedback and its editor, caching it 
        /// </summary>
        /// <param name="feedback"></param>
        /// <returns></returns>
        public virtual MMFeedbackEditor StoreFeedbackEditorInDictionary(MMFeedback feedback)
        {
            if (!_feedbackEditorDictionary.ContainsKey(feedback))
            {
                _feedbackEditorDictionary.Add(feedback, Editor.CreateEditor(feedback) as MMFeedbackEditor);
            }
            Editor feedbackEditor = _feedbackEditorDictionary[feedback];
            Editor.CreateCachedEditor(feedback, typeof(MMFeedbackEditor), ref feedbackEditor);
            (feedbackEditor as MMFeedbackEditor).ParentEditor = this;
            return feedbackEditor as MMFeedbackEditor;
        }

        /// <summary>
        /// Displays initialize, play, stop and reset buttons in the inspector
        /// </summary>
        protected virtual void DisplayTestButtons()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Tests");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Initialize", EditorStyles.miniButtonLeft))
            {
                _mmFeedbacks.Initialization(_mmFeedbacks.gameObject);
            }
            if (GUILayout.Button("Play Feedbacks", EditorStyles.miniButtonMid))
            {
                _mmFeedbacks.PlayFeedbacks(_mmFeedbacks.transform.position, 1.0f);
            }
            if (GUILayout.Button("Stop", EditorStyles.miniButtonMid))
            {
                _mmFeedbacks.StopFeedbacks(_mmFeedbacks.transform.position, 1.0f);
            }
            if (GUILayout.Button("Reset", EditorStyles.miniButtonMid))
            {
                _mmFeedbacks.ResetFeedbacks();
            }
            if (GUILayout.Button("Debug", EditorStyles.miniButtonRight))
            {
                Debug();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Displays a dropdown that adds a new feedback each time a new item gets selected
        /// </summary>
        protected virtual void DisplayFeedbackTypeSelection()
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Add a feedback", GUILayout.MaxWidth(100f));

            EditorGUI.BeginChangeCheck();
            _selectedIndex = EditorGUILayout.Popup(_selectedIndex, _mmFeedbackTypesNames);            
            if (EditorGUI.EndChangeCheck())
            {
                AddFeedback(_mmFeedbackTypes[_selectedIndex]);                
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// For each stored feedback, makes sure they're where they should be
        /// </summary>
        protected virtual void AddFeedbacksIfNeeded()
        {
            for (int i=0; i < _mmFeedbacks.Feedbacks.Count; i++)
            {
                // if the feedback comes from another gameobject, we copy it, remove it, and add a new one
                if (_mmFeedbacks.Feedbacks[i] == null)
                {
                    _mmFeedbacks.Feedbacks.Remove(_mmFeedbacks.Feedbacks[i]);
                    return;
                }

                if (_mmFeedbacks.Feedbacks[i].gameObject != _mmFeedbacks.gameObject)
                {
                    MMFeedback newMMFeedback = _mmFeedbacks.gameObject.AddComponent(_mmFeedbacks.Feedbacks[i].GetType()) as MMFeedback;  
                    UnityEditorInternal.ComponentUtility.CopyComponent(_mmFeedbacks.Feedbacks[i]);
                    UnityEditorInternal.ComponentUtility.PasteComponentValues(newMMFeedback);                    
                    _mmFeedbacks.Feedbacks.Remove(_mmFeedbacks.Feedbacks[i]);
                    _mmFeedbacks.Feedbacks.Add(newMMFeedback);
                }
            }            
        }

        /// <summary>
        /// Adds a new feedback of the specified type
        /// </summary>
        /// <param name="mmFeedbackType"></param>
        protected virtual void AddFeedback(Type mmFeedbackType)
        {
            MMFeedback newMMFeedback = _mmFeedbacks.gameObject.AddComponent(mmFeedbackType) as MMFeedback;
            
            _mmFeedbacks.Feedbacks.Add(newMMFeedback);
            newMMFeedback.SetCustomName();

            UnityEditor.Undo.RegisterCompleteObjectUndo(_mmFeedbacks, "Add Feedback");
            PrefabUtility.RecordPrefabInstancePropertyModifications(_mmFeedbacks);
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Removes the specified feedback from the list and adds it for delayed removal
        /// </summary>
        /// <param name="feedback"></param>
        public virtual void RemoveFeedback(MMFeedback feedback)
        {
            _mmFeedbacks.Feedbacks.Remove(feedback);
            _delayedRemovalList.Add(feedback);

            UnityEditor.Undo.RegisterCompleteObjectUndo(_mmFeedbacks, "Remove Feedback"); 
            PrefabUtility.RecordPrefabInstancePropertyModifications(_mmFeedbacks);
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Clears all feedbacks scheduled for removal
        /// </summary>
        public virtual void RemoveDelayedFeedbacks()
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (_delayedRemovalList.Count > 0)
            {
               foreach (MMFeedback feedback in _delayedRemovalList)
               {
                    DestroyImmediate(feedback);
               }
                _delayedRemovalList.Clear();
            }

            if (_mmFeedbacks != null)
            {
                Component[] feedbacks = _mmFeedbacks.gameObject.GetComponents(typeof(MMFeedback));

                foreach (MMFeedback feedback in feedbacks)
                {
                    if (!_mmFeedbacks.Feedbacks.Contains(feedback))    
                    {
                        DestroyImmediate(feedback);
                    }
                }
            }
            
                
        }

        /// <summary>
        /// Grabs all feedbacks from the project and builds a list with it
        /// </summary>
        protected virtual void SetFeedbackNamesArray()
        {
            Type mmFeedbackType = typeof(MMFeedback);
            Type[] allTypes = mmFeedbackType.Assembly.GetTypes();
            List<Type> mmFeedbackSubTypeList = new List<Type>();
            for (int i = 0; i < allTypes.Length; i++)
            {
                if (allTypes[i].IsSubclassOf(mmFeedbackType) && !allTypes[i].IsAbstract)
                {
                    mmFeedbackSubTypeList.Add(allTypes[i]);
                }
            }
            _mmFeedbackTypes = mmFeedbackSubTypeList.ToArray();
            List<string> mmFeedbackTypeNameList = new List<string>();
            for (int i = 0; i < _mmFeedbackTypes.Length; i++) 
            {
                mmFeedbackTypeNameList.Add(_mmFeedbackTypes[i].Name.Replace("MMFeedback","")); 
            }
            _mmFeedbackTypesNames = mmFeedbackTypeNameList.ToArray();
        }
        
        /// <summary>
        /// Toggles this MMFeedbacks Editor debug mode on or off
        /// </summary>
        protected virtual void Debug()
        {
            _debug.boolValue = !_debug.boolValue;
            foreach (var kvp in _feedbackEditorDictionary)
            {
                kvp.Value.Debug(_debug.boolValue);
            }
            serializedObject.ApplyModifiedProperties();
            Repaint();
        }
    }
}
