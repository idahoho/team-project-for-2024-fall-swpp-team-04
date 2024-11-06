using System.Collections;
using UnityEngine;
using TMPro;

public class AmmoManager : MonoBehaviour
{
	public int maxAmmo = 30;
	private int _currentAmmo;
	public TextMeshProUGUI ammoText;

	private bool _isReloading = false;

	void Start()
	{
		_currentAmmo = maxAmmo;
		UpdateAmmoUI();
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0) && !_isReloading)
		{
			Shoot();
		}
	}

	void Shoot()
	{
		if (_currentAmmo > 0)
		{
			_currentAmmo--;
			UpdateAmmoUI();

			if (_currentAmmo == 0)
			{
				StartCoroutine(ReloadAmmo());
			}
		}
	}

	IEnumerator ReloadAmmo()
	{
		_isReloading = true;
		yield return new WaitForSeconds(5);
		_currentAmmo = maxAmmo;
		_isReloading = false;
		UpdateAmmoUI();
	}

	void UpdateAmmoUI()
	{
		ammoText.text = $"{_currentAmmo} / {maxAmmo}";
	}
}
