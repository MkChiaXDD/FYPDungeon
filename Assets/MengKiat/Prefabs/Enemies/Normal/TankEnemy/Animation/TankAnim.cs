using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankAnim : MonoBehaviour
{
    [SerializeField] private Animator _anim;
    
    public void PlayAttackAnim()
    {
        _anim.SetTrigger("Attack");
    }

    public void PlayWalkingAnimation(bool isWalking, bool fasterWalk)
    {
        _anim.SetBool("IsChasing", isWalking);
        _anim.SetBool("IsFasterWalk", fasterWalk);
    }

    public void PlayCarryBomber()
    {
        _anim.SetTrigger("CarryBomber");
    }

    public void PlayThrowBomber()
    {
        _anim.SetTrigger("ThrowBomber");
    }
}
