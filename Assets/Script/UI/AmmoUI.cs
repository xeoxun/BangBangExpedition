using UnityEngine;
using TMPro;

public class AmmoUI : MonoBehaviour
{
    [Header("탄약 UI")]
    [SerializeField] private TMP_Text ammoText;

    public void UpdateAmmo(int currentAmmo, int magazineSize)
    {
        if (ammoText == null)
        {
            return;
        }

        ammoText.text = $"{currentAmmo} / {magazineSize}";
    }

    public void ShowReloading()
    {
        if (ammoText == null)
        {
            return;
        }

        ammoText.text = "장전 중..";
    }
}