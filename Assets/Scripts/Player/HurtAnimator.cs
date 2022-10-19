using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtAnimator : MonoBehaviour
{
    private Fighter _fighter;
    private Animator _animator;

    [SerializeField] private string _idleName;
    [SerializeField] private string _dazeName;
    [SerializeField] private string _launchName;
    [SerializeField] private string _landName;

    private void Awake()
    {
        _fighter = GetComponent<Fighter>();
        _animator = GetComponent<Animator>();
    }


    public void Daze()
    {
        _animator.Play(_dazeName, -1);
    }

    public IEnumerator PlayDaze()
    {
        _animator.Play(_dazeName, -1);
        yield return new WaitForSeconds(0.267f);

        _animator.Play(_idleName);
        yield break;
    }

    public IEnumerator PlayLaunch()
    {
        _animator.Play(_launchName, -1);
        yield return null;
        yield return new WaitForFixedUpdate();
        yield return new WaitUntil(() => _fighter.MovementController.CollisionData.y.isNegativeHit);

        _animator.Play(_landName);
        yield return new WaitUntil(() => _fighter.InputManager.Actions["Move"].disabledCount == 0);

        _animator.Play(_idleName);
        yield break;
    }
}
