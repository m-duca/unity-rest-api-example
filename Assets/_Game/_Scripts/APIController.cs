using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using SimpleJSON;
using UnityEngine.UI;

namespace APIExample
{
    public class APIController : MonoBehaviour
    {
        // Inspector
        [Header("Parameters")]
        [SerializeField] private int _minId;
        [SerializeField] private int _maxId;
        [SerializeField] private int _scaleMultiplier;
        [SerializeField] private int _shinyChance;
        [SerializeField] private string _apiPath = "https://pokeapi.co/api/v2/pokemon/";

        [Header("References")]
        [SerializeField] private CharacterView _characterView;
        [SerializeField] private CharacterAudioPlayer _characterAudioPlayer;
        [SerializeField] private Button _btnReroll;

        private void Start() => LoadRandomCharacter();

        private void OnEnable() => _btnReroll.onClick.AddListener(LoadRandomCharacter);

        private void OnDisable() => _btnReroll.onClick.RemoveListener(LoadRandomCharacter);

        private void LoadRandomCharacter()
        {
            int randomId = Random.Range(_minId, _maxId + 1);

            _characterView.ResetView(randomId);

            StartCoroutine(GetCharacter_Coroutine(randomId));
        }

        private IEnumerator GetCharacter_Coroutine(int id)
        {
            string url = $"{_apiPath}{id}";

            UnityWebRequest characterRequest = UnityWebRequest.Get(url);
            yield return characterRequest.SendWebRequest();

            if (characterRequest.result == UnityWebRequest.Result.ConnectionError ||
                characterRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"[APIController] {characterRequest.error}");
                yield break;
            }

            JSONNode json = JSON.Parse(characterRequest.downloadHandler.text);

            bool isShiny = Random.Range(0, 101) <= _shinyChance;

            string name = json["name"];
            string spriteUrl = isShiny
                ? json["sprites"]["front_shiny"]
                : json["sprites"]["front_default"];

            string cryUrl = json["cries"]["latest"];

            // Types
            JSONNode typesNode = json["types"];
            string[] types = new string[typesNode.Count];

            for (int i = 0, j = typesNode.Count - 1; i < typesNode.Count; i++, j--)
            {
                types[j] = "▪ " + UtilitiesUI.CapitalizeFirstLetter(typesNode[i]["type"]["name"]);
            }

            // Name
            string formattedName = (isShiny ? "* " : "■ ") +
                                   UtilitiesUI.CapitalizeFirstLetter(name);

            // Texture Request
            UnityWebRequest textureRequest = UnityWebRequestTexture.GetTexture(spriteUrl);
            yield return textureRequest.SendWebRequest();

            if (textureRequest.result == UnityWebRequest.Result.ConnectionError ||
                textureRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"[APIController] {textureRequest.error}");
                yield break;
            }

            Texture2D texture = UtilitiesUI.ScaleTexture(
                DownloadHandlerTexture.GetContent(textureRequest),
                _scaleMultiplier
            );

            // Audio Clip Request
            UnityWebRequest audioRequest = UnityWebRequestMultimedia.GetAudioClip(cryUrl, AudioType.OGGVORBIS);
            yield return audioRequest.SendWebRequest();

            if (audioRequest.result == UnityWebRequest.Result.ConnectionError ||
                audioRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"[APIController] {audioRequest.error}");
                yield break;
            }

            var clip = DownloadHandlerAudioClip.GetContent(audioRequest);
            _characterAudioPlayer.SetAudioClip(clip);

            _characterView.SetTexture(texture);
            _characterView.SetData(formattedName, types, isShiny);
            _characterView.PlayAnimation();
        }
    }
}