using UnityEngine;

public class CreditCardManager : MonoBehaviour
{
    [Header("Credit Card Settings")]
    public GameObject creditCardPrefab;
    public KeyCode takeCardKey = KeyCode.F;

    [Header("Position Offset")]
    public Vector3 cardOffset = new Vector3(0.416f, -0.038f, 0.019f);
    public Vector3 cardScale = new Vector3(0.1f, 0.1f, 0.1f);

    [Header("References")]
    public ItemHolder itemHolder;
    public QuestManager questManager;
    public int questIndex = 0;
    public int objectiveIndex = 2;

    [Header("UI Prompt")]
    public GameObject promptUI;

    private bool cardTaken = false;
    private GameObject currentCard;

    void Start()
    {
        if (itemHolder == null)
        {
            itemHolder = FindObjectOfType<ItemHolder>();
        }

        if (questManager == null)
        {
            questManager = FindObjectOfType<QuestManager>();
        }

        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }

    void Update()
    {
        GameObject wallet = itemHolder.GetHeldObject("Wallet");

        if (wallet != null)
        {
            if (promptUI != null)
            {
                promptUI.SetActive(true);
            }

            if (Input.GetKeyDown(takeCardKey))
            {
                if (!cardTaken)
                {
                    TakeCreditCard();
                }
                else
                {
                    ReturnCreditCard();
                }
            }
        }
        else if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }

    void TakeCreditCard()
    {
        cardTaken = true;

        Debug.Log("[CreditCard] Mengambil kartu dari wallet!");

        if (creditCardPrefab != null && itemHolder != null)
        {
            Vector3 spawnPos = itemHolder.holdPoint.position;
            currentCard = Instantiate(creditCardPrefab, spawnPos, Quaternion.identity);
            currentCard.name = "CreditCard";
            currentCard.transform.localScale = cardScale;

            itemHolder.HoldObject(currentCard, cardOffset);

            Debug.Log("[CreditCard] Kartu keluar! Press F lagi untuk simpan.");
        }
    }

    void ReturnCreditCard()
    {
        cardTaken = false;

        Debug.Log("[CreditCard] Menyimpan kartu ke wallet!");

        if (currentCard != null && itemHolder != null)
        {
            itemHolder.DropObject(currentCard);
            Destroy(currentCard);
            currentCard = null;

            Debug.Log("[CreditCard] Kartu disimpan kembali!");
        }
    }

    // TAMBAHAN BARU - Fungsi untuk reset state dari luar (ItemHolder)
    public void ResetCardState()
    {
        cardTaken = false;
        if (currentCard != null)
        {
            Destroy(currentCard);
            currentCard = null;
        }
        Debug.Log("[CreditCardManager] Card state reset");
    }
}
