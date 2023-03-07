using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoundTimer : MonoBehaviour
{
    [SerializeField] private GameObject _borderTimer;
    private float _roundTime;
    [Tooltip("The amount of time until the favor meter starts shrinking.")]
    [SerializeField] private float _secondsBeforeShrink;
    [Tooltip("How quickly the favor meter shrinks in units/sec.")]
    [SerializeField] private float _shrinkSpeed;
    [Tooltip("The minimum size of the favor meter in favor units.")]
    [SerializeField] private float _minFavorMeter;

    [Header("Images")]
    [SerializeField] private Image _topLeft;
    [SerializeField] private Image _topRight;
    [SerializeField] private Image _left;
    [SerializeField] private Image _right;
    [SerializeField] private Image _bottomLeft;
    [SerializeField] private Image _bottomRight;
    [SerializeField] private Image _leftNodePivot;
    [SerializeField] private Image _rightNodePivot;

    [SerializeField] private float _uBorderEdgeX;

    [Tooltip("What percentage of total time shouldd be spent on the sides?")]
    [SerializeField] [Range(0f, 1f)] private float _sidePercentage;

    [SerializeField] private Image _lock;
    [SerializeField] private Sprite _brokenLockSprite;
    [SerializeField] private float _lockDisappearTime;
    [SerializeField] private float _lockFallSpeed;

    public delegate void StartShrink();
    public StartShrink onStartShrink;

    private void Awake()
    {
        Services.RoundTimer = this;
    }

    private void Start()
    {
        onStartShrink += BreakLock;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        onStartShrink -= BreakLock;
    }

    public void StartTimer()
    {
        StartCoroutine(Timer());
    }

    public IEnumerator Timer()
    {
        StartCoroutine(BorderTimer());
        yield return new WaitForSeconds(_secondsBeforeShrink);

        onStartShrink?.Invoke();
        while (Services.FavorManager.MaxFavor > _minFavorMeter)
        {
            Services.FavorManager.ResizeFavorMeter(-_shrinkSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        yield break;
    }

    private void BreakLock()
    {
        _lock.sprite = _brokenLockSprite;
        StartCoroutine(LockFade());

        IEnumerator LockFade()
        {
            float timer = 0f;
            while (timer <= _lockDisappearTime)
            {
                timer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
                Color newLockColor = _lock.color;
                newLockColor.a = Mathf.Lerp(1f, 0f, timer / _lockDisappearTime);
                _lock.color = newLockColor;
                Vector2 newLockPosition = new Vector2(_lock.rectTransform.anchoredPosition.x, _lock.rectTransform.anchoredPosition.y - _lockFallSpeed * Time.fixedDeltaTime);
                _lock.rectTransform.anchoredPosition = newLockPosition;
            }
            _lock.gameObject.SetActive(false);
            yield break;
        }
    }

    private IEnumerator BorderTimer()
    {
        float sideTime = _secondsBeforeShrink * _sidePercentage;
        float topAndBottomTime = _secondsBeforeShrink - sideTime;
        float topTime = topAndBottomTime / 2f;
        float bottomTime = topAndBottomTime / 2f;
        float timer = 0f;
        while (timer <= topTime)
        {
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
            float fill = Mathf.Lerp(1f, 0f, timer / topTime);
            _topLeft.fillAmount = fill;
            _topRight.fillAmount = fill;

            float uNodeX = Mathf.Lerp(0f, _uBorderEdgeX, timer / topTime);
            Vector3 leftNodePosition = new Vector3(-uNodeX, _leftNodePivot.rectTransform.anchoredPosition.y, 0f);
            _leftNodePivot.rectTransform.anchoredPosition = leftNodePosition;
            Vector3 rightNodePosition = new Vector3(uNodeX, _rightNodePivot.rectTransform.anchoredPosition.y, 0f);
            _rightNodePivot.rectTransform.anchoredPosition = rightNodePosition;
        }

        timer = 0f;
        while (timer <= sideTime)
        {
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
            float fill = Mathf.Lerp(1f, 0f, timer / sideTime);
            _left.fillAmount = fill;
            _right.fillAmount = fill;

            float uNodeRotationZ = Mathf.Lerp(0f, 180f, timer / sideTime);
            Quaternion leftRotation = Quaternion.Euler(0f, 0f, uNodeRotationZ);
            _leftNodePivot.rectTransform.rotation = leftRotation;
            Quaternion rightRotation = Quaternion.Euler(0f, 0f, -uNodeRotationZ);
            _rightNodePivot.rectTransform.rotation = rightRotation;
        }

        timer = 0f;
        while (timer <= bottomTime)
        {
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
            float fill = Mathf.Lerp(1f, 0f, timer / bottomTime);
            _bottomLeft.fillAmount = fill;
            _bottomRight.fillAmount = fill;

            float uNodeX = Mathf.Lerp(_uBorderEdgeX, 0f, timer / bottomTime);
            Vector3 leftNodePosition = new Vector3(-uNodeX, _leftNodePivot.rectTransform.anchoredPosition.y, 0f);
            _leftNodePivot.rectTransform.anchoredPosition = leftNodePosition;
            Vector3 rightNodePosition = new Vector3(uNodeX, _rightNodePivot.rectTransform.anchoredPosition.y, 0f);
            _rightNodePivot.rectTransform.anchoredPosition = rightNodePosition;
        }

        _borderTimer.SetActive(false);
        yield break;
    }

    public void StopTimer(Dictionary<string, object> data)
    {
        StopAllCoroutines();
    }
}
