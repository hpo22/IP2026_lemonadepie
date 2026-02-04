using System;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;

public class ClueTrackerRealtimeDB : MonoBehaviour
{
    public static ClueTrackerRealtimeDB Instance;

    private FirebaseAuth auth;
    private DatabaseReference dbRoot;

    private async void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        var status = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (status != DependencyStatus.Available)
        {
            Debug.LogError("Firebase dependency error: " + status);
            return;
        }

        auth = FirebaseAuth.DefaultInstance;
        dbRoot = FirebaseDatabase.DefaultInstance.RootReference;
    }

    private string GetUserId()
    {
        var user = auth?.CurrentUser;
        return user != null ? user.UserId : null;
    }

    private DatabaseReference UserCluePath(string uid) =>
        dbRoot.Child("users").Child(uid).Child("clue_found");

    /// Call this when clue is grabbed
    public async Task RegisterClueAsync(string itemId, string itemName)
    {
        string uid = GetUserId();
        if (string.IsNullOrEmpty(uid))
        {
            Debug.LogError("No logged-in user. Login first before saving clues.");
            return;
        }

        var clueRef = UserCluePath(uid);
        var itemNameRef = clueRef.Child("items").Child(itemId).Child("itemName");
        var totalRef = clueRef.Child("total");

        try
        {
            // 1) Check if item already exists (avoid double counting)
            var snap = await itemNameRef.GetValueAsync();
            if (snap.Exists)
            {
                Debug.Log($"Clue already saved: {itemId}. Not incrementing total.");
                return;
            }

            // 2) Save item name under itemId
            await itemNameRef.SetValueAsync(itemName);

            // 3) Increment total safely using transaction
            await totalRef.RunTransaction(mutableData =>
            {
                long current = 0;
                if (mutableData.Value != null)
                {
                    // Realtime DB returns numbers as long/double sometimes
                    try { current = Convert.ToInt64(mutableData.Value); }
                    catch { current = 0; }
                }

                mutableData.Value = current + 1;
                return TransactionResult.Success(mutableData);
            });

            Debug.Log($"Saved clue: {itemId} ({itemName}) to Realtime DB");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save clue: " + e);
        }
    }
}

