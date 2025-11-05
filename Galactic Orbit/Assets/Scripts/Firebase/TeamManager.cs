using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

// Manages team/club functionality in Firebase
// Handles creating teams, joining, inviting, managing members, and calculating leaderboards
public class TeamManager : MonoBehaviour
{
    public static TeamManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ===== CREATE TEAM =====
    
    /// <summary>
    /// Creates a new team. Current user becomes the leader.
    /// </summary>
    public async Task<(bool success, string message, string teamId)> CreateTeamAsync(string name, string description, bool isPublic, int memberLimit = 50)
    {
        FirebaseUser currentUser = FirebaseManager.Instance.CurrentUser;
        
        if (currentUser == null)
            return (false, "Not signed in", null);

        if (string.IsNullOrWhiteSpace(name))
            return (false, "Team name is required", null);

        try
        {
            // Generate unique team ID
            string teamId = FirebaseManager.Instance.DbReference.Child("teams").Push().Key;

            // Get user's profile for username/displayName
            var profileSnapshot = await FirebaseManager.Instance.DbReference
                .Child("userProfiles").Child(currentUser.UserId).GetValueAsync();

            if (!profileSnapshot.Exists)
                return (false, "User profile not found", null);

            UserProfile userProfile = JsonUtility.FromJson<UserProfile>(profileSnapshot.GetRawJsonValue());

            // Create team data
            Team team = new Team
            {
                name = name,
                description = description,
                createdTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                creatorId = currentUser.UserId,
                isPublic = isPublic,
                memberLimit = memberLimit,
                memberCount = 1,
                totalPoints = 0
            };

            // Create leader member entry
            TeamMember leaderMember = new TeamMember
            {
                username = userProfile.username,
                displayName = userProfile.displayName,
                role = "leader",
                joinedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                pointsContributed = 0
            };

            // Prepare batch updates
            var updates = new Dictionary<string, object>
            {
                { $"teams/{teamId}/name", team.name },
                { $"teams/{teamId}/description", team.description },
                { $"teams/{teamId}/createdTimestamp", team.createdTimestamp },
                { $"teams/{teamId}/creatorId", team.creatorId },
                { $"teams/{teamId}/isPublic", team.isPublic },
                { $"teams/{teamId}/memberLimit", team.memberLimit },
                { $"teams/{teamId}/memberCount", team.memberCount },
                { $"teams/{teamId}/totalPoints", team.totalPoints },
                { $"teams/{teamId}/members/{currentUser.UserId}", JsonUtility.ToJson(leaderMember) }
            };

            // Add team ID to user's teamIds array
            var userTeamIds = new List<string>();
            if (userProfile.teamIds != null)
                userTeamIds.AddRange(userProfile.teamIds);
            userTeamIds.Add(teamId);
            
            updates[$"userProfiles/{currentUser.UserId}/teamIds"] = userTeamIds.ToArray();

            await FirebaseManager.Instance.DbReference.UpdateChildrenAsync(updates);

            Debug.Log($"✅ Team '{name}' created with ID: {teamId}");
            return (true, "Team created successfully!", teamId);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create team: {e.Message}");
            return (false, $"Error: {e.Message}", null);
        }
    }

    // ===== JOIN TEAM =====
    
