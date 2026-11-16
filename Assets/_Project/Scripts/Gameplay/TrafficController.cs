using MasterParking.Core;
using MasterParking.Utilities;
using UnityEngine;

namespace MasterParking.Gameplay
{
    /// <summary>Mirrors a pure TrafficMoverModel onto a car transform.</summary>
    public sealed class TrafficController
    {
        private readonly TrafficMoverModel _model;
        private readonly Transform _transform;

        public TrafficController(MoverDefinition definition, GameObject car)
        {
            _model = new TrafficMoverModel(definition);
            _transform = car.transform;
            Apply();
        }

        public TrafficMoverModel Model => _model;

        public void Step(float deltaTime)
        {
            _model.Step(deltaTime);
            Apply();
        }

        private void Apply()
        {
            _transform.localPosition = MathConversions.ToWorld(_model.Position);
            _transform.localRotation = MathConversions.HeadingToRotation(_model.Heading);
        }
    }
}
