using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAnimation : MonoBehaviour
{
    [SerializeField] public Animator _animator;

    public void PlaySpinAttack()
    {
        _animator.SetTrigger("TurnAttack");
    }

    public void PlayShootAttack()
    {
        _animator.SetTrigger("Shoot");
    }

    public void PlayDeadAnim()
    {
        _animator.SetTrigger("Die");
    }
}
