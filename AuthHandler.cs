using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class AuthHandler : MonoBehaviour
{
    private string url = "https://sid-restapi.onrender.com";
    private string token;
    private string username;
    private int score;

    [SerializeField] private TMP_InputField[] input;
    [SerializeField] private GameObject[] ventanas;
    [SerializeField] private TextMeshProUGUI nombre;
    [SerializeField] private TextMeshProUGUI scoreTexto;
    [SerializeField] private TextMeshProUGUI listaTexto;

    void Start()
    {
        token = PlayerPrefs.GetString("Token");
        username = PlayerPrefs.GetString("Username");
        score = PlayerPrefs.GetInt("Score");

        if (token != null)
        {
            StartCoroutine(GetProfile());
        }

    }

    public void Login()
    {
        JsonData data = new JsonData();

        data.username = input[0].text;
        data.password = input[1].text;

        string postData = JsonUtility.ToJson(data);

        StartCoroutine(GetLogin(postData));
    }

    public void Register()
    {
        JsonData data = new JsonData();

        data.username = input[0].text;
        data.password = input[1].text;

        string postData = JsonUtility.ToJson(data);

        StartCoroutine(GetRegister(postData));
    }

    public void Score()
    {
        score = int.Parse(input[2].text);
        string jsonString = $"{{\"username\":\"{username}\",\"data\":{{\"score\":{score}}}}}";

        StartCoroutine(GetScore(jsonString));
    }

    public void SignOut()
    {
        ventanas[0].SetActive(true);
        ventanas[1].SetActive(false);
    }

    IEnumerator GetRegister(string data)
    {
        UnityWebRequest request = UnityWebRequest.Post(url + "/api/usuarios", data, "application/json");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                StartCoroutine(GetLogin(data));
            }
            else
            {
                Debug.Log($"Status: {request.responseCode} \n Error: {request.error}");
            }
        }
    }

    IEnumerator GetLogin(string data)
    {
        UnityWebRequest request = UnityWebRequest.Post(url + "/api/auth/login", data, "application/json");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                AuthData authData = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);

                token = authData.token;
                username = authData.usuario.username;

                PlayerPrefs.SetString("Token", token);
                PlayerPrefs.SetString("Username", username);

                if (authData.usuario.data.score != 0)
                {
                    score = authData.usuario.data.score;
                    PlayerPrefs.SetInt("Score", score);
                }
                else
                {
                    score = 0;
                    PlayerPrefs.SetInt("Score", score);
                    
                }

                ventanas[0].SetActive(false);
                ventanas[1].SetActive(true);

                nombre.text = username;
                scoreTexto.text = score.ToString();

                StartCoroutine(GetList());
            }
            else
            {
                Debug.Log($"Status: {request.responseCode} \n Error: {request.error}");
            }
        }
    }

    IEnumerator GetProfile()
    {

        UnityWebRequest request = UnityWebRequest.Get(url + "/api/usuarios/" + username);
        request.SetRequestHeader("x-token", token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                ventanas[0].SetActive(false);
                ventanas[1].SetActive(true);

                nombre.text = username;
                scoreTexto.text = score.ToString();

                StartCoroutine(GetList());
            }
            else
            {
                ventanas[0].SetActive(true);
            }
        }
    }


    IEnumerator GetScore(string scoreData)
    {
        UnityWebRequest request = UnityWebRequest.Put(url + "/api/usuarios", scoreData);
        request.method = "PATCH";
        request.SetRequestHeader("x-token", token);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                scoreTexto.text = score.ToString();
                StartCoroutine(GetList());
            }
            else
            {
                Debug.Log($"Status: {request.responseCode} \n Error: {request.error}");
            }
        }
    }

    public IEnumerator GetList()
    {
        UnityWebRequest request = UnityWebRequest.Get(url + "/api/usuarios");
        request.SetRequestHeader("x-token", token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                ListUser users = JsonUtility.FromJson<ListUser>(request.downloadHandler.text);

                List<User> orderList = users.usuarios.OrderByDescending(u => u.data.score).ToList();

                string message = "";

                foreach (var user in orderList)
                {
                    if (user.data != null)
                    {
                        message += $"Username: {user.username} - Score: {user.data.score}\n";
                    }
                    else
                    {
                        message += $"Username: {user.username} - Score: 0\n";
                    }
                }

                listaTexto.text = message;
            }
        }
    }
}

public class JsonData
{
    public string username;
    public string password;
}

[System.Serializable]
public class AuthData
{
    public User usuario;
    public string token;
}

[System.Serializable]
public class ListUser
{
    public User[] usuarios;
}

[System.Serializable]
public class User
{
    public string _id;
    public string username;
    public bool estado;
    public UserData data;
}

[System.Serializable]
public class UserData
{
    public int score;
}

