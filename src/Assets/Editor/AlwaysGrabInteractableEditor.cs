using UnityEditor.XR.Interaction.Toolkit.Utilities;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEditor.XR.Interaction.Toolkit
{
    /// <summary>
    /// Custom editor for an <see cref="AlwaysGrabInteractable"/>.
    /// </summary>
    [CustomEditor(typeof(AlwaysGrabInteractable), true), CanEditMultipleObjects]
    public class AlwaysGrabInteractableEditor : XRBaseInteractableEditor
    {
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.attachTransform"/>.</summary>
        protected SerializedProperty m_AttachTransform;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.attachEaseInTime"/>.</summary>
        protected SerializedProperty m_AttachEaseInTime;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.movementType"/>.</summary>
        protected SerializedProperty m_MovementType;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.trackPosition"/>.</summary>
        protected SerializedProperty m_TrackPosition;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.smoothPosition"/>.</summary>
        protected SerializedProperty m_SmoothPosition;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.smoothPositionAmount"/>.</summary>
        protected SerializedProperty m_SmoothPositionAmount;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.tightenPosition"/>.</summary>
        protected SerializedProperty m_TightenPosition;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.trackRotation"/>.</summary>
        protected SerializedProperty m_TrackRotation;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.smoothRotation"/>.</summary>
        protected SerializedProperty m_SmoothRotation;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.smoothRotationAmount"/>.</summary>
        protected SerializedProperty m_SmoothRotationAmount;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.tightenRotation"/>.</summary>
        protected SerializedProperty m_TightenRotation;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.attachPointCompatibilityMode"/>.</summary>
        protected SerializedProperty m_AttachPointCompatibilityMode;

        /// <summary>Value to be checked before recalculate if the inspected object has a non-uniformly scaled parent.</summary>
        bool m_RecalculateHasNonUniformScale = true;
        /// <summary>Caches if the inspected object has a non-uniformly scaled parent.</summary>
        bool m_HasNonUniformScale;

        /// <summary>
        /// Contents of GUI elements used by this editor.
        /// </summary>
        protected static class Contents
        {
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.attachTransform"/>.</summary>
            public static readonly GUIContent attachTransform = EditorGUIUtility.TrTextContent("Attach Transform", "The attachment point to use on this Interactable (will use this object's position if none set).");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.attachEaseInTime"/>.</summary>
            public static readonly GUIContent attachEaseInTime = EditorGUIUtility.TrTextContent("Attach Ease In Time", "Time in seconds to ease in the attach when selected (a value of 0 indicates no easing).");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.movementType"/>.</summary>
            public static readonly GUIContent movementType = EditorGUIUtility.TrTextContent("Movement Type", "Specifies how this object is moved when selected, either through setting the velocity of the Rigidbody, moving the kinematic Rigidbody during Fixed Update, or by directly updating the Transform each frame.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.trackPosition"/>.</summary>
            public static readonly GUIContent trackPosition = EditorGUIUtility.TrTextContent("Track Position", "Whether this object should follow the position of the Interactor when selected.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.smoothPosition"/>.</summary>
            public static readonly GUIContent smoothPosition = EditorGUIUtility.TrTextContent("Smooth Position", "Apply smoothing while following the position of the Interactor when selected.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.smoothPositionAmount"/>.</summary>
            public static readonly GUIContent smoothPositionAmount = EditorGUIUtility.TrTextContent("Smooth Position Amount", "Scale factor for how much smoothing is applied while following the position of the Interactor when selected. The larger the value, the closer this object will remain to the position of the Interactor.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.tightenPosition"/>.</summary>
            public static readonly GUIContent tightenPosition = EditorGUIUtility.TrTextContent("Tighten Position", "Reduces the maximum follow position difference when using smoothing. The value ranges from 0 meaning no bias in the smoothed follow distance, to 1 meaning effectively no smoothing at all.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.trackRotation"/>.</summary>
            public static readonly GUIContent trackRotation = EditorGUIUtility.TrTextContent("Track Rotation", "Whether this object should follow the rotation of the Interactor when selected.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.smoothRotation"/>.</summary>
            public static readonly GUIContent smoothRotation = EditorGUIUtility.TrTextContent("Smooth Rotation", "Apply smoothing while following the rotation of the Interactor when selected.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.smoothRotationAmount"/>.</summary>
            public static readonly GUIContent smoothRotationAmount = EditorGUIUtility.TrTextContent("Smooth Rotation Amount", "Scale factor for how much smoothing is applied while following the rotation of the Interactor when selected. The larger the value, the closer this object will remain to the rotation of the Interactor.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.tightenRotation"/>.</summary>
            public static readonly GUIContent tightenRotation = EditorGUIUtility.TrTextContent("Tighten Rotation", "Reduces the maximum follow rotation difference when using smoothing. The value ranges from 0 meaning no bias in the smoothed follow rotation, to 1 meaning effectively no smoothing at all.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.attachPointCompatibilityMode"/>.</summary>
            public static readonly GUIContent attachPointCompatibilityMode = EditorGUIUtility.TrTextContent("Attach Point Compatibility Mode", "Use Default for consistent attach points between all Movement Type values. Use Legacy for older projects that want to maintain the incorrect method which was partially based on center of mass.");

            /// <summary>Message for non-uniformly scaled parent.</summary>
            public static readonly string nonUniformScaledParentWarning = "When a child object has a non-uniformly scaled parent and is rotated relative to that parent, it may appear skewed. To avoid this, use uniform scale in all parents' Transform of this object.";

            /// <summary>Array of type <see cref="GUIContent"/> for the options shown in the popup for <see cref="XRGrabInteractable.attachPointCompatibilityMode"/>.</summary>
            public static readonly GUIContent[] attachPointCompatibilityModeOptions =
            {
                EditorGUIUtility.TrTextContent("Default (Recommended)"),
                EditorGUIUtility.TrTextContent("Legacy (Obsolete)")
            };
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();

            m_AttachTransform = serializedObject.FindProperty("m_AttachTransform");
            m_AttachEaseInTime = serializedObject.FindProperty("m_AttachEaseInTime");
            m_MovementType = serializedObject.FindProperty("m_MovementType");
            m_TrackPosition = serializedObject.FindProperty("m_TrackPosition");
            m_SmoothPosition = serializedObject.FindProperty("m_SmoothPosition");
            m_SmoothPositionAmount = serializedObject.FindProperty("m_SmoothPositionAmount");
            m_TightenPosition = serializedObject.FindProperty("m_TightenPosition");
            m_TrackRotation = serializedObject.FindProperty("m_TrackRotation");
            m_SmoothRotation = serializedObject.FindProperty("m_SmoothRotation");
            m_SmoothRotationAmount = serializedObject.FindProperty("m_SmoothRotationAmount");
            m_TightenRotation = serializedObject.FindProperty("m_TightenRotation");
            m_AttachPointCompatibilityMode = serializedObject.FindProperty("m_AttachPointCompatibilityMode");

            Undo.postprocessModifications += OnPostprocessModifications;
        }

        /// <summary>
        /// This function is called when the object becomes disabled.
        /// </summary>
        /// <seealso cref="MonoBehaviour"/>
        protected virtual void OnDisable()
        {
            Undo.postprocessModifications -= OnPostprocessModifications;
        }

        /// <inheritdoc />
        protected override void DrawProperties()
        {
            base.DrawProperties();

            EditorGUILayout.Space();

            DrawGrabConfiguration();
            DrawTrackConfiguration();
            DrawAttachConfiguration();
        }

        /// <summary>
        /// Draw the property fields related to grab configuration.
        /// </summary>
        protected virtual void DrawGrabConfiguration()
        {
            EditorGUILayout.PropertyField(m_MovementType, Contents.movementType);
            DrawNonUniformScaleMessage();
        }

        /// <summary>
        /// Draw the property fields related to tracking configuration.
        /// </summary>
        protected virtual void DrawTrackConfiguration()
        {
            EditorGUILayout.PropertyField(m_TrackPosition, Contents.trackPosition);
            if (m_TrackPosition.boolValue)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(m_SmoothPosition, Contents.smoothPosition);
                    if (m_SmoothPosition.boolValue)
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            EditorGUILayout.PropertyField(m_SmoothPositionAmount, Contents.smoothPositionAmount);
                            EditorGUILayout.PropertyField(m_TightenPosition, Contents.tightenPosition);
                        }
                    }
                }
            }

            EditorGUILayout.PropertyField(m_TrackRotation, Contents.trackRotation);
            if (m_TrackRotation.boolValue)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(m_SmoothRotation, Contents.smoothRotation);
                    if (m_SmoothRotation.boolValue)
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            EditorGUILayout.PropertyField(m_SmoothRotationAmount, Contents.smoothRotationAmount);
                            EditorGUILayout.PropertyField(m_TightenRotation, Contents.tightenRotation);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draw property fields related to attach configuration.
        /// </summary>
        protected virtual void DrawAttachConfiguration()
        {
            EditorGUILayout.PropertyField(m_AttachTransform, Contents.attachTransform);
            EditorGUILayout.PropertyField(m_AttachEaseInTime, Contents.attachEaseInTime);
            EditorGUILayout.PropertyField(m_AttachPointCompatibilityMode, Contents.attachPointCompatibilityMode);
            //XRInteractionEditorGUI.EnumPropertyField(m_AttachPointCompatibilityMode, Contents.attachPointCompatibilityMode, Contents.attachPointCompatibilityModeOptions);
        }

        /// <summary>
        /// Checks if the object has a non-uniformly scaled parent and draws a message if necessary.
        /// </summary>
        protected virtual void DrawNonUniformScaleMessage()
        {
            if (m_RecalculateHasNonUniformScale)
            {
                var monoBehaviour = target as MonoBehaviour;
                if (monoBehaviour == null)
                    return;

                var transform = monoBehaviour.transform;
                if (transform == null)
                    return;

                m_HasNonUniformScale = false;
                for (var parent = transform.parent; parent != null; parent = parent.parent)
                {
                    var localScale = parent.localScale;
                    if (!Mathf.Approximately(localScale.x, localScale.y) ||
                        !Mathf.Approximately(localScale.x, localScale.z))
                    {
                        m_HasNonUniformScale = true;
                        break;
                    }
                }

                m_RecalculateHasNonUniformScale = false;
            }

            if (m_HasNonUniformScale)
                EditorGUILayout.HelpBox(Contents.nonUniformScaledParentWarning, MessageType.Warning);
        }

        /// <summary>
        /// Callback registered to be triggered whenever a new set of property modifications is created.
        /// </summary>
        /// <seealso cref="Undo.postprocessModifications"/>
        protected virtual UndoPropertyModification[] OnPostprocessModifications(UndoPropertyModification[] modifications)
        {
            m_RecalculateHasNonUniformScale = true;
            return modifications;
        }
    }
}
