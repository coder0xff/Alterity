using System.ComponentModel;
namespace Alterity.Models
{
    public enum DocumentEditability
    {
        [Description("Anyone, including anonymous users.")]
        Public,
        [Description("Anyone that is logged in.")]
        Protected,
        [Description("Only people that you invite.")]
        Private
    }
}