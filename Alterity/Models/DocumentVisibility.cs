using System.ComponentModel;
namespace Alterity.Models
{
    public enum DocumentVisibility
    {
        [Description("Anyone, including anonymous users.")]
        Public,
        [Description("Only people that you invite.")]
        Private
    }
}