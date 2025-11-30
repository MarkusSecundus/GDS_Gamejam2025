using DG.Tweening;
using MarkusSecundus.Utils.Primitives;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopBarController : MonoBehaviour
{
    [SerializeField] TMP_Text _text;
    [SerializeField] Image _image;

    [SerializeField] float _showBuildup_seconds = 1f;
    [SerializeField] float _showSustain_seconds;
    [SerializeField] float _showFade_seconds;

    Vector3 _originalPos;
	private void Start()
	{
        _originalPos = _text.transform.position;
        _text.color = new Color(0, 0, 0, 0);
		_image.color = new Color(0, 0, 0, 0);
	}


    Tween _runningTween, _runningTween2;
	public void ShowText(string toShow)
    {
        WhenNoTweenRunning(() =>
        {
            _text.text = toShow;
            _text.color = new Color(0,0,0,0);
            _image.color = new Color(0, 0, 0, 0);
            _runningTween = _text.DOColor(Color.white, _showBuildup_seconds);
            _runningTween2 = _image.DOColor(Color.white, _showBuildup_seconds);
            _runningTween.OnComplete(() =>
            {
                _runningTween = _text.DOColor(new Color(0, 0, 0, 0), _showFade_seconds).SetDelay(_showSustain_seconds);
                _runningTween2 = _image.DOColor(new Color(0, 0, 0, 0), _showFade_seconds).SetDelay(_showSustain_seconds);
            });
        });
    }

    void WhenNoTweenRunning(TweenCallback toDo)
    {
        if(_runningTween != null && _runningTween.IsPlaying())
        {
            _runningTween.Kill();
            _runningTween2?.Kill();
            _runningTween = _text.DOColor(new Color(0, 0, 0, 0), _showFade_seconds).OnComplete(toDo);
            _runningTween2 = _image.DOColor(new Color(0, 0, 0, 0), _showFade_seconds);
        }
        else
        {
            toDo();
        }
    }
}