    /// <summary>
    /// Joins a public team or uses invite code for private team
    /// </summary>
    public async Task<(bool success, string message)> JoinTeamAsync(string teamId, string inviteCode = null)
    {
        FirebaseUser currentUser = FirebaseManager.Instance.CurrentUser;
        
        if (currentUser == null)
            return (false, "Not signed in");

        try
        {
            // Get team data
            var teamSnapshot = await FirebaseManager.Instance.DbReference
                .Child("teams").Child(teamId).GetValueAsync();

            if (!teamSnapshot.Exists)
                return (false, "Team not found");

            // Parse team data
            var teamData = teamSnapshot.Value as Dictionary<string, object>;
            bool isPublic = Convert.ToBoolean(teamData["isPublic"]);
            int memberLimit = Convert.ToInt32(teamData["memberLimit"]);
            int memberCount = Convert.ToInt32(teamData["memberCount"]);

            // Check if team is full
            if (memberCount >= memberLimit)
                return (false, "Team is full");

            // Check if already a member
            var memberSnapshot = await FirebaseManager.Instance.DbReference
                .Child("teams").Child(teamId).Child("members").Child(currentUser.UserId).GetValueAsync();

            if (memberSnapshot.Exists)
                return (false, "Already a member of this team");

            // If private team, verify invite code
            if (!isPublic)
            {
                if (string.IsNullOrEmpty(inviteCode))
                    return (false, "Invite code required for private team");

                var inviteSnapshot = await FirebaseManager.Instance.DbReference
                    .Child("teams").Child(teamId).Child("inviteCodes").Child(inviteCode).GetValueAsync();

                if (!inviteSnapshot.Exists)
                    return (false, "Invalid invite code");
            }

            // Get user profile
            var profileSnapshot = await FirebaseManager.Instance.DbReference
                .Child("userProfiles").Child(currentUser.UserId).GetValueAsync();

            UserProfile userProfile = JsonUtility.FromJson<UserProfile>(profileSnapshot.GetRawJsonValue());

            // Create member entry
            TeamMember newMember = new TeamMember
            {
                username = userProfile.username,
                displayName = userProfile.displayName,
                role = "member",
                joinedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                pointsContributed = 0
            };

            // Prepare updates
            var updates = new Dictionary<string, object>
            {
                { $"teams/{teamId}/members/{currentUser.UserId}", JsonUtility.ToJson(newMember) },
                { $"teams/{teamId}/memberCount", memberCount + 1 }
            };

            // Add team to user's teamIds
            var userTeamIds = new List<string>();
            if (userProfile.teamIds != null)
                userTeamIds.AddRange(userProfile.teamIds);
            userTeamIds.Add(teamId);
            
            updates[$"userProfiles/{currentUser.UserId}/teamIds"] = userTeamIds.ToArray();

            await FirebaseManager.Instance.DbReference.UpdateChildrenAsync(updates);

            Debug.Log($"✅ Joined team: {teamId}");
            return (true, "Joined team successfully!");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to join team: {e.Message}");
            return (false, $"Error: {e.Message}");
        }
    }

    // ===== LEAVE TEAM =====
    
    /// <summary>
    /// Leave a team. If leader leaves, promote oldest admin or member to leader.
    /// </summary>
    public async Task<(bool success, string message)> LeaveTeamAsync(string teamId)
    {
        FirebaseUser currentUser = FirebaseManager.Instance.CurrentUser;
        
        if (currentUser == null)
            return (false, "Not signed in");

        try
        {
            // Get member data to check role
            var memberSnapshot = await FirebaseManager.Instance.DbReference
                .Child("teams").Child(teamId).Child("members").Child(currentUser.UserId).GetValueAsync();

            if (!memberSnapshot.Exists)
                return (false, "Not a member of this team");

            TeamMember member = JsonUtility.FromJson<TeamMember>(memberSnapshot.GetRawJsonValue());
            bool isLeader = member.role == "leader";

            // If leader is leaving, handle succession
            if (isLeader)
            {
                var allMembersSnapshot = await FirebaseManager.Instance.DbReference
                    .Child("teams").Child(teamId).Child("members").GetValueAsync();

                var members = new List<(string userId, TeamMember member)>();
                foreach (var child in allMembersSnapshot.Children)
                {
                    if (child.Key != currentUser.UserId)
                    {
                        TeamMember m = JsonUtility.FromJson<TeamMember>(child.GetRawJsonValue());
                        members.Add((child.Key, m));
                    }
                }

                if (members.Count == 0)
                {
                    // Last member - delete the team
                    await FirebaseManager.Instance.DbReference.Child("teams").Child(teamId).RemoveValueAsync();
                    Debug.Log("Team deleted - no remaining members");
                }
                else
                {
                    // Promote next leader (prioritize admins, then oldest member)
                    var nextLeader = members
                        .OrderByDescending(m => m.member.role == "admin")
                        .ThenBy(m => m.member.joinedTimestamp)
                        .First();

                    await FirebaseManager.Instance.DbReference
                        .Child("teams").Child(teamId).Child("members").Child(nextLeader.userId).Child("role")
                        .SetValueAsync("leader");

                    Debug.Log($"Promoted {nextLeader.member.username} to leader");
                }
            }

            // Remove member
            var updates = new Dictionary<string, object>
            {
                { $"teams/{teamId}/members/{currentUser.UserId}", null }
            };

            // Decrement member count
            var teamSnapshot = await FirebaseManager.Instance.DbReference
                .Child("teams").Child(teamId).GetValueAsync();
            
            if (teamSnapshot.Exists)
            {
                var teamData = teamSnapshot.Value as Dictionary<string, object>;
                int memberCount = Convert.ToInt32(teamData["memberCount"]);
                updates[$"teams/{teamId}/memberCount"] = memberCount - 1;
            }

            // Remove team from user's teamIds
            var profileSnapshot = await FirebaseManager.Instance.DbReference
                .Child("userProfiles").Child(currentUser.UserId).GetValueAsync();
            
            UserProfile userProfile = JsonUtility.FromJson<UserProfile>(profileSnapshot.GetRawJsonValue());
            var userTeamIds = new List<string>();
            if (userProfile.teamIds != null)
                userTeamIds.AddRange(userProfile.teamIds);
            userTeamIds.Remove(teamId);
            
            updates[$"userProfiles/{currentUser.UserId}/teamIds"] = userTeamIds.ToArray();

            await FirebaseManager.Instance.DbReference.UpdateChildrenAsync(updates);

            return (true, "Left team successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to leave team: {e.Message}");
            return (false, $"Error: {e.Message}");
        }
    }

