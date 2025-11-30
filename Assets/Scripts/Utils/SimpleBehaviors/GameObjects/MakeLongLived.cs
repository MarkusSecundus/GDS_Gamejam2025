using UnityEngine;

namespace MarkusSecundus.Utils
{
    public class MakeLongLived : MonoBehaviour
    {
        void Awake()
        {
            Object.DontDestroyOnLoad(this);
        }

    }
}
