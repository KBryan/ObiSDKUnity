using Newtonsoft.Json;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using Vuplex.WebView;

///<summary>
/// Create a Smart Account using Obi SDK
/// Also a good place to include helpful links for context.
/// Implementation used is for user with ZAuth accounts
///</summary>
public class CreateAccountSignIn : MonoBehaviour
{
    private string AccessToken = "ADD_ACCESS_TOKEN";
    private string RefreshToken = "ADD_REFRESH_TOKEN";
    public GameObject CanvasWebViewPrefab; 
    
    private void Start()
    {
        CreateAccountOrLogIn();
    }

    private async void CreateAccountOrLogIn()
    {
        Debug.Log("Creating Account or Logging In");

        // Find or instantiate the CanvasWebViewPrefab GameObject
        var webViewPrefabGameObject = GameObject.Find("CanvasWebViewPrefab");
        if (webViewPrefabGameObject == null)
        {
            Debug.Log("CanvasWebViewPrefab not found, instantiating...");
            webViewPrefabGameObject = Instantiate(CanvasWebViewPrefab);
            webViewPrefabGameObject.name = "CanvasWebViewPrefab"; // Set the name so it can be found later
        }

        Assert.IsNotNull(webViewPrefabGameObject);
        
        var webViewPrefab = webViewPrefabGameObject.GetComponent<CanvasWebViewPrefab>();
        // Wait for the WebViewPrefab to initialize, because the WebViewPrefab.WebView property
        // is null until the prefab has initialized.
        await webViewPrefab.WaitUntilInitialized();
        webViewPrefab.WebView.PageLoadScripts.Add(@"
            window.vuplex.addEventListener('message', function(event) {
                console.log('Message received from C#: ' + event.data);
            });
        ");
        string messageToPost = CreatePostMessageString(AccessToken, RefreshToken);
        Debug.Log("posting message: " + messageToPost);
        await webViewPrefab.WebView.WaitForNextPageLoadToFinish();
        webViewPrefab.WebView.MessageEmitted += (sender, eventArgs) =>
        {
            Debug.Log("JSON received: " + eventArgs.Value);
        };
        webViewPrefab.WebView.PostMessage(messageToPost);
    }

    private static string CreatePostMessageString(string _accessToken, string _refreshToken)
    {
        var payload = new
        {
            homeChainId = "secret-4",
            accessToken = _accessToken,
            refreshToken = _refreshToken
        };

        var message = new
        {
            type = "@obi/create-account",
            payload
        };

        return JsonConvert.SerializeObject(message);
    }
}