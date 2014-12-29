using UnityEngine;
using Tiled;

[RequireComponent(typeof(Map))]
public class LoadMapOnStart : MonoBehaviour
{
    #region MonoBehaviour

    void Start()
    {
        this.GetComponent<Map>().Load();
    }

    #endregion
}
