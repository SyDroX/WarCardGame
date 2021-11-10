using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    [SerializeField] private List<Card> _cardsDeckPrefabs;
    private List<Card> _cardsDeck;
    private List<Card> _playerCards;
    private List<Card> _opponentCards;
    private List<Card> _playerWarCards;
    private List<Card> _OpponentWarCards;
    
    [SerializeField] private Transform _startCardsRoot;
    [SerializeField] private Transform _playerCardsRoot;
    [SerializeField] private Transform _opponentCardsRoot;
    
    [SerializeField] private Transform _playerMovePosition;
    [SerializeField] private Transform _opponentMovePosition;
    
    [SerializeField] private TMP_Text _playerCardsRemaining;
    [SerializeField] private TMP_Text _opponentCardsRemaining;
    [SerializeField] private TMP_Text _gameMessage;
    
    [SerializeField] private float _cardMovementAnimationDuration;
    [SerializeField] private float _cardRotationAnimationDuration;

    private bool _roundPlaying = false;
    private bool _warHappened = false;
    private bool _cardsDealt = false;
    private bool _gameOver = false;
    
    private IEnumerator Start()
    {
        _cardsDeck = new List<Card>(_cardsDeckPrefabs.Count);
        SpawnCards(_cardsDeckPrefabs);
        List<Card> shuffledDeck = ShuffleCards(_cardsDeck);
        _playerCards = new List<Card>(_cardsDeck.Count / 2);
        _opponentCards = new List<Card>(_cardsDeck.Count / 2);
        _playerWarCards = new List<Card>();
        _OpponentWarCards = new List<Card>();
        
        yield return new WaitForSeconds(2f);
        
        _gameMessage.gameObject.SetActive(false);
        
        yield return StartCoroutine(DealCards(shuffledDeck));
        
        UpdateCardsRemaining();
    }
    
    public void OnPlayRoundButtonClick()
    {
        if (!_cardsDealt || _roundPlaying) return;
        
        _roundPlaying = true;
        PlayRound();
    }

    private void UpdateCardsRemaining()
    {
        _playerCardsRemaining.text = _playerCards.Count.ToString();
        _opponentCardsRemaining.text = _opponentCards.Count.ToString();
    }

    private void SpawnCards(List<Card> cards)
    {
        foreach (Card card in cards)
        {
            GameObject cardInst = Instantiate(card.gameObject, _startCardsRoot);
            _cardsDeck.Add(cardInst.GetComponent<Card>());
        }
    }

    private List<Card> ShuffleCards(List<Card> cards)
    {
        List<Card> shuffledCards = new List<Card>(cards.Count);
        
        while (cards.Count > 0)
        {
            int cardIndex = Random.Range(0, cards.Count - 1);
            Card currentCard = cards[cardIndex];
            shuffledCards.Add(currentCard);
            cards.RemoveAt(cardIndex);
        }

        return shuffledCards;
    }

    private IEnumerator DealCards(List<Card> cardsToDeal)
    {
        while (cardsToDeal.Count > 0)
        {
            DealCard(cardsToDeal, _playerCards, _playerCardsRoot);
            
            yield return new WaitForSeconds(0.05f);

            DealCard(cardsToDeal, _opponentCards, _opponentCardsRoot);
            
            yield return new WaitForSeconds(0.05f);
        }

        _cardsDealt = true;
    }

    private void DealCard(List<Card> cardsToDeal, List<Card> targetDeck, Transform targetDeckTransform)
    {
        Card currentCard = cardsToDeal[0];
        currentCard.gameObject.transform.DOMove(targetDeckTransform.position, _cardMovementAnimationDuration);
        targetDeck.Add(currentCard);
        currentCard.gameObject.transform.SetParent(targetDeckTransform);
        cardsToDeal.RemoveAt(0);
    }

    private void CheckGameOver()
    {
        if (_playerCards.Count == 0 && _opponentCards.Count == 0)
        {
            _gameMessage.text = "Draw";
            _gameMessage.gameObject.SetActive(true);
            _gameOver = true;
        }
        else if (_playerCards.Count > 0 && _opponentCards.Count == 0)
        {
            _gameMessage.text = "Player Won";
            _gameMessage.gameObject.SetActive(true);
            _gameOver = true;
        }
        else if (_opponentCards.Count > 0 && _playerCards.Count == 0)
        {
            _gameMessage.text = "Opponent Won";
            _gameMessage.gameObject.SetActive(true);
            _gameOver = true;
        }
    }
    
    private void PlayRound()
    {
        CheckGameOver();

        if (_gameOver) return;
        
        FixZOffset(1f);
        Card playerCard = _playerCards[0];
        Card opponentCard = _opponentCards[0];
        Sequence roundSequence = DOTween.Sequence();
        roundSequence.Append(playerCard.transform.DOMove(_playerMovePosition.position, _cardMovementAnimationDuration));
        roundSequence.Append(playerCard.transform.DORotate(new Vector3(0f, 180f, 0f), _cardRotationAnimationDuration));
        roundSequence.Append(opponentCard.transform.DOMove(_opponentMovePosition.position, _cardMovementAnimationDuration));
        roundSequence.Append(opponentCard.transform.DORotate(new Vector3(0f, 180f, 0f), _cardRotationAnimationDuration));
        roundSequence.OnComplete(EvaluateRound);
    }

    private void EvaluateRound()
    {
        Card playerCard = _playerCards[0];
        Card opponentCard = _opponentCards[0];
        _playerCards.RemoveAt(0);
        _opponentCards.RemoveAt(0);
        
        if (playerCard.Value == opponentCard.Value)
        {
            HandleWar(playerCard, opponentCard);
        }
        else 
        {
            if (playerCard.Value > opponentCard.Value)
            {
                HandleRoundWinner(playerCard, opponentCard, _playerCards, _playerCardsRoot);
                opponentCard.gameObject.transform.SetParent(_playerCardsRoot);
            }
            else
            {
                HandleRoundWinner(playerCard, opponentCard, _opponentCards, _opponentCardsRoot);
                playerCard.gameObject.transform.SetParent(_opponentCardsRoot);
            }
        }
    }

    private void HandleWar(Card playerCard, Card opponentCard)
    {
        CheckGameOver();

        if (_gameOver) return;
        
        _warHappened = true;
        _playerWarCards.Add(playerCard);
        _OpponentWarCards.Add(opponentCard);
        Card playerWarCard = _playerCards[0];
        Card opponentWarCard = _opponentCards[0];
        _playerCards.RemoveAt(0);
        _opponentCards.RemoveAt(0);
        _playerWarCards.Add(playerWarCard);
        _OpponentWarCards.Add(opponentWarCard);
        Sequence warSequence = DOTween.Sequence();
        warSequence.Append(playerCard.transform.DORotate(new Vector3(0f, 0, 0f), _cardRotationAnimationDuration));
        warSequence.Append(opponentCard.transform.DORotate(new Vector3(0f, 0, 0f), _cardRotationAnimationDuration));
        warSequence.Append(playerWarCard.transform.DOMove(_playerMovePosition.position, _cardMovementAnimationDuration));
        warSequence.Append(opponentWarCard.transform.DOMove(_opponentMovePosition.position, _cardMovementAnimationDuration));
        warSequence.OnComplete(PlayRound);
    }

    private void FixZOffset(float offset)
    {
        foreach (Card warCard in _playerWarCards)
        {
            Vector3 position = warCard.transform.position;
            warCard.transform.position = new Vector3(position.x, position.y, offset);
        }
        
        foreach (Card warCard in _OpponentWarCards)
        {
            Vector3 position = warCard.transform.position;
            warCard.transform.position = new Vector3(position.x, position.y, offset);
        }
    }
    
    private void HandleRoundWinner(Card playerCard, Card opponentCard, List<Card> targetDeck, Transform targetDeckTransform)
    {
        Sequence evaluationSequence = DOTween.Sequence();
        
        if (_warHappened)
        {
            foreach (Card warCard in _playerWarCards)
            {
                evaluationSequence.Append(warCard.transform.DOMove(targetDeckTransform.position, _cardMovementAnimationDuration));
                warCard.gameObject.transform.SetParent(targetDeckTransform);
                targetDeck.Add(warCard);
            }

            _playerWarCards.Clear();

            foreach (Card warCard in _OpponentWarCards)
            {
                evaluationSequence.Append(warCard.transform.DOMove(targetDeckTransform.position, _cardMovementAnimationDuration));
                warCard.gameObject.transform.SetParent(targetDeckTransform);
                targetDeck.Add(warCard);
            }

            _OpponentWarCards.Clear();
            _warHappened = false;
        }

        evaluationSequence.Append(playerCard.transform.DORotate(new Vector3(0f, 0, 0f), _cardRotationAnimationDuration));
        evaluationSequence.Append(opponentCard.transform.DORotate(new Vector3(0f, 0, 0f), _cardRotationAnimationDuration));
        evaluationSequence.Append(playerCard.transform.DOMove(targetDeckTransform.position, _cardMovementAnimationDuration));
        evaluationSequence.Append(opponentCard.transform.DOMove(targetDeckTransform.position, _cardMovementAnimationDuration));
        evaluationSequence.OnComplete(FinishRound);
        targetDeck.Add(playerCard);
        targetDeck.Add(opponentCard);
    }
    
    private void FinishRound()
    {
        UpdateCardsRemaining();
        CheckGameOver();
        _roundPlaying = false;
    }
}
