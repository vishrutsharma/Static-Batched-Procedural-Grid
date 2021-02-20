using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Snake3D.Components
{
    public class SmoothAnimationComponent : MonoBehaviour
    {
        #region -------------------------------- SerializeField ---------------------------------

#pragma warning disable 649

        [Header("Smooth Translation")]
        [SerializeField]
        private AnimationCurve translationCurve;
        [SerializeField]
        private float startY;
        [SerializeField]
        private float endY;

        [Header("Smooth Scaling")]
        [SerializeField]
        private AnimationCurve scalingCurve;
        [SerializeField]
        private Vector3 startScale;
        [SerializeField]
        private Vector3 endScale;

        [SerializeField]
        private float animationDuration;

#pragma warning restore 649

        #endregion -------------------------------------------------------------------------------


        #region -------------------------------- SerializeField ---------------------------------

        private float timeQuant = 0;

        #endregion -------------------------------------------------------------------------------


        #region -------------------------------- Private Methods ----------------------------------

        private IEnumerator Translate()
        {
            Vector3 startPos = transform.position;
            startPos.y = startY;
            transform.position = startPos;

            while (timeQuant < 1)
            {
                timeQuant += Time.deltaTime / animationDuration;
                transform.position = new Vector3(startPos.x,
                                                 startY - ((startY - endY) * translationCurve.Evaluate(timeQuant)),
                                                 startPos.z);

                yield return new WaitForEndOfFrame();
            }
            transform.position = new Vector3(transform.position.x, endY, transform.position.z);
        }

        private IEnumerator Scale()
        {
            transform.localScale = startScale;
            while (timeQuant < 1)
            {
                timeQuant += Time.deltaTime / animationDuration;
                transform.localScale = endScale * scalingCurve.Evaluate(timeQuant);
                yield return new WaitForEndOfFrame();
            }

            transform.localScale = endScale;
        }

        #endregion -------------------------------------------------------------------------------


        #region -------------------------------- Public Methods ----------------------------------

        public void DoTranslation()
        {
            StartCoroutine(Translate());
        }

        public void DoScaling()
        {
            StartCoroutine(Translate());
        }

        #endregion -------------------------------------------------------------------------------
    }
}

