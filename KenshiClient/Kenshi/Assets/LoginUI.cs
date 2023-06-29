using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private Button loginButton;
    [SerializeField] private LoginMessagePanel _loginMessagePanel;
    [SerializeField] private GameObject _loginPanel;
    [SerializeField] private ClientConnectionSettings _clientConnectionSettings;

    private bool useLocalUrl => _clientConnectionSettings.useLocalLoginServer;
    private string localUrl => _clientConnectionSettings.loginLocalUrl;
    private string remoteUrl => _clientConnectionSettings.loginRemoteUrl;
    
    public string _url { get; private set; } = "http://localhost:3330";
    private string loginUrl => _clientConnectionSettings.loginApiEndpoint;
    private string checkTokenUrl => _clientConnectionSettings.checkTokenApiEndpoint;

    private int selectedInputField = 0;
    
    private Coroutine loginCoroutine;

    private void Awake()
    {
        if (!useLocalUrl)
        {
            _url = remoteUrl;
        }
        
        loginButton.onClick.AddListener(LoginClick);
    }

    private void Update()
    {
        if (!_loginPanel.activeSelf)
            return;
        
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            selectedInputField++;
            if (selectedInputField == 2)
            {
                selectedInputField = 0;
            }

            if (!usernameField.isFocused && !passwordField.isFocused)
            {
                selectedInputField = 0;
            }
            
            switch (selectedInputField)
            {
                case 0:
                    usernameField.Select();;
                    break;
                case 1:
                    passwordField.Select();
                    break;
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (_loginMessagePanel.isActiveAndEnabled)
            {   
                _loginMessagePanel.cancelButton.OnPointerClick(new PointerEventData(EventSystem.current));
            }
            else
            {
                loginButton.OnPointerClick(new PointerEventData(EventSystem.current));
            }
        }
    }

    private void LoginClick()
    {
        if(loginCoroutine != null) return;

        loginCoroutine = StartCoroutine(Login());
    }

    public class LoginRequestModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    private void Start()
    {
        usernameField.Select();
        
        if(!PlayerPrefs.HasKey("login_token")) return;
        
        loginCoroutine = StartCoroutine(CheckToken());
    }

    private IEnumerator CheckToken()
    {
        var dataObject = new
        {
            Username = PlayerPrefs.GetString("login_username"),
            Token = PlayerPrefs.GetString("login_token"),
        };

        string json = JsonConvert.SerializeObject(dataObject);

        using (UnityWebRequest www = new UnityWebRequest(_url + checkTokenUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.uploadHandler.contentType = "application/json";

            www.downloadHandler = new DownloadHandlerBuffer();

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                string msg = $"{www.error}: " + www.downloadHandler.text;
                Debug.Log(msg);
                //_loginMessagePanel.SetMessage(msg);
            }
            else
            {
                Debug.Log("Response: " + www.downloadHandler.text);

                SetLoginData(www.downloadHandler.text);
                LoadMenu();
            }

            loginCoroutine = null;
        }
    }

    private void SetLoginData(string response)
    {
        var user = JObject.Parse(response)["user"];
        PlayerPrefs.SetString("login_token", user?["token"].ToString());
        PlayerPrefs.SetString("login_username", user?["username"].ToString());
    }

    private IEnumerator Login()
    {
        if (usernameField.text.IsNullOrEmpty() || passwordField.text.IsNullOrEmpty())
        {
            _loginMessagePanel.SetMessage("Username and password fields needs to be filled.");
            yield break;
        }
        
        _loginMessagePanel.SetMessage("Logging in...");
        var dataObject = new
        {
            Username = usernameField.text,
            Password = passwordField.text
        };

        string json = JsonConvert.SerializeObject(dataObject);

        using (UnityWebRequest www = new UnityWebRequest(_url + loginUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.uploadHandler.contentType = "application/json";

            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success)
            {
                string msg = www.downloadHandler.text;
                if (msg.IsNullOrEmpty())
                {
                    msg = www.error;
                }
                _loginMessagePanel.SetMessage(msg);
            }
            else
            {
                Debug.Log("Response: " + www.downloadHandler.text);

                SetLoginData(www.downloadHandler.text);
                LoadMenu();
            }

            loginCoroutine = null;
        }
    }

    private static void LoadMenu()
    {
        SceneManager.LoadScene(1);
    }

    public void SetCredentials(string username, string password)
    {
        usernameField.text = username;
        passwordField.text = password;
    }
}
