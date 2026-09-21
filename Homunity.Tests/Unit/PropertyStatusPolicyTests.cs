using Homunity_Buisness_Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity.Tests.Unit
{
    public class PropertyStatusPolicyTests
    {
        [Theory]
        [InlineData(1, 2, true)]   // Pending -> Approved
        [InlineData(1, 3, true)]   // Pending -> Rejected
        [InlineData(2, 1, false)]  // Approved -> anything
        [InlineData(2, 3, false)]
        [InlineData(3, 1, false)]  // Rejected -> anything
        [InlineData(3, 2, false)]
        [InlineData(1, 1, false)]  // Pending -> Pending (no-op transition, not allowed)
        public void CanChangeStatus_MatchesOriginalStateMachine(int oldStatus, int newStatus, bool expected)
        {
            Assert.Equal(expected, PropertyStatusPolicy.CanChangeStatus(oldStatus, newStatus));
        }
    }
}
