using System.Collections;
using UnityEngine;
using TMPro;

public class AmmoManager : MonoBehaviour
{
	public int maxAmmo = 30;
	private int currentAmmo;
	public TextMeshProUGUI ammoText;

	private bool isReloading = false;

	void Start()
	{
		currentAmmo = maxAmmo;
		UpdateAmmoUI();
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0) && !isReloading)
		{
			Shoot();
		}
	}

	void Shoot()
	{
		if (currentAmmo > 0)
		{
			currentAmmo--;
			UpdateAmmoUI();

			if (currentAmmo == 0)
			{
				StartCoroutine(ReloadAmmo());
			}
		}
	}

	IEnumerator ReloadAmmo()
	{
		isReloading = true;
		yield return new WaitForSeconds(5);
		currentAmmo = maxAmmo;
		isReloading = false;
		UpdateAmmoUI();
	}

	void UpdateAmmoUI()
	{
		ammoText.text = $"{currentAmmo} / {maxAmmo}";
	}
}
