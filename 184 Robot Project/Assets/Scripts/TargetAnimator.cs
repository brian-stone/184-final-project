using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetAnimator : MonoBehaviour {
    #region Editor Variables
    [SerializeField]
    [Tooltip("The animator controller for the right hand target")]
    private Animator m_RightHand;

    [SerializeField]
    [Tooltip("The animator controller for the right foot target")]
    private Animator m_RightFoot;

    [SerializeField]
    [Tooltip("The animator controller for the left hand target")]
    private Animator m_LeftHand;

    [SerializeField]
    [Tooltip("The animator controller for the left foot target")]
    private Animator m_LeftFoot;
    #endregion

    #region Private Variables

    #endregion

    private void Awake() {
        m_RightHand.SetBool("Idle", true);
        m_RightFoot.SetBool("Idle", true);
        m_LeftHand.SetBool("Idle", true);
        m_LeftHand.SetBool("Idle", true);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            Reset();
        }

        else if (Input.GetKeyDown(KeyCode.Alpha1)) {
            Wave();
        }

        else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            Debug.Log("Test");
            Shake();
        }
    }

    #region Animations
    private void Reset() {
        m_RightHand.SetBool("Idle", true);
        m_RightFoot.SetBool("Idle", true);
        m_LeftHand.SetBool("Idle", true);
        m_LeftHand.SetBool("Idle", true);
        m_RightHand.SetBool("Wave", false);
        m_RightHand.SetBool("Shake", false);
    }

    private void Wave() {
        m_RightHand.SetBool("Wave", true);
        m_RightHand.SetBool("Idle", false);
    }

    private void Shake() {
        m_RightHand.SetBool("Shake", true);
        m_RightHand.SetBool("Idle", false);
    }
    #endregion
}
