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
    [HideInInspector] public Fighter sender;
    [HideInInspector] public Fighter receiver;

    void Start()
    {
        _vfx = GetComponent<VisualEffect>();
        // _lifeTime = _vfx.GetFloat("Life");
        // all vfx destroy after one second
        Destroy(gameObject, 1);
    }

    void Update() {
        if (sender)
        {
            Debug.Log(_vfx.HasBool("FaceLeft"));
            _vfx.SetBool(Shader.PropertyToID("FaceLeft"), sender.MovingDirection == Fighter.Direction.Left);
            ChangeColor();
        }
    }

    void ChangeColor()
    {
        switch (sender.PlayerId)
        {
            case 0:
                break;
            case 1:
                _vfx.SetGradient("ParticleGradient", _vfx.GetGradient("P2Gradient"));
                break;
        }
    }
}