    // ===== INVITE CODE MANAGEMENT =====
    
    /// <summary>
    /// Generates a random invite code for private teams
    /// </summary>
    public async Task<(bool success, string inviteCode)> GenerateInviteCodeAsync(string teamId)
    {
        try
        {
            // Check if user has permission (leader or admin)
            if (!await HasPermissionAsync(teamId, new[] { "leader", "admin" }))
                return (false, null);

            string inviteCode = GenerateRandomCode(8);
            
            await FirebaseManager.Instance.DbReference
                .Child("teams").Child(teamId).Child("inviteCodes").Child(inviteCode)
                .SetValueAsync(true);

            return (true, inviteCode);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to generate invite code: {e.Message}");
            return (false, null);
        }
    }

    // ===== ROLE MANAGEMENT =====
    
    /// <summary>
    /// Changes a member's role (leader/admin/member). Only leader can do this.
    /// </summary>
    public async Task<(bool success, string message)> ChangeRoleAsync(string teamId, string targetUserId, string newRole)
    {
        FirebaseUser currentUser = FirebaseManager.Instance.CurrentUser;
        
        if (currentUser == null)
            return (false, "Not signed in");

        if (!new[] { "leader", "admin", "member" }.Contains(newRole))
            return (false, "Invalid role");

        try
        {
            // Only leader can change roles
            if (!await HasPermissionAsync(teamId, new[] { "leader" }))
                return (false, "Only team leader can change roles");

            // Can't demote yourself as leader
            if (currentUser.UserId == targetUserId && newRole != "leader")
                return (false, "Leader cannot demote themselves");

            await FirebaseManager.Instance.DbReference
                .Child("teams").Child(teamId).Child("members").Child(targetUserId).Child("role")
                .SetValueAsync(newRole);

            return (true, "Role updated successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to change role: {e.Message}");
            return (false, $"Error: {e.Message}");
        }
    }

    // ===== KICK MEMBER =====
    
    /// <summary>
    /// Removes a member from the team (leader/admin only)
    /// </summary>
    public async Task<(bool success, string message)> KickMemberAsync(string teamId, string targetUserId)
    {
        try
        {
            if (!await HasPermissionAsync(teamId, new[] { "leader", "admin" }))
                return (false, "Insufficient permissions");

            // Can't kick yourself
            if (FirebaseManager.Instance.CurrentUser.UserId == targetUserId)
                return (false, "Cannot kick yourself. Use leave team instead.");

            // Remove member
            var updates = new Dictionary<string, object>
            {
                { $"teams/{teamId}/members/{targetUserId}", null }
            };

            // Decrement member count
            var teamSnapshot = await FirebaseManager.Instance.DbReference
                .Child("teams").Child(teamId).GetValueAsync();
            var teamData = teamSnapshot.Value as Dictionary<string, object>;
            int memberCount = Convert.ToInt32(teamData["memberCount"]);
            updates[$"teams/{teamId}/memberCount"] = memberCount - 1;

            // Remove team from target user's teamIds
            var profileSnapshot = await FirebaseManager.Instance.DbReference
                .Child("userProfiles").Child(targetUserId).GetValueAsync();
            
            UserProfile userProfile = JsonUtility.FromJson<UserProfile>(profileSnapshot.GetRawJsonValue());
            var userTeamIds = new List<string>();
            if (userProfile.teamIds != null)
                userTeamIds.AddRange(userProfile.teamIds);
            userTeamIds.Remove(teamId);
            
            updates[$"userProfiles/{targetUserId}/teamIds"] = userTeamIds.ToArray();

            await FirebaseManager.Instance.DbReference.UpdateChildrenAsync(updates);

            return (true, "Member removed");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to kick member: {e.Message}");
            return (false, $"Error: {e.Message}");
        }
    }

    // ===== TEAM SETTINGS =====
    
