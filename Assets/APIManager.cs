using System;
using System.Collections.Generic;
using System.Linq;
using Proyecto26;
using TMPro;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;

public class APIManager : MonoBehaviour
{
    [Header("Get Score Panel")]
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private InputField _getUserScoreText;
    [SerializeField] private Button _submitButton;
    [SerializeField] private Button _getDataButton;
    
    [Header("Sign Up/In Panel")]
    [SerializeField] private InputField _emailText;
    [SerializeField] private InputField _usernameText;
    [SerializeField] private InputField _passwordText;
    [SerializeField] private Button _signUpButton;
    [SerializeField] private Button _signInButton;
    
    private readonly System.Random _random = new();
    User user = new ();
    private string AuthKey = "ENTER_YOUR_AUTHKEY"; 
    public static int playerScore { get; private set; }
    public static string playerName { get; private set; }
    public static string localId { get; private set; }
    private string _idToken;
    private string _getLocalId;
    private const string _databaseURL = "https://projecttraining-23ba7-default-rtdb.firebaseio.com/users";
    
    private void Start()
    {
        _submitButton.onClick.AddListener(OnSubmit);
        _getDataButton.onClick.AddListener(OnGetData);
        _signUpButton.onClick.AddListener(SignUpUserButton);
        _signInButton.onClick.AddListener(SignInUserButton);
        playerScore = _random.Next(0, 101);
        _scoreText.text = "Score: " + playerScore;
    }

    private void OnSubmit()
    {
        PostDataToDatabase();
    }

    private void OnGetData()
    {
        GetLocalID();
    }

    private void SignUpUserButton()
    {
        SignUpUser(_emailText.text, _usernameText.text, _passwordText.text);
        CleanInputFields(_emailText, _usernameText, _passwordText);
    }
    private void SignInUserButton()
    {
        SignInUser(_emailText.text, _passwordText.text);
        CleanInputFields(_emailText, _passwordText);
    }
    private void UpdateScore()
    {
        _scoreText.text = "Score: " + user.userScore;
    }

    private void PostDataToDatabase()
    {
        User user = new ();

        
        RestClient.Put(_databaseURL + "/" + user.localId + ".json?auth=" + _idToken, user);
        Debug.Log($"Name: {user.userName} Score: {user.userScore} Local ID: {user.localId} idToken : {_idToken}" );
    }

    private void GetDataFromDatabase()
    {
        RestClient.Get<User>(_databaseURL + "/" + _getLocalId + ".json?auth=" + _idToken).Then(
            response =>
            {
                user =  response;
                Debug.Log($"Name: {response.userName} Score: {response.userScore}");
                UpdateScore();
            });
    }
    

    private void SignUpUser(string email, string username, string password)
    {
        string userdata = "{\"email\":\"" + email + "\",\"password\":\"" + password + "\",\"returnSecureToken\":true}";
        const string signUpUserURL = "https://identitytoolkit.googleapis.com/v1/accounts:signUp?key=";
        RestClient.Post<SignResponse>(signUpUserURL + AuthKey, userdata).Then(response =>
            {
                localId = response.localId;
                _idToken = response.idToken;
                playerName = username;
                PostDataToDatabase();
            }).Catch(error =>
        {
            Debug.Log(error);
        });
    }

    private void SignInUser(string email, string password)
    {
        string userdata = "{\"email\":\"" + email + "\",\"password\":\"" + password + "\",\"returnSecureToken\":true}";
        const string signInUserURL = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=";
        RestClient.Post<SignResponse>(signInUserURL + AuthKey, userdata).Then(
            response =>
            {
                localId = response.localId;
                _idToken = response.idToken;
                GetUsername();
            }).Catch(Debug.Log);
    }
    
    private void GetUsername()
    {
        RestClient.Get<User>(_databaseURL + "/" + localId + ".json?auth=" + _idToken).Then(response =>
        {
            playerName = response.userName;
        });
    }
    
    private void GetLocalID()
    {
        RestClient.Get(_databaseURL + ".json?auth=" + _idToken).Then(response =>
        {
            var username = _getUserScoreText.text;
            fsData userData = fsJsonParser.Parse(response.Text);
            Dictionary<string, User> users = null;
            var fsSerializer = new fsSerializer();
            fsSerializer.TryDeserialize(userData, ref users);

            foreach (var user in users.Values.Where(user => user.userName == username))
            {
                _getLocalId  = user.localId;
                Debug.Log(_getLocalId);
                GetDataFromDatabase();
                break;
            }

        });
    }

    private void OnDisable()
    {
        _submitButton.onClick.RemoveListener(OnSubmit);
        _getDataButton.onClick.RemoveListener(OnGetData);
        _signUpButton.onClick.RemoveListener(SignUpUserButton);
        _signInButton.onClick.RemoveListener(SignInUserButton);
    }

    private void CleanInputFields(params InputField[] fields)
    {
        fields.ToList().ForEach(u => u.text = String.Empty);

    }
    
    
}
