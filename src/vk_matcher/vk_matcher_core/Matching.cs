using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using VkNet;
using VkNet.Model;
using VkNet.Enums.Filters;

namespace Vk.Models
{
    public class User
    {
        // General fields
        public long id;
        public string first_name;
        public string last_name;
        public string deactivated;
        public int? hidden;

        // Addons
        public string photo_100;
    }

    public class Group
    {
        public long id;
    }
}

namespace VKMatcher.Core
{
    public class VkUser
    {
        public User User { get; }
        public List<VkUser> Friends { get; set; }

        public List<long> Subscriptions { get; set; }
        public List<long> Groups { get; set; }

        public VkUser(User user)
        {
            User = user;
        }
    }

    public class VkApiGetGroupsExecuteGroupsList
    {
        public long count;
        public long[] items; // May be array of objects
    }

    public class VkApiGetGroupsExecuteUser
    {
        public long userId;
        public VkApiGetGroupsExecuteGroupsList groups;
    }

    public class VkApiGetSubExecuteSubList
    {
        public long count;
        public Vk.Models.Group[] items; // May be array of objects
    }

    public class VkApiGetSubExecuteUser
    {
        public long userId;
        public VkApiGetSubExecuteSubList groups;
    }

    public class VkApiGetFriendExecuteFriendsList
    {
        public long count;
        public Vk.Models.User[] items; // May be array of objects
    }

    public class VkApiGetFriendExecuteUser
    {
        public long userId;
        public VkApiGetFriendExecuteFriendsList friends;
        //public long[] friends;
    }

    public class VkApiExecuteResponse<TResponse>
    {
        public TResponse response;
    }

    class MatchedGroup
    {
        public long Id { get; }
        public double Weight { get; }

        public MatchedGroup(long id, double weight)
        {
            Id = id;
            Weight = weight;
        }
    }

    class MatchedUser : IComparable<MatchedUser>
    {
        public VkUser User { get; }
        public double Likely { get; set; }
        public List<long> MatchedGroups { get; }

        public MatchedUser(VkUser user, double likely, List<long> matchedGroups)
        {
            User = user;
            Likely = likely;
            MatchedGroups = matchedGroups;
        }

        public int CompareTo(MatchedUser other)
        {
            return other.Likely.CompareTo(Likely);
        }
    }

    public class Matching
    {
        public readonly int MaxConsiderGroups = 30;

        HttpClient client;

        VkApi vkApi;
        VkUser selfUser;

        Dictionary<long, VkUser> considerUsers = new Dictionary<long, VkUser>();

        public Matching(string accessToken, uint userId)
        {
            client = new HttpClient(new HttpClientHandler());

            vkApi = new VkApi();
            vkApi.Authorize(accessToken, userId);

            selfUser = new VkUser(vkApi.Users.Get(userId, ProfileFields.All));
        }

        void AddUniqueUserToConsider(VkUser user)
        {
            //// !!!
            // REMOVE ME:!!!
            //if (considerUsers.Count >= 200)
            //    return;
            ///!!!
            /// !!!
            if (!considerUsers.ContainsKey(user.User.Id))
            {
                considerUsers.Add(user.User.Id, user);
            }
        }

