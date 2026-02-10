using UnityEngine;
using UnityEngine.AI;

namespace EchoThief.AI
{
    /// <summary>
    /// Handles waypoint-based patrol behavior for guards.
    /// Guards walk between waypoints, pausing briefly at each one to "look around."
    /// </summary>
    public class GuardPatrol : MonoBehaviour
    {
        [Header("Waypoints")]
        [Tooltip("Assign patrol waypoints in the Inspector. The guard walks between them in order.")]
        [SerializeField] private Transform[] _waypoints;

        [Header("Patrol Behavior")]
        [Tooltip("How long the guard pauses at each waypoint.")]
        [SerializeField] private float _waitTimeAtWaypoint = 2f;

        [Tooltip("If true, guard ping-pongs (A→B→C→B→A). If false, loops (A→B→C→A).")]
        [SerializeField] private bool _pingPong = false;

        private int _currentWaypointIndex = 0;
        private float _waitTimer;
        private bool _isWaiting;
        private int _direction = 1; // 1 = forward, -1 = reverse (for ping-pong)

        /// <summary>
        /// Called every frame by GuardStateMachine while in Patrol state.
        /// </summary>
        public void UpdatePatrol(NavMeshAgent agent)
        {
            if (_waypoints == null || _waypoints.Length == 0) return;

            if (_isWaiting)
            {
                _waitTimer -= Time.deltaTime;
                if (_waitTimer <= 0f)
                {
                    _isWaiting = false;
                    AdvanceWaypoint();
                    SetDestination(agent);
                }
                return;
            }

            // Check if we've reached the current waypoint
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                _isWaiting = true;
                _waitTimer = _waitTimeAtWaypoint;
            }
        }

        /// <summary>
        /// Resume patrol from the current waypoint after returning from another state.
        /// </summary>
        public void ResumePatrol()
        {
            _isWaiting = false;
        }

        /// <summary>
        /// Set the NavMeshAgent destination to the current waypoint.
        /// </summary>
        public void SetDestination(NavMeshAgent agent)
        {
            if (_waypoints == null || _waypoints.Length == 0) return;

            agent.SetDestination(_waypoints[_currentWaypointIndex].position);
        }

        private void AdvanceWaypoint()
        {
            if (_pingPong)
            {
                _currentWaypointIndex += _direction;

                if (_currentWaypointIndex >= _waypoints.Length - 1)
                    _direction = -1;
                else if (_currentWaypointIndex <= 0)
                    _direction = 1;
            }
            else
            {
                _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Length;
            }
        }

        /// <summary>
        /// Draw waypoint path in the editor for easy visualization.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (_waypoints == null || _waypoints.Length < 2) return;

            Gizmos.color = Color.yellow;
            for (int i = 0; i < _waypoints.Length - 1; i++)
            {
                if (_waypoints[i] != null && _waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(_waypoints[i].position, _waypoints[i + 1].position);
                    Gizmos.DrawSphere(_waypoints[i].position, 0.3f);
                }
            }

            if (_waypoints[_waypoints.Length - 1] != null)
            {
                Gizmos.DrawSphere(_waypoints[_waypoints.Length - 1].position, 0.3f);

                // Draw closing line for loop mode
                if (!_pingPong && _waypoints[0] != null)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(_waypoints[_waypoints.Length - 1].position, _waypoints[0].position);
                }
            }
        }
    }
}
