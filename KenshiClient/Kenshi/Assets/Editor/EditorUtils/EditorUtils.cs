using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorUtils : MonoBehaviour
{
    [MenuItem("Game/Erase Player Prefs")]
    static void Export()
    {
        PlayerPrefs.DeleteAll();
    }
}
