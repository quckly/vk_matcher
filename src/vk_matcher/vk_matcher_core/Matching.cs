using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Newtonsoft.Json;
using VkNet;
using VkNet.Model;
using VkNet.Enums.Filters;

namespace VKMatcher.Core
{
    public class VkUser
    {
        public User User { get; }
        public List<VkUser> Friends { get; set; }
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

    public class VkApiGetFriendExecuteFriendsList
    {
        public long count;
        public long[] items; // May be array of objects
    }

    public class VkApiGetFriendExecuteUser
    {
        public long userId;
        public VkApiGetFriendExecuteFriendsList friends;
    }

    public class Matching
    {
        VkApi vkApi;
        VkUser selfUser;

        Dictionary<long, VkUser> considerUsers;

        public Matching(string accessToken, uint userId)
        {
            vkApi = new VkApi();
            vkApi.Authorize(accessToken, userId);

            selfUser = new VkUser(vkApi.Users.Get(userId, ProfileFields.All));
        }

        void AddUniqueUserToConsider(VkUser user)
        {
            if (!considerUsers.ContainsKey(user.User.Id))
            {
                considerUsers.Add(user.User.Id, user);
            }
        }

        string BuildGetGroupsRequest(IEnumerable<long> userIds, int count = 30)
        {
            return string.Format(@"var users = [{0}];
            var result = [];

            var i = 0;
            while (i < users.length)
            {
                result.push({ userId: users[i], groups: API.groups.get({ user_id: users[i], count: {1} })});

                i = i + 1;
            }

            return result;", String.Join(",", userIds.Select(x => x.ToString())), count);
        }

        string BuildGetFriendsRequest(IEnumerable<long> userIds)
        {
            return string.Format(@"var users = [{0}];
            var result = [];

            var i = 0;
            while (i < users.length)
            {
                result.push({ userId: users[i], friends: API.friends.get({ user_id: users[i] })});

                i = i + 1;
            }

            return result;", String.Join(",", userIds.Select(x => x.ToString())));
        }

        public void FillConsiderUsersGroups(List<VkUser> users)
        {
            for (int i = 0; i < users.Count; i += 25)
            {
                string executeCode = BuildGetGroupsRequest(users.Skip(i).Take(25).Select(user => user.User.Id));

                string vkApiResponse = vkApi.Invoke("execute", new Dictionary<string, string>() { { "code", executeCode } }, true);
                var response = JsonConvert.DeserializeObject<VkApiGetGroupsExecuteUser[]>(vkApiResponse);

                foreach (var group in response)
                {
                    considerUsers[group.userId].Groups = group.groups.items.ToList();
                }
            }
        }

        public string GetMatchingJsonResponse()
        {
            // First, get friends;
            selfUser.Friends = vkApi.Friends.Get(selfUser.User.Id).Select(friend => new VkUser(friend)).ToList();

            Dictionary<long, VkUser> friendsDict = new Dictionary<long, VkUser>();
            selfUser.Friends.ForEach(x => friendsDict.Add(x.User.Id, x));
            
            // Friends friends, by packet exec (25 friends in one packet)
            for (int i = 0; i < selfUser.Friends.Count; i += 25)
            {
                string executeCode = BuildGetFriendsRequest(selfUser.Friends.Skip(i).Take(25).Select(user => user.User.Id));

                string vkApiResponse = vkApi.Invoke("execute", new Dictionary<string, string>() { { "code", executeCode } }, true);
                var response = JsonConvert.DeserializeObject<VkApiGetFriendExecuteUser[]>(vkApiResponse);
                
                foreach (var friend in response)
                {
                    friendsDict[friend.userId].Friends = friend.friends.items.Select(frId => new VkUser(new User() { Id = frId })).ToList();
                }
            }

            // selfUser.Friends = friendsDict.Values.ToList(); // not have a sense
            friendsDict = null;

            // user.Friends has VkUser with list of friends.


            // Collect unique users from all friends
            foreach (var friend in selfUser.Friends)
            {
                AddUniqueUserToConsider(friend);

                foreach (var ff in friend.Friends)
                {
                    AddUniqueUserToConsider(ff);
                }
            }

            // Get necessary fields
            var considerUsersList = considerUsers.Values.ToList();

            // Fetch all groups
            selfUser.Groups = vkApi.Groups.Get(selfUser.User.Id, false, null, null, 0, 30).Select(x => x.Id).ToList();
            FillConsiderUsersGroups(considerUsersList);

            // And sort all consider users.


            return "";
        }
    }
}
