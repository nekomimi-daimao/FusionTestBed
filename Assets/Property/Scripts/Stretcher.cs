using UniRx;
using UnityEngine;

namespace Property
{
    public class Stretcher : MonoBehaviour
    {
        private IStretchScale _stretchScale;

        public void Init(IStretchScale stretchScale)
        {
            _stretchScale = stretchScale;

            stretchScale.Scale
                .TakeUntilDisable(this)
                .Subscribe(OnScaleChanged);
        }

        private void OnScaleChanged(int s)
        {
            this.transform.localScale = Vector3.one + Vector3.up * s;
        }
    }
}
