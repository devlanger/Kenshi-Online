using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown fullscreenDropdown;
    [SerializeField] private TMP_Dropdown resolutionsDropdown;
    [SerializeField] private Button exitGameButton;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button logoutButton;

    public Dictionary<string, Resolution> res = new Dictionary<string, Resolution>();

    private void Awake()
    {
        logoutButton.onClick.AddListener(LogoutClick);
        
        fullscreenDropdown.ClearOptions();
        resolutionsDropdown.ClearOptions();
        

        foreach (var mode in Enum.GetValues(typeof(FullScreenMode)))
        {
            fullscreenDropdown.AddOptions(new List<string>(){ mode.ToString() });
        }
        
        Screen.resolutions.ToList().ForEach((x) => res.Add(x.ToString(), x));
        var v = Screen.resolutions.Select(s => s.ToString()).ToList();

        resolutionsDropdown.AddOptions(v);
        exitGameButton.onClick.AddListener(() => { Application.Quit(); });
        applyButton.onClick.AddListener(ApplySettings);
        
        fullscreenDropdown.value = fullscreenDropdown.options.FindIndex(o => o.text == Screen.fullScreenMode.ToString());
        resolutionsDropdown.value = resolutionsDropdown.options.FindIndex(o => o.text == Screen.currentResolution.ToString());
    }

    private void LogoutClick()
    {
        PlayerPrefs.DeleteKey("login_username");
        PlayerPrefs.DeleteKey("login_token");
        SceneManager.LoadScene(0);
    }

    private void ApplySettings()
    {
        var text = resolutionsDropdown.captionText.text;
        var resolution = res[text];
        if (!FullScreenMode.TryParse(fullscreenDropdown.captionText.text, out FullScreenMode mode))
        {
            mode = FullScreenMode.ExclusiveFullScreen;
        }
        
        Screen.SetResolution(resolution.width, resolution.height, mode, resolution.refreshRateRatio);
    }
}
