using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RegisterPanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField emailField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private TMP_InputField repeatPasswordField;
    [SerializeField] private Button registerButton;
    [SerializeField] private LoginMessagePanel _loginMessagePanel;

    [SerializeField] private GameObject _loginPanel;
    [SerializeField] private LoginUI _loginUi;

    [SerializeField] private string registerUrl;

    private Coroutine registerCoroutine;

    private int selectedInputField = 0;

    private void Update()
    {
        if (!gameObject.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            selectedInputField++;
            if (selectedInputField == 4)
            {
                selectedInputField = 0;
            }

            if (!usernameField.isFocused && !passwordField.isFocused && !repeatPasswordField.isFocused && !emailField.isFocused)
            {
                selectedInputField = 0;
            }
            
            switch (selectedInputField)
            {
                case 0:
                    usernameField.Select();;
                    break;
                case 1:
                    emailField.Select();
                    break;
                case 2:
                    passwordField.Select();
                    break;
                case 3:
                    repeatPasswordField.Select();
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
                registerButton.OnPointerClick(new PointerEventData(EventSystem.current));
            }
        }
    }

    private void OnEnable()
    {
        usernameField.Select();
    }

    private void OnDisable()
    {
        usernameField.text = "";
        emailField.text = "";
        passwordField.text = "";
        repeatPasswordField.text = "";
    }

    private void Awake()
    {
        registerButton.onClick.AddListener(() =>
        {
            if(registerCoroutine != null) return;
            
            registerCoroutine = StartCoroutine(Register());
        });
    }

    private IEnumerator Register()
    {
        var dataObject = new
        {
            Username = usernameField.text,
            Email = emailField.text,
            Password = passwordField.text,
            RepeatPassword = repeatPasswordField.text
        };

        string json = JsonConvert.SerializeObject(dataObject);

        using (UnityWebRequest www = new UnityWebRequest(_loginUi._url + registerUrl, "POST"))
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
                _loginMessagePanel.SetMessage(msg);
            }
            else
            {
                Debug.Log("Response: " + www.downloadHandler.text);
                _loginMessagePanel.SetMessage(www.downloadHandler.text);
                _loginPanel.SetActive(true);
                gameObject.SetActive(false);

                _loginUi.SetCredentials(dataObject.Username, dataObject.Password);
            }

            registerCoroutine = null;
        }
    }
}
