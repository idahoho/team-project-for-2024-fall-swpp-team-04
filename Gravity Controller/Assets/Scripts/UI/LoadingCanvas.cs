using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingCanvas : MonoBehaviour
{
	[SerializeField] private float _interval;
	[SerializeField] private int _numDots;
	private Text _text;
	private int _progress = 0;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void StartLoading()
	{
		StartCoroutine(LoadingCoroutine());
	}

	private IEnumerator LoadingCoroutine()
	{
		_text = transform.GetChild(1).GetComponent<Text>();

		while (true)
		{
			_text.text = "Loading" + MultiplyString(".", _progress);
			yield return new WaitForSecondsRealtime(_interval);
			_progress++;
			if(_progress > _numDots)
			{
				_progress = 0;
			}
		}
	}

	private string MultiplyString(string str, int num)
	{
		string ret = "";
		for (int i = 0; i < num; i++)
		{
			ret += str;
		}
		return ret;
	}
}
