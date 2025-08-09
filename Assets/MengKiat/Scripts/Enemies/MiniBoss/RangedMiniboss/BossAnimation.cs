using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAnimation : MonoBehaviour
{
    [SerializeField] Animator _animator;

    public void PlaySpinAttack()
    {
        _animator.SetTrigger("TurnAttack");
    }
}
