using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class VFXCleanUp : MonoBehaviour
{
    private VisualEffect _vfx;
    private float _lifeTime;
    [HideInInspector] public Fighter f;
    private bool isMoveBased;

    void Start()
    {
        _vfx = GetComponent<VisualEffect>();
        // _lifeTime = _vfx.GetFloat("Life");
        // all vfx destroy after one second
        Destroy(gameObject, 1);
        isMoveBased = name == "VFX_DashSmoke(Clone)";
    }

    void Update() {
        if (f)
        {
            _vfx.SetBool("FaceLeft", isMoveBased ? f.MovingDirection == Fighter.Direction.Left : f.FacingDirection == Fighter.Direction.Left);
            ChangeColor();
        }
    }

    void ChangeColor()
    {
        switch (f.PlayerId)
        {
            case 0:
                break;
            case 1:
                _vfx.SetGradient("ParticleGradient", _vfx.GetGradient("P2Gradient"));
                break;
        }
    }
}
