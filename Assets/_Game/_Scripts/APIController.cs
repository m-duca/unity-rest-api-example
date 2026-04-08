using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using SimpleJSON;
using UnityEngine.UI;
using TMPro;
using System;

namespace APIExample
{
    public class APIController : MonoBehaviour
    {
        // Inspector
        [Header("Parameters")]
        [SerializeField] private int _minId;
        [SerializeField] private int _maxId;
     
        [Header("References")]
        [SerializeField] private RawImage _iconRawImg;
        [SerializeField] private TextMeshProUGUI _nameTxt;
        [SerializeField] private TextMeshProUGUI _idTxt;
        [SerializeField] private TextMeshProUGUI[] _typeTxts;
        [SerializeField] private Button _btnReroll;

        // Not serialize
        private const string API_PATH_URL = "https://pokeapi.co/api/v2/pokemon/";

        private void Start()
        {
            _iconRawImg.texture = Texture2D.blackTexture;

            _nameTxt.text = String.Empty;
            _idTxt.text = String.Empty;

            foreach(TextMeshProUGUI typeTxt in _typeTxts)
                typeTxt.text = String.Empty;
        }

        private void OnEnable() => _btnReroll.onClick.AddListener(Call_GetRandomCharacter);
        
        private void OnDisable() => _btnReroll.onClick.RemoveListener(Call_GetRandomCharacter);

        private void Call_GetRandomCharacter()
        {
            int randomId = UnityEngine.Random.Range(_minId, _maxId + 1);
            
            _iconRawImg.texture = Texture2D.blackTexture;

            _nameTxt.text = "Waiting...";
            _idTxt.text = $"#{randomId}";

            foreach(TextMeshProUGUI typeTxt in _typeTxts)
                typeTxt.text = String.Empty;
            
            StartCoroutine(GetCharacterById_Coroutine(randomId));
        }

        private IEnumerator GetCharacterById_Coroutine(int id)
        {
            string characterUrl = $"{API_PATH_URL}{id}";

            UnityWebRequest characterInfoRequest = UnityWebRequest.Get(characterUrl);

            yield return characterInfoRequest.SendWebRequest();

            if (characterInfoRequest.isNetworkError || characterInfoRequest.isHttpError)
            {
                Debug.LogError($"[APIController] {characterInfoRequest.error}");
                yield break;
            }
        }
    }
}

