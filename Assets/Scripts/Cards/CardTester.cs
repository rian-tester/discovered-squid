using UnityEngine;
using FirstRound;
using System.Security;

/// <summary>
/// Simple test script to manually test Card functionality
/// </summary>
public class CardTester : MonoBehaviour
{
    [Header("Test Setup")]
    [SerializeField] private Card testCard;
    [SerializeField] private CardData testCardData; 
    [SerializeField] private Sprite testBackSprite;

    private void Start()
    {
        // Initialize the test card
        if (testCard != null && testCardData != null)
        {
            testCard.Initialize(testCardData, testBackSprite, 0);

            // Subscribe to click event
            testCard.OnCardClicked += OnCardClicked;

            Debug.Log("Card initialized and ready for testing!");
        }
        else
        {
            Debug.LogError("Please assign testCard, testCardData, and testBackSprite in the inspector!");
        }
    }

    private void OnCardClicked(Card card)
    {
        Debug.Log("Card clicked! ID: " + card.GetCardID());

        // Test flip to front
        card.FlipToFront(() =>
        {
            Debug.Log("Flip to front complete!");

            // After 1 second, flip back or match
            Invoke(nameof(TestFlipBackOrMatch), 1f);
        });
    }

    private void TestFlipBackOrMatch()
    {
        // Randomly choose to flip back or play matched animation
        float randomValue = Random.Range(0, 1f);
        Debug.Log($"Random value for testing : {randomValue} ");
        if (randomValue > 0.5f)
        {
            Debug.Log("Testing flip back...");
            testCard.FlipToBack(() => Debug.Log("Flip to back complete!"));
        }
        else
        {
            Debug.Log("Testing match animation...");
            testCard.PlayMatchedAnimation(() => Debug.Log("Match animation complete!"));
        }
    }

    private void OnDestroy()
    {
        if (testCard != null)
        {
            testCard.OnCardClicked -= OnCardClicked;
        }
    }

    // Manual testing via keyboard
    private void Update()
    {
        if (testCard == null) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Manual flip test (Space pressed)");
            if (testCard.GetState() == CardState.FaceDown)
            {
                testCard.FlipToFront();
            }
            else if (testCard.GetState() == CardState.FaceUp)
            {
                testCard.FlipToBack();
            }
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("Manual match test (M pressed)");
            testCard.PlayMatchedAnimation();
        }
    }
}