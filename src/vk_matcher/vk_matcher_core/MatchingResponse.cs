using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VKMatcher.Models
{
    class MatchingResponse
    {
        public long Id { get; }
        public double Likely { get; }
        public IEnumerable<long> Groups { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Photo { get; }
        public string Domain { get; }

        public MatchingResponse(long id, double likely, IEnumerable<long> groups, string firstName, string lastName, string photo, string domain)
        {
            Id = id;
            Likely = likely;
            Groups = groups;
            FirstName = firstName;
            LastName = lastName;
            Photo = photo;
            Domain = domain;
        }

    }
}
