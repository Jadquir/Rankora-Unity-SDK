using UnityEngine;

namespace Assets.Rankora_API.Scripts.Visual.Scripts
{
    public class LoadingSpinnerRotator : MonoBehaviour
    {
        [Range(0.01f, 3f)]
        [SerializeField] float RotationSpeed;
        private void Update()
        {
            transform.Rotate(Vector3.back, RotationSpeed * 360 * Time.deltaTime);
        }
    }
}