        string BuildGetGroupsRequest(IEnumerable<long> userIds, string getMethod, int count = 30, string addFields = null)
        {
            return string.Format(@"var users = [{0}];
            var result = [];

            var i = 0;
            while (i < users.length)
            {{
                result.push({{ userId: users[i], groups: API.{2}({{ user_id: users[i], count: {1}{3} }})}});

                i = i + 1;
            }}

            return result;", String.Join(",", userIds.Select(x => x.ToString())), count, getMethod, (addFields != null ? addFields : ""));
        }

        string BuildGetFriendsRequest(IEnumerable<long> userIds)
        {
            return string.Format(@"var users = [{0}];
            var result = [];

            var i = 0;
            while (i < users.length)
            {{
                result.push({{ userId: users[i], friends: API.friends.get({{ user_id: users[i], fields: ""deactivated,photo_100"" }})}});

                i = i + 1;
            }}

            return result;", String.Join(",", userIds.Select(x => x.ToString())));
        }

        public async Task<string> ExecuteVkApiMethodAsync(string methodName, IDictionary<string, string> parameters, string accessToken = null)
        {
            string url = $"https://api.vk.com/method/{methodName}";

            parameters.Add("v", "5.42");

            if (accessToken != null && !parameters.ContainsKey("access_token"))
            {
                parameters.Add("access_token", accessToken);
            }

            HttpResponseMessage response = null;

            int countOfError = 10;
            while (response == null && countOfError-- > 0)
            {
                try
                {
                    var responseTask = client.PostAsync(url, new FormUrlEncodedContent(parameters));
                    response = await responseTask;
                }
                catch { }
            }

            return await response.Content.ReadAsStringAsync();
        }

        public void FillConsiderUsersGroups(List<VkUser> users, string getMethod = "groups.get")
        {
            int freeTasks = 2;
            HashSet<Task<string>> tasks = new HashSet<Task<string>>();

            for (int i = 0; tasks.Count > 0 || (i < users.Count && i <= 5000); ) // REMOVE ME
            {
                if (freeTasks > 0 && (i < users.Count && i <= 5000)) {
                    string executeCode = BuildGetGroupsRequest(users.Skip(i).Take(25).Select(user => user.User.Id), getMethod, MaxConsiderGroups);

                    var newTask = ExecuteVkApiMethodAsync("execute", new Dictionary<string, string>() { { "code", executeCode } }, vkApi.AccessToken);

                    tasks.Add(newTask);
                    freeTasks--;

                    i += 25;
                }
                else {
                    var completedTask = Task.WhenAny(tasks.ToArray()).Result;

                    tasks.Remove(completedTask);
                    freeTasks++;

                    string vkApiResponse = completedTask.Result;
                    var response = JsonConvert.DeserializeObject<VkApiExecuteResponse<VkApiGetGroupsExecuteUser[]>>(vkApiResponse).response;

                    if (getMethod == "groups.get") // Replace this
                    {
                        foreach (var group in response)
                        {
                            considerUsers[group.userId].Groups = group.groups.items.ToList();
                        }
                    }
                    else
                    {
                        foreach (var group in response)
                        {
                            considerUsers[group.userId].Subscriptions = group.groups.items.ToList();
                        }
                    }

                    // REMOVE ME
                    Console.WriteLine($"Done {i * 100.0 / (double)users.Count}% ({i}/{users.Count})");
                }
            }
        }

        public void FillConsiderUsersSubs(List<VkUser> users, string getMethod = "users.getSubscriptions")
        {
            int maximumHandleUsers = 3000;

            int freeTasks = 2;
            HashSet<Task<string>> tasks = new HashSet<Task<string>>();

            for (int i = 0; tasks.Count > 0 || (i < users.Count && i <= maximumHandleUsers);) // REMOVE ME
            {
                if (freeTasks > 0 && (i < users.Count && i <= maximumHandleUsers))
                {
                    string executeCode = BuildGetGroupsRequest(users.Skip(i).Take(25).Select(user => user.User.Id), getMethod, MaxConsiderGroups, ", extended: 1");

                    var newTask = ExecuteVkApiMethodAsync("execute", new Dictionary<string, string>() { { "code", executeCode } }, vkApi.AccessToken);

                    tasks.Add(newTask);
                    freeTasks--;

                    i += 25;
                }
                else
                {
                    var completedTask = Task.WhenAny(tasks.ToArray()).Result;

                    tasks.Remove(completedTask);
                    freeTasks++;

                    string vkApiResponse = completedTask.Result;
                    var response = JsonConvert.DeserializeObject<VkApiExecuteResponse<VkApiGetSubExecuteUser[]>>(vkApiResponse).response;

                    foreach (var group in response)
                    {
                        considerUsers[group.userId].Subscriptions = group.groups.items.Select(sub => sub.id).ToList();
                    }

                    // REMOVE ME
                    Console.WriteLine($"Done(Subs) {i * 100.0 / (double)users.Count}% ({i}/{users.Count})");
                }
            }
        }

        public double GroupWeightMatchFactor(double linearGroupWeight)
        {
            return Math.Exp(linearGroupWeight * 2.4);
        }

        List<MatchedUser> FillAndSortUsersBySubscription(List<MatchedUser> users)
        {
            FillConsiderUsersSubs(users.Select(u => u.User).ToList(), "users.getSubscriptions");

            // And sort all consider users.

            // Group weight : linear function declared in [0,1]
            // = 1 - groupIndex / MaxConsiderGroups;

            // Group match cost: http://www.wolframalpha.com/input/?i=plot+%28%28e^%28%28x%29+*+2.4%29%29^%281.5%29+%29+from+0+to+1
            // F: (e^((x) * 2.4)), x in [0,1]

            Dictionary<long, MatchedGroup> selfGroupsIdx = new Dictionary<long, MatchedGroup>();

            //System.Diagnostics.Debug.Assert(selfUser.Groups.Count <= MaxConsiderGroups);

            for (int i = 0; i < selfUser.Subscriptions.Count && i < MaxConsiderGroups; i++)
            {
                long id = selfUser.Subscriptions[i];
                double weight = GroupWeightMatchFactor(1.0 - ((double)i / (double)MaxConsiderGroups));

                selfGroupsIdx.Add(id, new MatchedGroup(id, weight));
            }

            // Compare all users to self.
            foreach (var matchingUser in users)
            {
                var groups = matchingUser.User.Subscriptions;

                if (groups == null)
                {
                    continue;
                }

                //System.Diagnostics.Debug.Assert(groups.Count <= MaxConsiderGroups);

                List<long> matchedByUserGroups = new List<long>();
                double sumOfLikely = 0.0;

                // Collect groups match likes
                for (int i = 0; i < groups.Count && i < MaxConsiderGroups; i++)
                {
                    // If matchingUser group intersect self user group
                    if (selfGroupsIdx.ContainsKey(groups[i]))
                    {
                        double groupWeight = GroupWeightMatchFactor(1.0 - ((double)i / (double)MaxConsiderGroups));

                        double groupLikely = groupWeight * selfGroupsIdx[groups[i]].Weight;

                        sumOfLikely += groupLikely;
                        matchedByUserGroups.Add(groups[i]);
                    }
                }

                matchingUser.Likely += sumOfLikely;
                matchingUser.MatchedGroups.AddRange(matchedByUserGroups);
            }

            // Finally sort users, get top, and return result.
            users.Sort();

            return users;
        }

        List<MatchedUser> FillAndSortUsersByGroup(List<VkUser> users)
        {
            FillConsiderUsersGroups(users);

            // And sort all consider users.

            // Group weight : linear function declared in [0,1]
            // = 1 - groupIndex / MaxConsiderGroups;

            // Group match cost: http://www.wolframalpha.com/input/?i=plot+%28%28e^%28%28x%29+*+2.4%29%29^%281.5%29+%29+from+0+to+1
            // F: (e^((x) * 2.4)), x in [0,1]

            Dictionary<long, MatchedGroup> selfGroupsIdx = new Dictionary<long, MatchedGroup>();

            //System.Diagnostics.Debug.Assert(selfUser.Groups.Count <= MaxConsiderGroups);

            for (int i = 0; i < selfUser.Groups.Count && i < MaxConsiderGroups; i++)
            {
                long id = selfUser.Groups[i];
                double weight = GroupWeightMatchFactor(1.0 - ((double)i / (double)MaxConsiderGroups));

                selfGroupsIdx.Add(id, new MatchedGroup(id, weight));
            }

            // Compare all users to self.
            List<MatchedUser> matchedUsers = new List<MatchedUser>(users.Count);

            foreach (var matchingUser in users)
            {
                var groups = matchingUser.Groups;

                if (groups == null)
                {
                    continue;
                }

                //System.Diagnostics.Debug.Assert(groups.Count <= MaxConsiderGroups);

                List<long> matchedByUserGroups = new List<long>();
                double sumOfLikely = 0.0;

                // Collect groups match likes
                for (int i = 0; i < groups.Count && i < MaxConsiderGroups; i++)
                {
                    // If matchingUser group intersect self user group
                    if (selfGroupsIdx.ContainsKey(groups[i]))
                    {
                        double groupWeight = GroupWeightMatchFactor(1.0 - ((double)i / (double)MaxConsiderGroups));

                        double groupLikely = groupWeight * selfGroupsIdx[groups[i]].Weight;

                        sumOfLikely += groupLikely;
                        matchedByUserGroups.Add(groups[i]);
                    }
                }

                // Add current user with likely weight to result list
                matchedUsers.Add(new MatchedUser(matchingUser, sumOfLikely, matchedByUserGroups));
            }

            // Finally sort users, get top, and return result.
            matchedUsers.Sort();

            return matchedUsers;
        }

        public string GetMatchingJsonResponse()
        {
            Console.WriteLine($"Start matching ID({selfUser.User.Id})");

            // First, get friends;
            selfUser.Friends = vkApi.Friends.Get(selfUser.User.Id, ProfileFields.FirstName|ProfileFields.Photo100)
                .Where(friend => friend.Deactivated == null)
                .Select(friend => new VkUser(friend))
                .ToList();
            
            Dictionary<long, VkUser> friendsDict = new Dictionary<long, VkUser>();
            selfUser.Friends.ForEach(x => friendsDict.Add(x.User.Id, x));
            
            // Friends friends, by packet exec (25 friends in one packet)
            for (int i = 0; i < selfUser.Friends.Count; i += 25)
            {
                string executeCode = BuildGetFriendsRequest(selfUser.Friends.Skip(i).Take(25).Select(user => user.User.Id));

                string vkApiResponse = ExecuteVkApiMethodAsync("execute", new Dictionary<string, string>() { { "code", executeCode } }, vkApi.AccessToken).Result;
                var response = JsonConvert.DeserializeObject<VkApiExecuteResponse<VkApiGetFriendExecuteUser[]>>(vkApiResponse).response;
                
                foreach (var friend in response)
                {
                    friendsDict[friend.userId].Friends = friend.friends.items
                        .Where(u => u.deactivated == null)
                        .Select(u => new VkUser(new User() { Id = u.id, Photo100 = (u.photo_100 != null ? new Uri(u.photo_100) : null) })).ToList();
                }

                Console.WriteLine($"Fetching users: {i}/{selfUser.Friends.Count}");
            }

            // user.Friends has VkUser with list of friends.


            // Collect unique users from all friends
            foreach (var friend in selfUser.Friends) // First add friends who have extented info
            {
                AddUniqueUserToConsider(friend);
            }

            foreach (var friend in selfUser.Friends) // Later fr-fr
            {
                foreach (var ff in friend.Friends)
                {
                    AddUniqueUserToConsider(ff);
                }
            }

            // Get necessary fields
            var considerUsersList = considerUsers.Values.ToList();

            // Put friends first
            considerUsersList = considerUsersList.AsParallel().OrderBy(u => friendsDict.ContainsKey(u.User.Id) ? 0 : 1).ToList();

            // Fetch all groups
            selfUser.Groups = vkApi.Groups.Get(selfUser.User.Id, false, null, null, 0, (uint)MaxConsiderGroups).Select(x => x.Id).ToList();
            selfUser.Subscriptions = vkApi.Users.GetSubscriptions(selfUser.User.Id, (int?)MaxConsiderGroups, 0).Select(x => x.Id).ToList();

            // Very long operations:
            //List<MatchedUser> matchedUsers = considerUsersList
            //    .Select(u => new MatchedUser(u, 0, new List<long>())).ToList();

            var matchedUsers = ( from user in considerUsersList
                     select new MatchedUser(user, 0, new List<long>()))
                     .ToList();

            matchedUsers = FillAndSortUsersBySubscription(matchedUsers);

            var resultUsers = matchedUsers.Take(20).Select(u => new { Likely = u.Likely, UserId = u.User.User.Id, Groups = u.MatchedGroups, Photo = u.User.User.Photo100 });

            return JsonConvert.SerializeObject(resultUsers);
        }
    }
}
