using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alterity.Models
{
    public class VoteBox
    {
        public int Id { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public ICollection<VoteEntry> Votes { get; set; }

        static public ICollection<VoteEntry> Inheret(ICollection<VoteEntry> selfVotes, ICollection<VoteEntry> inheritedVotes)
        {
            HashSet<String> nonInheritedUserNames = new HashSet<String>(selfVotes.Select(vote => vote.User.UserName));
            List<VoteEntry> results = new List<VoteEntry>(selfVotes);
            foreach (VoteEntry entry in inheritedVotes)
                if (!nonInheritedUserNames.Contains(entry.User.UserName)) results.Add(entry);
            return results;
        }

        static public float ComputeVoteRatio(ICollection<VoteEntry> votes)
        {
            int upVoteCount = 1, totalVoteCount = 0;
            foreach (VoteEntry vote in votes)
            {
                if (vote.WasUpvote) upVoteCount++;
                totalVoteCount++;
            }
            return (float)upVoteCount / totalVoteCount;
        }
    }
}
