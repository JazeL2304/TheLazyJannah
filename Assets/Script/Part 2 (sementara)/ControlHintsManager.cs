using UnityEngine;

public class ControlHintsManager : MonoBehaviour
{
    public GameObject controlHintsPanel;

    private ItemHolder itemHolder;

    void Start()
    {
        Debug.Log("[ControlHints] START!");

        itemHolder = FindObjectOfType<ItemHolder>();

        if (itemHolder == null)
        {
            Debug.LogError("[ControlHints] ItemHolder NOT FOUND!");
        }
        else
        {
            Debug.Log("[ControlHints] ItemHolder FOUND!");
        }

        if (controlHintsPanel == null)
        {
            Debug.LogError("[ControlHints] controlHintsPanel is NULL!");
        }
        else
        {
            Debug.Log("[ControlHints] controlHintsPanel FOUND: " + controlHintsPanel.name);
            controlHintsPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (itemHolder == null || controlHintsPanel == null) return;

        bool isHolding = itemHolder.IsHoldingWallet();

        if (isHolding && !controlHintsPanel.activeSelf)
        {
            controlHintsPanel.SetActive(true);
            Debug.Log("[ControlHints] SHOWN!");
        }
        else if (!isHolding && controlHintsPanel.activeSelf)
        {
            controlHintsPanel.SetActive(false);
            Debug.Log("[ControlHints] HIDDEN!");
        }
    }
}
