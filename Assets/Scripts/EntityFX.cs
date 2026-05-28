    using System.Collections;
    using UnityEngine;

    public class EntityFX : MonoBehaviour
    {
        private SpriteRenderer sr;

        [Header("Flash FX")]
        [SerializeField] private float flashDuration;
        [SerializeField] private Material hitMat;
        private Material originalMat;

        [Header("Ailment colors")]
        [SerializeField] private Color[] igniteColor;
        [SerializeField] private Color[] chillColor;
        [SerializeField] private Color[] shockColor;
        private void Awake()
        {
            CacheRenderer();
        }

        private void Start()
        {
            CacheRenderer();
        }

        private void CacheRenderer()
        {
            if (sr != null)
                return;

            sr = GetComponentInChildren<SpriteRenderer>();

            if (sr != null)
                originalMat = sr.material;
        }

        public IEnumerator FlashFX()
        {
            CacheRenderer();

            if (sr == null)
                yield break;

            originalMat = sr.material;

            if (hitMat != null)
                sr.material = hitMat;

            Color currentColor = sr.color;
            sr.color = Color.white;

            yield return new WaitForSeconds(flashDuration);

            sr.color = currentColor;

            if (originalMat != null)
                sr.material = originalMat;
        }


        private void RedColorBlink()
        {
            if (sr.color != Color.white)
            { sr.color = Color.white; }
            else { sr.color = Color.red; }
        }

        private void CancelColorChange()
        {
            CancelInvoke();
            sr.color = Color.white;
        }
        public void IgniteFxFor(float _seconds)
        {
            InvokeRepeating("IgniteColorFx", 0, 0.3f);
            Invoke("CancelColorChange", _seconds);
        }

        public void ChillFxFor(float _seconds)
        {
            InvokeRepeating("ChillColorFx", 0, 0.3f);
            Invoke("CancelColorChange", _seconds);
        }

        public void ShockFxFor(float _seconds)
        {
            InvokeRepeating("ShockColorFx", 0, 0.3f);
            Invoke("CancelColorChange", _seconds);
        }

        private void IgniteColorFx()
        {
            if (sr.color != igniteColor[0])
                sr.color = igniteColor[0];
            else
                sr.color = igniteColor[1];
        }
        private void ChillColorFx()
        {
            if (sr.color != chillColor[0])
                sr.color = chillColor[0];
            else
                sr.color = chillColor[1];
        }

        private void ShockColorFx()
        {
            if (sr.color != shockColor[0])
                sr.color = shockColor[0];
            else
                sr.color = shockColor[1];
        }

    }
