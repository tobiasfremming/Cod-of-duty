using System;
using UnityEngine;

public class InitializeUI : MonoBehaviour
{
    [SerializeField] private GameObject screenUI;

    private void Start()
    {
        screenUI.SetActive(true);
    }
}
