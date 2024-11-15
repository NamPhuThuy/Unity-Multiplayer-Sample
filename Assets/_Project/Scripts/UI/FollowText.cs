using TMPro;
using UnityEngine;

namespace UI
{
    public class FollowText : MonoBehaviour
    {
        public GameObject target; // Reference to the object to follow
        public TextMeshProUGUI text; // Reference to the Text component

        void Update()
        {
            // Update the text's position to match the target's position
            text.transform.position = target.transform.position;
        }
    }
}