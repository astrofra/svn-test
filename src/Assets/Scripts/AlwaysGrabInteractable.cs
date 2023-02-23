using System;
using UnityEngine.Serialization;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Interactable component that allows basic "grab" functionality.
    /// Can attach to a selecting Interactor and follow it around while obeying physics (and inherit velocity when released).
    /// </summary>
    [SelectionBase]
    [DisallowMultipleComponent]
    [CanSelectMultiple(false)]
    [AddComponentMenu("XR/XR Always Grab Interactable", 11)]
    public partial class AlwaysGrabInteractable : XRBaseInteractable
    {
        const float k_DefaultTighteningAmount = 0.5f;
        const float k_DefaultSmoothingAmount = 5f;
        const float k_DefaultAttachEaseInTime = 0.15f;

        /// <summary>
        /// Controls the method used when calculating the target position of the object.
        /// </summary>
        /// <seealso cref="attachPointCompatibilityMode"/>
        public enum AttachPointCompatibilityMode
        {
            /// <summary>
            /// Use the default, correct method for calculating the target position of the object.
            /// </summary>
            Default,

            /// <summary>
            /// Use an additional offset from the center of mass when calculating the target position of the object.
            /// Also incorporate the scale of the Interactor's Attach Transform.
            /// Marked for deprecation.
            /// This is the backwards compatible support mode for projects that accounted for the
            /// unintended difference when using XR Interaction Toolkit prior to version <c>1.0.0-pre.4</c>.
            /// To have the effective attach position be the same between all <see cref="XRBaseInteractable.MovementType"/> values, use <see cref="Default"/>.
            /// </summary>
            Legacy,
        }

        [SerializeField]
        Transform m_AttachTransform;

        /// <summary>
        /// The attachment point Unity uses on this Interactable (will use this object's position if none set).
        /// </summary>
        public Transform attachTransform
        {
            get => m_AttachTransform;
            set => m_AttachTransform = value;
        }

        [SerializeField]
        float m_AttachEaseInTime = k_DefaultAttachEaseInTime;

        /// <summary>
        /// Time in seconds Unity eases in the attach when selected (a value of 0 indicates no easing).
        /// </summary>
        public float attachEaseInTime
        {
            get => m_AttachEaseInTime;
            set => m_AttachEaseInTime = value;
        }

        [SerializeField]
        MovementType m_MovementType = MovementType.Instantaneous;

        /// <summary>
        /// Specifies how this object moves when selected, either through setting the velocity of the <see cref="Rigidbody"/>,
        /// moving the kinematic <see cref="Rigidbody"/> during Fixed Update, or by directly updating the <see cref="Transform"/> each frame.
        /// </summary>
        /// <seealso cref="XRBaseInteractable.MovementType"/>
        public MovementType movementType
        {
            get => m_MovementType;
            set
            {
                m_MovementType = value;

                if (isSelected)
                {
                    UpdateCurrentMovementType();
                }
            }
        }

        [SerializeField]
        bool m_TrackPosition = true;

        /// <summary>
        /// Whether this object should follow the position of the Interactor when selected.
        /// </summary>
        /// <seealso cref="trackRotation"/>
        public bool trackPosition
        {
            get => m_TrackPosition;
            set => m_TrackPosition = value;
        }

        [SerializeField]
        bool m_SmoothPosition;

        /// <summary>
        /// Whether Unity applies smoothing while following the position of the Interactor when selected.
        /// </summary>
        /// <seealso cref="smoothPositionAmount"/>
        /// <seealso cref="tightenPosition"/>
        public bool smoothPosition
        {
            get => m_SmoothPosition;
            set => m_SmoothPosition = value;
        }

        [SerializeField, Range(0f, 20f)]
        float m_SmoothPositionAmount = k_DefaultSmoothingAmount;

        /// <summary>
        /// Scale factor for how much smoothing is applied while following the position of the Interactor when selected.
        /// The larger the value, the closer this object will remain to the position of the Interactor.
        /// </summary>
        /// <seealso cref="smoothPosition"/>
        /// <seealso cref="tightenPosition"/>
        public float smoothPositionAmount
        {
            get => m_SmoothPositionAmount;
            set => m_SmoothPositionAmount = value;
        }

        [SerializeField, Range(0f, 1f)]
        float m_TightenPosition = k_DefaultTighteningAmount;

        /// <summary>
        /// Reduces the maximum follow position difference when using smoothing.
        /// </summary>
        /// <remarks>
        /// Fractional amount of how close the smoothed position should remain to the position of the Interactor when using smoothing.
        /// The value ranges from 0 meaning no bias in the smoothed follow distance,
        /// to 1 meaning effectively no smoothing at all.
        /// </remarks>
        /// <seealso cref="smoothPosition"/>
        /// <seealso cref="smoothPositionAmount"/>
        public float tightenPosition
        {
            get => m_TightenPosition;
            set => m_TightenPosition = value;
        }

        [SerializeField]
        bool m_TrackRotation = true;

        /// <summary>
        /// Whether this object should follow the rotation of the Interactor when selected.
        /// </summary>
        /// <seealso cref="trackPosition"/>
        public bool trackRotation
        {
            get => m_TrackRotation;
            set => m_TrackRotation = value;
        }

        [SerializeField]
        bool m_SmoothRotation;

        /// <summary>
        /// Apply smoothing while following the rotation of the Interactor when selected.
        /// </summary>
        /// <seealso cref="smoothRotationAmount"/>
        /// <seealso cref="tightenRotation"/>
        public bool smoothRotation
        {
            get => m_SmoothRotation;
            set => m_SmoothRotation = value;
        }

        [SerializeField, Range(0f, 20f)]
        float m_SmoothRotationAmount = k_DefaultSmoothingAmount;

        /// <summary>
        /// Scale factor for how much smoothing is applied while following the rotation of the Interactor when selected.
        /// The larger the value, the closer this object will remain to the rotation of the Interactor.
        /// </summary>
        /// <seealso cref="smoothRotation"/>
        /// <seealso cref="tightenRotation"/>
        public float smoothRotationAmount
        {
            get => m_SmoothRotationAmount;
            set => m_SmoothRotationAmount = value;
        }

        [SerializeField, Range(0f, 1f)]
        float m_TightenRotation = k_DefaultTighteningAmount;

        /// <summary>
        /// Reduces the maximum follow rotation difference when using smoothing.
        /// </summary>
        /// <remarks>
        /// Fractional amount of how close the smoothed rotation should remain to the rotation of the Interactor when using smoothing.
        /// The value ranges from 0 meaning no bias in the smoothed follow rotation,
        /// to 1 meaning effectively no smoothing at all.
        /// </remarks>
        /// <seealso cref="smoothRotation"/>
        /// <seealso cref="smoothRotationAmount"/>
        public float tightenRotation
        {
            get => m_TightenRotation;
            set => m_TightenRotation = value;
        }

        [SerializeField]
        AttachPointCompatibilityMode m_AttachPointCompatibilityMode = AttachPointCompatibilityMode.Default;

        /// <summary>
        /// Controls the method used when calculating the target position of the object.
        /// Use <see cref="AttachPointCompatibilityMode.Default"/> for consistent attach points
        /// between all <see cref="XRBaseInteractable.MovementType"/> values.
        /// Marked for deprecation, this property will be removed in a future version.
        /// </summary>
        /// <remarks>
        /// This is a backwards compatibility option in order to keep the old, incorrect method
        /// of calculating the attach point. Projects that already accounted for the difference
        /// can use the Legacy option to maintain the same attach positioning from older versions
        /// without needing to modify the Attach Transform position.
        /// </remarks>
        /// <seealso cref="AttachPointCompatibilityMode"/>
        public AttachPointCompatibilityMode attachPointCompatibilityMode
        {
            get => m_AttachPointCompatibilityMode;
            set => m_AttachPointCompatibilityMode = value;
        }

        // Point we are attaching to on this Interactable (in Interactor's attach coordinate space)
        Vector3 m_InteractorLocalPosition;
        Quaternion m_InteractorLocalRotation;

        // Point we are moving towards each frame (eventually will be at Interactor's attach point)
        Vector3 m_TargetWorldPosition;
        Quaternion m_TargetWorldRotation;

        float m_CurrentAttachEaseTime;
        MovementType m_CurrentMovementType;

        Transform m_OriginalSceneParent;

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            m_CurrentMovementType = m_MovementType;
        }

        /// <inheritdoc />
        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);

            switch (updatePhase)
            {
                // During Dynamic update we want to perform any Transform-based manipulation (e.g., Instantaneous).
                // case XRInteractionUpdateOrder.UpdatePhase.Dynamic:
                //     if (isSelected)
                //     {
                //         var interactor = interactorsSelecting[0];
                //         // Legacy does not support the Attach Transform position changing after being grabbed
                //         // due to depending on the Physics update to compute the world center of mass.
                //         if (m_AttachPointCompatibilityMode == AttachPointCompatibilityMode.Default)
                //             UpdateInteractorLocalPose(interactor);
                //         UpdateTarget(interactor, Time.deltaTime);
                //         //SmoothVelocityUpdate(interactor, Time.deltaTime);

                //         if (m_CurrentMovementType == MovementType.Instantaneous)
                //             PerformInstantaneousUpdate(updatePhase);
                //     }

                //     break;

                // During OnBeforeRender we want to perform any last minute Transform position changes before rendering (e.g., Instantaneous).
                case XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender:
                    if (isSelected)
                    {
                        var interactor = interactorsSelecting[0];
                        // Legacy does not support the Attach Transform position changing after being grabbed
                        // due to depending on the Physics update to compute the world center of mass.
                        if (m_AttachPointCompatibilityMode == AttachPointCompatibilityMode.Default)
                            UpdateInteractorLocalPose(interactor);
                        UpdateTarget(interactor, Time.deltaTime);

                        if (m_CurrentMovementType == MovementType.Instantaneous)
                            PerformInstantaneousUpdate(updatePhase);
                    }

                    break;
            }
        }

        /// <inheritdoc />
        public override Transform GetAttachTransform(IXRInteractor interactor)
        {
            return m_AttachTransform != null ? m_AttachTransform : base.GetAttachTransform(interactor);
        }

        /// <summary>
        /// Calculates the world position to place this object at when selected.
        /// </summary>
        /// <param name="interactor">Interactor that is initiating the selection.</param>
        /// <returns>Returns the attach position in world space.</returns>
        Vector3 GetWorldAttachPosition(IXRInteractor interactor)
        {
            var interactorAttachTransform = interactor.GetAttachTransform(this);

            if (!m_TrackRotation)
            {
                var thisAttachTransform = GetAttachTransform(interactor);
                return interactorAttachTransform.position + thisAttachTransform.TransformDirection(m_InteractorLocalPosition);
            }

            return interactorAttachTransform.position + interactorAttachTransform.rotation * m_InteractorLocalPosition;
        }

        /// <summary>
        /// Calculates the world rotation to place this object at when selected.
        /// </summary>
        /// <param name="interactor">Interactor that is initiating the selection.</param>
        /// <returns>Returns the attach rotation in world space.</returns>
        Quaternion GetWorldAttachRotation(IXRInteractor interactor)
        {
            if (!m_TrackRotation)
                return m_TargetWorldRotation;

            var interactorAttachTransform = interactor.GetAttachTransform(this);
            return interactorAttachTransform.rotation * m_InteractorLocalRotation;
        }

        void UpdateTarget(IXRInteractor interactor, float timeDelta)
        {
            // Compute the unsmoothed target world position and rotation
            var rawTargetWorldPosition = GetWorldAttachPosition(interactor);
            var rawTargetWorldRotation = GetWorldAttachRotation(interactor);

            // Apply smoothing (if configured)
            if (m_AttachEaseInTime > 0f && m_CurrentAttachEaseTime <= m_AttachEaseInTime)
            {
                var easePercent = m_CurrentAttachEaseTime / m_AttachEaseInTime;
                m_TargetWorldPosition = Vector3.Lerp(m_TargetWorldPosition, rawTargetWorldPosition, easePercent);
                m_TargetWorldRotation = Quaternion.Slerp(m_TargetWorldRotation, rawTargetWorldRotation, easePercent);
                m_CurrentAttachEaseTime += timeDelta;
            }
            else
            {
                if (m_SmoothPosition)
                {
                    m_TargetWorldPosition = Vector3.Lerp(m_TargetWorldPosition, rawTargetWorldPosition, m_SmoothPositionAmount * timeDelta);
                    m_TargetWorldPosition = Vector3.Lerp(m_TargetWorldPosition, rawTargetWorldPosition, m_TightenPosition);
                }
                else
                {
                    m_TargetWorldPosition = rawTargetWorldPosition;
                }

                if (m_SmoothRotation)
                {
                    m_TargetWorldRotation = Quaternion.Slerp(m_TargetWorldRotation, rawTargetWorldRotation, m_SmoothRotationAmount * timeDelta);
                    m_TargetWorldRotation = Quaternion.Slerp(m_TargetWorldRotation, rawTargetWorldRotation, m_TightenRotation);
                }
                else
                {
                    m_TargetWorldRotation = rawTargetWorldRotation;
                }
            }
        }

        void PerformInstantaneousUpdate(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic ||
                updatePhase == XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender)
            {
                if (m_TrackPosition)
                {
                    transform.position = m_TargetWorldPosition;
                }

                if (m_TrackRotation)
                {
                    transform.rotation = m_TargetWorldRotation;
                }
            }
        }

        void UpdateInteractorLocalPose(IXRInteractor interactor)
        {
            // In order to move the Interactable to the Interactor we need to
            // calculate the Interactable attach point in the coordinate system of the
            // Interactor's attach point.
            var thisAttachTransform = GetAttachTransform(interactor);
            var attachOffset = transform.position - thisAttachTransform.position;
            var localAttachOffset = thisAttachTransform.InverseTransformDirection(attachOffset);

            m_InteractorLocalPosition = localAttachOffset;
            m_InteractorLocalRotation = Quaternion.Inverse(Quaternion.Inverse(transform.rotation) * thisAttachTransform.rotation);
        }

        void UpdateCurrentMovementType()
        {
            // Special case where the interactor will override this objects movement type (used for Sockets and other absolute interactors)
            var interactor = interactorsSelecting[0];
            var baseInteractor = interactor as XRBaseInteractor;
            m_CurrentMovementType = (baseInteractor != null ? baseInteractor.selectedInteractableMovementTypeOverride : null) ?? m_MovementType;
        }

        /// <inheritdoc />
        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            base.OnSelectEntering(args);
            Grab();
        }

        /// <inheritdoc />
        protected override void OnSelectExiting(SelectExitEventArgs args)
        {
            int toto = 1;
            toto += 1;
            // TODO add bool
            //base.OnSelectExiting(args);
        }

        /// <summary>
        /// Updates the state of the object due to being grabbed.
        /// Automatically called when entering the Select state.
        /// </summary>
        /// <seealso cref="Drop"/>
        protected virtual void Grab()
        {
            var thisTransform = transform;
            m_OriginalSceneParent = thisTransform.parent;
            thisTransform.SetParent(null);

            UpdateCurrentMovementType();

            // Initialize target pose for easing and smoothing
            m_TargetWorldPosition = thisTransform.position;
            m_TargetWorldRotation = thisTransform.rotation;
            m_CurrentAttachEaseTime = 0f;

            var interactor = interactorsSelecting[0];
            UpdateInteractorLocalPose(interactor);
        }
    }
}
