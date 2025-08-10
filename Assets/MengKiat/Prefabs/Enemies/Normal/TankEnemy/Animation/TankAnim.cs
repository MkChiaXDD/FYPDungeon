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
}
