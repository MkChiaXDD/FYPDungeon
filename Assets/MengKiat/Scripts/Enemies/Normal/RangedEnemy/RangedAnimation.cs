using UnityEngine;

public class RangedAnimation : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    public void PlayAttack()
    {
        _animator.SetTrigger("Shoot");
    }
}
