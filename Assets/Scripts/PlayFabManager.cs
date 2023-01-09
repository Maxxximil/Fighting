using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using TMPro;

//Скрипт для авторизации через плейфаб
public class PlayFabManager : MonoBehaviour
{
    public TMP_Text MessageText;
    public TMP_InputField EmailInput;
    public TMP_InputField PasswordInput;
    
    //Кнопка регистрации
    public void RegisterButton()
    {
        if (PasswordInput.text.Length < 6)
        {
            MessageText.color = Color.red;
            MessageText.text = "Password too short";
            return;
        }
        var request = new RegisterPlayFabUserRequest
        {
            Email = EmailInput.text,
            Password = PasswordInput.text,
            RequireBothUsernameAndEmail = false
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
    }

    //Кнопка Логина
    public void LogInButton()
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = EmailInput.text,
            Password = PasswordInput.text,
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
    }

    //Кнопка сброса пароля
    public void ResetPasswordButton()
    {
        var request = new SendAccountRecoveryEmailRequest
        {
            Email = EmailInput.text,
            TitleId = "40431",
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnPasswordReset, OnError);
    }

    //удачный сброс пароля
    void OnPasswordReset(SendAccountRecoveryEmailResult result)
    {
        MessageText.color = Color.green;
        MessageText.text = "Password resent mail sent";
    }
    //удачная регистрация
    void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        MessageText.color = Color.green;
        MessageText.text = "Registered and Logged In";
    }
    //Удачное залогинивание
    void OnLoginSuccess(LoginResult result)
    {
        MessageText.color = Color.green;
        MessageText.text = "Logged In"; ;
        Debug.Log("Successful login");
    }
    //Ошибка
    void OnError(PlayFabError error)
    {
        MessageText.color = Color.red;
        MessageText.text = error.ErrorMessage;
        Debug.Log(error.GenerateErrorReport()); 
    }

}
