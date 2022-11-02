using Fusion;
using UniRx;

namespace Property
{
    public class StretchScaleFusion : NetworkBehaviour, IStretchScale
    {
        [Networked(OnChanged = nameof(OnScaleChanged))]
        public int ScaleNetworked { get; set; }

        public IntReactiveProperty Scale { get; } = new IntReactiveProperty(ScaleDefault);

        private static readonly int ScaleDefault = 1;

        public static void OnScaleChanged(Changed<StretchScaleFusion> changed)
        {
            changed.Behaviour.Scale.Value = changed.Behaviour.ScaleNetworked;
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            if (Object.HasStateAuthority && GetInput<ScalerInput.ScaleInputData>(out var s))
            {
                ScaleNetworked = (int)s.Scale;
            }
        }

        public override void Spawned()
        {
            base.Spawned();

            var spawner = FindObjectOfType<StretcherRunner>();
            if (spawner == null)
            {
                return;
            }

            spawner.stretchScaleFusions.Add(this);
        }
    }
}
