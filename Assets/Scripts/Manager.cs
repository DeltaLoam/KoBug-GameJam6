using TMPro;
using UnityEngine;

public class FragmentManager : MonoBehaviour
{
    public static FragmentManager instance;
    public int fragmentCount = 0;
    public TextMeshProUGUI fragmentText;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void AddFragment(int amount)
    {
        fragmentCount += amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (fragmentText != null)
            fragmentText.text = "" + fragmentCount;
    }
}
