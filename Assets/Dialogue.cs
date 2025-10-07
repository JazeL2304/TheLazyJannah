using System.Collections;
using UnityEngine;
using TMPro;

public class Dialogue : MonoBehaviour
{
    // VARIABEL BARU UNTUK NAMA KARAKTER
    public TextMeshProUGUI nameTextComponent; // <-- BARU

    // VARIABEL LAMA UNTUK TEKS DIALOG
    public TextMeshProUGUI dialogueTextComponent; // Diganti namanya agar lebih jelas

    // STRUKTUR DATA BARU
    public DialogueLine[] lines; // Menggunakan struct agar bisa menyimpan Nama dan Kalimat

    public float textSpeed;

    private int index;

    [System.Serializable]
    public struct DialogueLine // Definisi struktur untuk menyimpan Nama dan Kalimat
    {
        public string characterName;
        [TextArea(1, 3)]
        public string sentence;
    }

    void Start()
    {
        dialogueTextComponent.text = string.Empty;
        StartDialogue();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Periksa apakah pengetikan sudah selesai
            if (dialogueTextComponent.text == lines[index].sentence)
            {
                NextLine();
            }
            else // Jika belum selesai, paksa selesai
            {
                StopAllCoroutines();
                dialogueTextComponent.text = lines[index].sentence;
            }
        }
    }

    void StartDialogue()
    {
        index = 0;
        // Langsung tampilkan nama, lalu mulai ketik dialog
        nameTextComponent.text = lines[index].characterName; // Tampilkan nama karakter
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        dialogueTextComponent.text = string.Empty;
        // Loop hanya pada kalimat dialog (sentence)
        foreach (char c in lines[index].sentence.ToCharArray())
        {
            dialogueTextComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            // Langsung tampilkan nama baris berikutnya
            nameTextComponent.text = lines[index].characterName;

            StartCoroutine(TypeLine());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}