    /// <summary>
    /// Updates team settings (name, description, privacy, member limit)
    /// </summary>
    public async Task<(bool success, string message)> UpdateTeamSettingsAsync(string teamId, string name = null, string description = null, bool? isPublic = null, int? memberLimit = null)
    {
        try
        {
            if (!await HasPermissionAsync(teamId, new[] { "leader", "admin" }))
                return (false, "Insufficient permissions");

            var updates = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(name))
                updates[$"teams/{teamId}/name"] = name;
            
            if (description != null)
                updates[$"teams/{teamId}/description"] = description;
            
            if (isPublic.HasValue)
                updates[$"teams/{teamId}/isPublic"] = isPublic.Value;
            
            if (memberLimit.HasValue && memberLimit.Value > 0)
                updates[$"teams/{teamId}/memberLimit"] = memberLimit.Value;

            if (updates.Count > 0)
                await FirebaseManager.Instance.DbReference.UpdateChildrenAsync(updates);

            return (true, "Team settings updated");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to update team: {e.Message}");
            return (false, $"Error: {e.Message}");
        }
    }

    // ===== GET TEAM DATA =====
    
    /// <summary>
    /// Gets full team information including all members
    /// </summary>
    public async Task<Team> GetTeamAsync(string teamId)
    {
        try
        {
            var snapshot = await FirebaseManager.Instance.DbReference
                .Child("teams").Child(teamId).GetValueAsync();

            if (!snapshot.Exists)
                return null;

            // Parse base team data
            var teamData = snapshot.Value as Dictionary<string, object>;
            Team team = new Team
            {
                teamId = teamId,
                name = teamData["name"].ToString(),
                description = teamData.ContainsKey("description") ? teamData["description"].ToString() : "",
                createdTimestamp = Convert.ToInt64(teamData["createdTimestamp"]),
                creatorId = teamData["creatorId"].ToString(),
                isPublic = Convert.ToBoolean(teamData["isPublic"]),
                memberLimit = Convert.ToInt32(teamData["memberLimit"]),
                memberCount = Convert.ToInt32(teamData["memberCount"]),
                totalPoints = teamData.ContainsKey("totalPoints") ? Convert.ToInt32(teamData["totalPoints"]) : 0
            };

            // Parse members
            if (snapshot.Child("members").Exists)
            {
                var membersList = new List<TeamMemberWithId>();
                foreach (var memberSnapshot in snapshot.Child("members").Children)
                {
                    TeamMember member = JsonUtility.FromJson<TeamMember>(memberSnapshot.GetRawJsonValue());
                    membersList.Add(new TeamMemberWithId
                    {
                        userId = memberSnapshot.Key,
                        member = member
                    });
                }
                team.members = membersList.ToArray();
            }

            return team;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to get team: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets all teams the current user is a member of
    /// </summary>
    public async Task<Team[]> GetMyTeamsAsync()
    {
        FirebaseUser currentUser = FirebaseManager.Instance.CurrentUser;
        
        if (currentUser == null)
            return new Team[0];

        try
        {
            var profileSnapshot = await FirebaseManager.Instance.DbReference
                .Child("userProfiles").Child(currentUser.UserId).GetValueAsync();

            if (!profileSnapshot.Exists)
                return new Team[0];

            UserProfile profile = JsonUtility.FromJson<UserProfile>(profileSnapshot.GetRawJsonValue());
            
            if (profile.teamIds == null || profile.teamIds.Length == 0)
                return new Team[0];

            var teams = new List<Team>();
            foreach (string teamId in profile.teamIds)
            {
                Team team = await GetTeamAsync(teamId);
                if (team != null)
                    teams.Add(team);
            }

            return teams.ToArray();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to get user teams: {e.Message}");
            return new Team[0];
        }
    }

    // ===== HELPER FUNCTIONS =====
    
    private async Task<bool> HasPermissionAsync(string teamId, string[] allowedRoles)
    {
        FirebaseUser currentUser = FirebaseManager.Instance.CurrentUser;
        if (currentUser == null) return false;

        try
        {
            var memberSnapshot = await FirebaseManager.Instance.DbReference
                .Child("teams").Child(teamId).Child("members").Child(currentUser.UserId).GetValueAsync();

            if (!memberSnapshot.Exists) return false;

            TeamMember member = JsonUtility.FromJson<TeamMember>(memberSnapshot.GetRawJsonValue());
            return allowedRoles.Contains(member.role);
        }
        catch
        {
            return false;
        }
    }

    private string GenerateRandomCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        System.Random random = new System.Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}

// ===== DATA STRUCTURES =====

[System.Serializable]
public class Team
{
    public string teamId;
    public string name;
    public string description;
    public long createdTimestamp;
    public string creatorId;
    public bool isPublic;
    public int memberLimit;
    public int memberCount;
    public int totalPoints;
    public TeamMemberWithId[] members;
}

[System.Serializable]
public class TeamMember
{
    public string username;
    public string displayName;
    public string role;  // "leader", "admin", or "member"
    public long joinedTimestamp;
    public int pointsContributed;
}

[System.Serializable]
public class TeamMemberWithId
{
    public string userId;
    public TeamMember member;
}