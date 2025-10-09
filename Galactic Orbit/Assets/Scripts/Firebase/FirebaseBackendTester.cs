using UnityEngine;
using System.Collections.Generic;

public class FirebaseBackendTester : MonoBehaviour
{
    [Header("Test Controls")]
    [Tooltip("Press these keys to run tests")]
    public KeyCode testGetProfileKey = KeyCode.Alpha1;
    public KeyCode testUpdateDisplayNameKey = KeyCode.Alpha2;
    public KeyCode testUpdateBioKey = KeyCode.Alpha3;
    public KeyCode testAddPointsKey = KeyCode.Alpha4;
    public KeyCode testAttendEventKey = KeyCode.Alpha5;
    public KeyCode testCheckAuthKey = KeyCode.Alpha0;

    [Header("Test Data")]
    public string testDisplayName = "TestUser123";
    public string testBio = "This is a test bio!";
    public int testPointsToAdd = 50;
    public string testEventId = "event_test_001";

    private void Update()
    {
        // Check auth status
        if (Input.GetKeyDown(testCheckAuthKey))
        {
            TestAuthStatus();
        }

        // Test get profile
        if (Input.GetKeyDown(testGetProfileKey))
        {
            TestGetProfile();
        }

        // Test update display name
        if (Input.GetKeyDown(testUpdateDisplayNameKey))
        {
            TestUpdateDisplayName();
        }

        // Test update bio
        if (Input.GetKeyDown(testUpdateBioKey))
        {
            TestUpdateBio();
        }

        // Test add points
        if (Input.GetKeyDown(testAddPointsKey))
        {
            TestAddPoints();
        }

        // Test attend event
        if (Input.GetKeyDown(testAttendEventKey))
        {
            TestAttendEvent();
        }
    }

    private void OnGUI()
    {
        // Display instructions on screen
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 16;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(10, 10, 500, 300), 
            "=== Firebase Backend Tester ===\n\n" +
            $"[{testCheckAuthKey}] Check Auth Status\n" +
            $"[{testGetProfileKey}] Get Current Profile\n" +
            $"[{testUpdateDisplayNameKey}] Update Display Name\n" +
            $"[{testUpdateBioKey}] Update Bio\n" +
            $"[{testAddPointsKey}] Add {testPointsToAdd} Points\n" +
            $"[{testAttendEventKey}] Attend Test Event\n\n" +
            "Check Console for results!", 
            style);
    }

    // ===== TEST METHODS =====

    void TestAuthStatus()
    {
        Debug.Log("=== TEST: Auth Status ===");
        
        if (FirebaseManager.Instance == null)
        {
            Debug.LogError("❌ FirebaseManager not initialized!");
            return;
        }

        var currentUser = FirebaseManager.Instance.CurrentUser;
        
        if (currentUser != null)
        {
            Debug.Log($"✅ User is logged in!");
            Debug.Log($"   User ID: {currentUser.UserId}");
            Debug.Log($"   Email: {currentUser.Email}");
            Debug.Log($"   Email Verified: {currentUser.IsEmailVerified}");
        }
        else
        {
            Debug.LogWarning("⚠️ No user logged in. Please log in first!");
        }
    }

    void TestGetProfile()
    {
        Debug.Log("=== TEST: Get Current User Profile ===");
        
        UserProfileManager.Instance.GetCurrentUserProfile((profile) =>
        {
            if (profile != null)
            {
                Debug.Log("✅ Profile loaded successfully!");
                Debug.Log($"   Username: {profile.username}");
                Debug.Log($"   Email: {profile.email}");
                Debug.Log($"   Display Name: {profile.displayName}");
                Debug.Log($"   Bio: {profile.bio}");
                Debug.Log($"   Total Points: {profile.totalPoints}");
                Debug.Log($"   Level: {profile.level}");
                Debug.Log($"   Events Attended: {profile.eventsAttended}");
                Debug.Log($"   Quests Completed: {profile.questsCompleted}");
                Debug.Log($"   Buildings Visited: {profile.buildingsVisited}");
                Debug.Log($"   Created: {System.DateTimeOffset.FromUnixTimeSeconds(profile.createdTimestamp)}");
            }
            else
            {
                Debug.LogError("❌ Failed to load profile. Are you logged in?");
            }
        });
    }

    void TestUpdateDisplayName()
    {
        Debug.Log($"=== TEST: Update Display Name to '{testDisplayName}' ===");
        
        UserProfileManager.Instance.UpdateProfileField("displayName", testDisplayName, (success, message) =>
        {
            if (success)
            {
                Debug.Log($"✅ {message}");
                Debug.Log("   Fetching updated profile...");
                TestGetProfile();
            }
            else
            {
                Debug.LogError($"❌ {message}");
            }
        });
    }

    void TestUpdateBio()
    {
        Debug.Log($"=== TEST: Update Bio to '{testBio}' ===");
        
        UserProfileManager.Instance.UpdateProfileField("bio", testBio, (success, message) =>
        {
            if (success)
            {
                Debug.Log($"✅ {message}");
                Debug.Log("   Fetching updated profile...");
                TestGetProfile();
            }
            else
            {
                Debug.LogError($"❌ {message}");
            }
        });
    }

    void TestAddPoints()
    {
        Debug.Log($"=== TEST: Add {testPointsToAdd} Points ===");
        
        UserProfileManager.Instance.AddPoints(testPointsToAdd, (success, message) =>
        {
            if (success)
            {
                Debug.Log($"✅ {message}");
            }
            else
            {
                Debug.LogError($"❌ {message}");
            }
        });
    }

    void TestAttendEvent()
    {
        Debug.Log($"=== TEST: Attend Event '{testEventId}' ===");
        
        UserProfileManager.Instance.AttendEvent(testEventId, 100, (success, message) =>
        {
            if (success)
            {
                Debug.Log($"✅ {message}");
                Debug.Log("   Fetching updated profile...");
                TestGetProfile();
            }
            else
            {
                Debug.LogError($"❌ {message}");
            }
        });
    }
}