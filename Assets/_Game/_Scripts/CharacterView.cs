using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace APIExample
{
    public class CharacterView : MonoBehaviour
    {
        // Inspector
        [Header("References")]
        [SerializeField] private RawImage _iconRawImg;
        [SerializeField] private TextMeshProUGUI _nameTxt;
        [SerializeField] private TextMeshProUGUI _idTxt;
        [SerializeField] private TextMeshProUGUI[] _typesTxt;
        [SerializeField] private CharacterAudioPlayer _characterAudioPlayer;

        // Properties
        public RawImage GetIconRawImage() => _iconRawImg;
        public TextMeshProUGUI GetNameText() => _nameTxt;
        public TextMeshProUGUI GetIdText() => _idTxt;
        public TextMeshProUGUI[] GetTypesTexts() => _typesTxt;

        public void ResetView(int id)
        {
            _iconRawImg.texture = Texture2D.blackTexture;

            _nameTxt.text = "■ Waiting...";
            _idTxt.text = $"# {id}";

            UtilitiesUI.SetAlpha(_idTxt, 0);

            foreach (var t in _typesTxt)
                t.text = "";
        }

        public void SetData(string name, string[] types, bool isShiny)
        {
            _nameTxt.color = isShiny ? Color.yellow : Color.white;
            _nameTxt.text = name;

            for (int i = 0; i < _typesTxt.Length; i++)
            {
                _typesTxt[i].text = i < types.Length ? types[i] : "";
            }
        }

        public void SetTexture(Texture2D tex)
        {
            _iconRawImg.texture = tex;
            _iconRawImg.texture.filterMode = FilterMode.Point;
        }

        public void PlayAnimation()
        {
            // Hiding
            UtilitiesUI.SetAlpha(_iconRawImg, 0);
            UtilitiesUI.SetAlpha(_nameTxt, 0);
            UtilitiesUI.SetAlpha(_idTxt, 0);

            foreach (TextMeshProUGUI type in _typesTxt)
                UtilitiesUI.SetAlpha(type, 0);

            RectTransform rt = _iconRawImg.rectTransform;
            rt.localScale = Vector3.zero;

            Sequence seq = DOTween.Sequence();

            // Scale + fade
            seq.Append(rt.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
            seq.Join(_iconRawImg.DOFade(1, 0.2f));

            // Sway
            seq.Append(rt.DORotate(new Vector3(0, 0, 10f), 0.15f)
                .SetLoops(4, LoopType.Yoyo));

            seq.Append(_nameTxt.DOFade(1, 0.2f));
            seq.Join(_idTxt.DOFade(1, 0.2f));

            // Moving Info Txts
            foreach (TextMeshProUGUI types in _typesTxt)
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
                _characterAudioPlayer.PlayCurrentAudioClip();
            }));
        }
    }
}
