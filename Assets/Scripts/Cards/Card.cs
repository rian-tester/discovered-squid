using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FirstRound
{
    /// <summary>
    /// Represents a single card in the memory game
    /// Handles visual representation, state, and interactions
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class Card : MonoBehaviour
    {
        [Header("Visual Reference")]
        [SerializeField] private Image cardImage;
        [SerializeField] private Sprite backSprite;

        [Header("Animation Settings")]
        [SerializeField] private float flipDuration = 0.3f;
        [SerializeField] private float matchDisappearDuration = 0.5f;

        // Card data and state
        private CardData cardData;
        private CardState currentState = CardState.FaceDown;
        private Button button;

        private int gridIndex;
        
        // Coroutine management
        private Coroutine currentAnimation;

        // Events
        public event Action<Card> OnCardClicked;

        #region Initialization
        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(HandleClick);
        }

        /// <summary>
        /// Initialize the card with data and position
        /// </summary>
        /// <param name="data"></param>
        /// <param name="backSprite"></param>
        /// <param name="index"></param>
        public void Initialize(CardData data, Sprite backSprite, int index)
        {
            this.cardData = data;
            this.backSprite = backSprite;
            this.gridIndex = index;

            StopCurrentAnimation();

            // Start with back side visible
            SetState(CardState.FaceDown);
            cardImage.sprite = backSprite;

            // Reset transform
            transform.localScale = Vector3.one;
            cardImage.color = Color.white;
            gameObject.SetActive(true);
        }
        #endregion

        #region State Management
        public void SetState(CardState newState)
        {
            currentState = newState;

            button.interactable = currentState == CardState.FaceDown;
        }

        public CardState GetState() => currentState;

        public bool IsInteractable() => currentState == CardState.FaceDown;
        #endregion

        #region UserInteraction
        private void HandleClick()
        {
            if (currentState == CardState.FaceDown)
            {
                OnCardClicked?.Invoke(this);
            }
        }
        #endregion

        #region Animation Management
        /// <summary>
        /// Stops any currently running animation coroutine
        /// </summary>
        private void StopCurrentAnimation()
        {
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
                currentAnimation = null;
            }
        }
        #endregion

        #region Visual Updates
        /// <summary>
        /// Flip the card to show its face
        /// </summary>
        /// <param name="onComplete"></param>
        public void FlipToFront(Action onComplete = null)
        {
            if (currentState != CardState.FaceDown)
            {
                onComplete?.Invoke();
                return;
            }

            StopCurrentAnimation();
            currentAnimation = StartCoroutine(FlipCoroutine(true, onComplete));
        }

        /// <summary>
        /// Flip the card to show its back
        /// </summary>
        /// <param name="onComplete"></param>
        public void FlipToBack(Action onComplete = null)
        {
            if (currentState != CardState.FaceUp)
            {
                onComplete?.Invoke();
                return;
            }

            StopCurrentAnimation();
            currentAnimation = StartCoroutine(FlipCoroutine(false, onComplete));
        }

        private IEnumerator FlipCoroutine(bool toFront, Action onComplete)
        {
            SetState(CardState.Flipping);

            float elapsed = 0f;
            float halfDuration = flipDuration / 2f;

            // First half : scale down to 0 on X axis
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                float scale = Mathf.Lerp(1f, 0f, t);
                transform.localScale = new Vector3(scale, 1f, 1f);
                yield return null;
            }

            // Change sprite at the middle of animation
            cardImage.sprite = toFront ? cardData.frontSprite : backSprite;

            // Second half : scale back to 1
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                float scale = Mathf.Lerp(0f, 1f, t);
                transform.localScale = new Vector3(scale, 1f, 1f);
                yield return null;
            }

            transform.localScale = Vector3.one;

            // Update state
            SetState(toFront ? CardState.FaceUp : CardState.FaceDown);

            currentAnimation = null;
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// Play matched animation and make card disappear
        /// </summary>
        public void PlayMatchedAnimation(Action onComplete = null)
        {
            StopCurrentAnimation();
            currentAnimation = StartCoroutine(MatchedCoroutine(onComplete));
        }

        private IEnumerator MatchedCoroutine(Action onComplete)
        {
            SetState(CardState.Matched);

            float elapsed = 0f;
            Vector3 startScale = transform.localScale;
            Color startColor = cardImage.color;

            while (elapsed < matchDisappearDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / matchDisappearDuration;

                // Scale down and fade out
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                cardImage.color = Color.Lerp(startColor, new Color(1, 1, 1, 0), t);

                yield return null;
            }

            SetState(CardState.Disappeared);
            gameObject.SetActive(false);

            currentAnimation = null;
            onComplete?.Invoke();
        }
        
        #endregion
        
        #region Getters
        public int GetCardID() => cardData.id;
    
        public int GetGridIndex() => gridIndex;

        public CardData GetCardData() => cardData;
        #endregion

        #region Cleanup
        private void OnDisable()
        {
            StopCurrentAnimation();
        }

        private void OnDestroy()
        {
            StopCurrentAnimation();
            button.onClick.RemoveListener(HandleClick);
        }
        #endregion
    }
}