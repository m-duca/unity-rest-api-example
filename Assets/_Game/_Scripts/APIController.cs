using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using SimpleJSON;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

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

        [Header("References")]
        [SerializeField] private RawImage _iconRawImg;
        [SerializeField] private TextMeshProUGUI _nameTxt;
        [SerializeField] private TextMeshProUGUI _idTxt;
        [SerializeField] private TextMeshProUGUI[] _typeTxts;
        [SerializeField] private Button _btnReroll;
        [SerializeField] private AudioSource _cryAudioSource;

        // Not serialize
        private const string API_PATH_URL = "https://pokeapi.co/api/v2/pokemon/";

        private void Start()
        {
            _iconRawImg.texture = Texture2D.blackTexture;

            _nameTxt.text = String.Empty;
            _idTxt.text = String.Empty;
            SetAlpha(_idTxt, 0);

            foreach (TextMeshProUGUI typeTxt in _typeTxts)
                typeTxt.text = String.Empty;

            _cryAudioSource.clip = null;

            Call_GetRandomCharacter();
        }

        private void OnEnable() => _btnReroll.onClick.AddListener(Call_GetRandomCharacter);

        private void OnDisable() => _btnReroll.onClick.RemoveListener(Call_GetRandomCharacter);

        private void Call_GetRandomCharacter()
        {
            int randomId = UnityEngine.Random.Range(_minId, _maxId + 1);

            _iconRawImg.texture = Texture2D.blackTexture;

            _nameTxt.text = "■ Waiting...";
            _idTxt.text = $"# {randomId}";
            SetAlpha(_idTxt, 0);

            foreach (TextMeshProUGUI typeTxt in _typeTxts)
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

            JSONNode infoNode = JSON.Parse(characterInfoRequest.downloadHandler.text);

            string characterName = "■ " + CapitalizeFirstLetter(infoNode["name"]);

            string characterSpriteURL;

            if (UnityEngine.Random.Range(0, 101) <= _shinyChance)
                characterSpriteURL = infoNode["sprites"]["front_shiny"];
            else
                characterSpriteURL = infoNode["sprites"]["front_default"];

            string characteAudioURL = infoNode["cries"]["latest"];

            JSONNode typesNode = infoNode["types"];
            string[] typesName = new string[typesNode.Count];

            for (int i = 0, j = typesNode.Count - 1; i < typesNode.Count; i++, j--)
                typesName[j] = "▪ " + CapitalizeFirstLetter(typesNode[i]["type"]["name"]);

            UnityWebRequest characterSpriteRequest = UnityWebRequestTexture.GetTexture(characterSpriteURL);
            yield return characterSpriteRequest.SendWebRequest();

            if (characterSpriteRequest.isNetworkError || characterSpriteRequest.isHttpError)
            {
                Debug.LogError($"[APIController] {characterSpriteRequest.error}");
                yield break;
            }

            _iconRawImg.texture = ScaleTexture(DownloadHandlerTexture.GetContent(characterSpriteRequest), _scaleMultiplier);
            _iconRawImg.texture.filterMode = FilterMode.Point;

            UnityWebRequest characterAudioRequest = UnityWebRequestMultimedia.GetAudioClip(characteAudioURL, AudioType.OGGVORBIS);
            yield return characterAudioRequest.SendWebRequest();

            if (characterAudioRequest.isNetworkError || characterAudioRequest.isHttpError)
            {
                Debug.LogError($"[APIController] {characterAudioRequest.error}");
                yield break;
            }

            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(characterAudioRequest);
            _cryAudioSource.clip = audioClip;

            _nameTxt.text = CapitalizeFirstLetter(characterName);

            for (int i = 0; i < typesName.Length; i++)
                _typeTxts[i].text = CapitalizeFirstLetter(typesName[i]);

            PlayAnimation();
        }

        private string CapitalizeFirstLetter(string str)
        {
            return char.ToUpper(str[0]) + str.Substring(1);
        }

        private Texture2D ScaleTexture(Texture2D src, int scale)
        {
            Texture2D result = new Texture2D(src.width * scale, src.height * scale);
            result.filterMode = FilterMode.Point;

            for (int y = 0; y < result.height; y++)
            {
                for (int x = 0; x < result.width; x++)
                {
                    result.SetPixel(x, y, src.GetPixel(x / scale, y / scale));
                }
            }

            result.Apply();
            return result;
        }

        private void PlayAnimation()
        {
            // Reset
            SetAlpha(_iconRawImg, 0);
            SetAlpha(_nameTxt, 0);
            SetAlpha(_idTxt, 0);

            foreach (var t in _typeTxts)
                SetAlpha(t, 0);

            RectTransform rt = _iconRawImg.rectTransform;
            rt.localScale = Vector3.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.rotation = Quaternion.identity;

            Sequence seq = DOTween.Sequence();

            // Scale + fade
            seq.Append(rt.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
            seq.Join(_iconRawImg.DOFade(1, 0.2f));

            // Sway
            seq.Append(
                rt.DORotate(new Vector3(0, 0, 10f), 0.15f)
                  .SetLoops(4, LoopType.Yoyo)
                  .SetEase(Ease.InOutSine)
            );

            seq.Append(_nameTxt.DOFade(1, 0.2f));
            seq.Join(_idTxt.DOFade(1, 0.2f));

            foreach (var types in _typeTxts)
            {
                seq.Append(types.DOFade(1, 0.15f));
                seq.Join(
                    types.rectTransform
                     .DOLocalMoveY(types.rectTransform.localPosition.y + 10f, 0.15f)
                     .From()
                );
            }

            // Sync SFX
            seq.Insert(0.2f, DOVirtual.DelayedCall(0f, () =>
            {
                _cryAudioSource.Play();
            }));
        }

        private void SetAlpha(Graphic g, float alpha)
        {
            Color c = g.color;
            c.a = alpha;
            g.color = c;
        }
    }
}

