using MasterParking.Core;

namespace MasterParking.Vehicle
{
    /// <summary>
    /// Binds the deterministic simulation to its view. Nothing else owns vehicle
    /// state, which keeps restart/reset logic in one place.
    /// </summary>
    public sealed class VehicleController
    {
        private readonly VehicleSimulation _simulation;
        private readonly VehicleView _view;

        public VehicleController(VehicleConfig config, StartPose start, VehicleView view)
        {
            _simulation = new VehicleSimulation(config);
            _view = view;
            Reset(start);
        }

        public VehicleState State => _simulation.State;
        public VehicleSimulation Simulation => _simulation;

        public void Reset(StartPose start)
        {
            _simulation.Reset(start.Position, start.Angle);
            _view?.ResetPose();
            _view?.ApplyState(_simulation.State);
        }

        public void Step(in VehicleInput input, float deltaTime)
        {
            _simulation.Step(input, deltaTime);
        }

        public void ApplyVisuals()
        {
            _view?.ApplyState(_simulation.State);
        }

        public void Halt()
        {
            _simulation.Halt();
            _view?.ApplyState(_simulation.State);
        }
    }
}
