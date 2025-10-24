using UnityEngine;
using System.Collections;
using EtT.World;

namespace EtT.Location
{
    public class LocationServiceWrapper : MonoBehaviour
    {
        public bool IsRunning { get; private set; }
        public Position LastPosition { get; private set; }

        public System.Action<Position> OnLocationUpdated;

        [SerializeField] private float updateInterval = 2f;
        [SerializeField] private float minDistanceMeters = 2f;

        private Coroutine _loop;

        public void StartLocation()
        {
            if (_loop != null) return;
            _loop = StartCoroutine(Run());
        }

        public void StopLocation()
        {
            if (_loop != null) StopCoroutine(_loop);
            _loop = null;
            if (Input.location.status == LocationServiceStatus.Running)
                Input.location.Stop();
            IsRunning = false;
        }

        private IEnumerator Run()
        {
            if (!Input.location.isEnabledByUser)
            {
                Debug.LogWarning("[GPS] Location disabled by user.");
                yield break;
            }

            Input.location.Start(5f, minDistanceMeters); // accuracy, distance
            int maxWait = 20;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait-- > 0)
                yield return new WaitForSeconds(1);

            if (Input.location.status != LocationServiceStatus.Running)
            {
                Debug.LogWarning("[GPS] Failed to start.");
                yield break;
            }

            IsRunning = true;
            var lastSent = LastPosition;
            while (true)
            {
                var d = Input.location.lastData;
                var p = new Position(d.latitude, d.longitude);
                LastPosition = p;

                if (OnLocationUpdated != null)
                    OnLocationUpdated(p);

                yield return new WaitForSeconds(updateInterval);
            }
        }
    }
}