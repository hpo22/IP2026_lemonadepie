using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Firebase;
using Firebase.Auth;
using System.Threading.Tasks;

public class LoginManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField usernameInput;   // email
    public TMP_InputField passwordInput;
    public TMP_Text errorText;

    [Header("Scene to Load")]
    public string sceneToLoad = "dda_part";

    private FirebaseAuth auth;

    private async void Awake()
    {
        var status = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (status != DependencyStatus.Available)
        {
            if (errorText) errorText.text = "Firebase dependency error: " + status;
            Debug.LogError("Firebase dependency error: " + status);
            return;
        }

        auth = FirebaseAuth.DefaultInstance;
    }

    

    public void OnLoginButtonPressed()
    {
        _ = Login(usernameInput.text.Trim(), passwordInput.text);
    }

    private async Task Login(string email, string password)
    {
        if (errorText) errorText.text = "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            if (errorText) errorText.text = "Email/password cannot be empty.";
            return;
        }

        try
        {
            await auth.SignInWithEmailAndPasswordAsync(email, password);

            // success
            SceneManager.LoadScene(sceneToLoad);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            if (errorText) errorText.text = "Login failed: " + e.Message;
        }
    }
}
