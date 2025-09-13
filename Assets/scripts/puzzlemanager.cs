using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PuzzleManager : MonoBehaviour
{
    [Header("Highlight Settings")]
    public Material highlightMaterial;
    private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();

    [Header("Setup")]
    public Transform puzzlePiecesParent;
    public EyeController[] eyes;
    public XRSocketInteractor[] sockets;

    [Header("Reward Settings")]
    public AudioClip rewardSound;
    public int coinAmount = 1;
    public Text coinText;

    [Header("Success Message UI")]
    public Text messageText;

    private List<XRGrabInteractable> pieces = new List<XRGrabInteractable>();
    private XRGrabInteractable currentPiece;
    private AudioSource audioSource;
    private int coins = 0;
    private int totalCoins = 0;
    private int currentSceneIndex;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0); // Load saved progress

        // Clear or hide the message at the start
        if (messageText != null)
        {
            messageText.text = "";
            messageText.gameObject.SetActive(false);
        }

        foreach (Transform child in puzzlePiecesParent)
        {
            XRGrabInteractable grab = child.GetComponent<XRGrabInteractable>();
            if (grab != null)
            {
                SetPieceActive(grab, false);
                pieces.Add(grab);
            }
        }

        foreach (var socket in sockets)
        {
            socket.selectEntered.AddListener(OnPiecePlaced);
        }

        UpdateCoinUI();
        SelectNextPiece();

        Debug.Log("Puzzle Manager initialized. Current scene index: " + currentSceneIndex);
    }

    void HighlightPiece()
    {
        if (currentPiece == null) return;

        Renderer rend = currentPiece.GetComponent<Renderer>();
        if (rend == null) return;

        if (!originalMaterials.ContainsKey(rend))
            originalMaterials[rend] = rend.sharedMaterial;

        rend.material = highlightMaterial;
        Invoke(nameof(RemoveHighlight), 2f);
    }

    void RemoveHighlight()
    {
        if (currentPiece == null) return;

        Renderer rend = currentPiece.GetComponent<Renderer>();
        if (rend != null && originalMaterials.ContainsKey(rend))
            rend.material = originalMaterials[rend];
    }

    void SelectNextPiece()
    {
        if (pieces.Count == 0)
        {
            Debug.Log("All pieces placed!");
            return;
        }

        int index = Random.Range(0, pieces.Count);
        currentPiece = pieces[index];

        foreach (var piece in pieces)
            SetPieceActive(piece, piece == currentPiece);

        CancelInvoke(nameof(HighlightPiece));
        CancelInvoke(nameof(RemoveHighlight));
        HighlightPiece();
        InvokeRepeating(nameof(HighlightPiece), 5f, 10f);

        foreach (var eye in eyes)
            eye.SetTarget(currentPiece.transform);
    }

    void OnPiecePlaced(SelectEnterEventArgs args)
    {
        XRGrabInteractable placedPiece = args.interactableObject as XRGrabInteractable;
        if (placedPiece == null) return;

        if (placedPiece == currentPiece)
        {
            CancelInvoke(nameof(HighlightPiece));
            CancelInvoke(nameof(RemoveHighlight));
            RemoveHighlight();

            SetPieceActive(placedPiece, false);
            pieces.Remove(currentPiece);

            foreach (var eye in eyes)
                eye.SetTarget(null);

            if (rewardSound != null)
                audioSource.PlayOneShot(rewardSound);

            coins += coinAmount;
            totalCoins += coinAmount;
            PlayerPrefs.SetInt("TotalCoins", totalCoins); // Save progress

            UpdateCoinUI();

            if (coins >= 9)
            {
                ShowMessage("Puzzle Completed!");
                Invoke(nameof(GoToNextScene), 2f);
            }
            else
            {
                Invoke(nameof(SelectNextPiece), 1f);
            }
        }
    }

    void UpdateCoinUI()
    {
        if (coinText != null)
            coinText.text = "Coins: " + coins;
    }

    void ShowMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
            messageText.gameObject.SetActive(true);
        }
    }

    void HideMessage()
    {
        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }

    void GoToNextScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "vig")
        {
            Debug.Log("Loading Scene2...");
            SceneManager.LoadScene("war");
        }
        else if (currentSceneIndex == 2)
        {
            if (totalCoins >= 18)
            {
                ShowMessage("Puzzle successfully completed! Game over.");
                Debug.Log("Game over condition reached.");
            }
        }
    }

    void SetPieceActive(XRGrabInteractable piece, bool active)
    {
        piece.enabled = active;

        Collider col = piece.GetComponent<Collider>();
        if (col != null)
            col.enabled = active;

        Rigidbody rb = piece.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = !active;
            if (!active)
                rb.constraints = RigidbodyConstraints.FreezeAll;
            else
                rb.constraints = RigidbodyConstraints.None;
        }
    }
}
