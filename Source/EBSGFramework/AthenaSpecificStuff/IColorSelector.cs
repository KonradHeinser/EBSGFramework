using UnityEngine;

namespace EBSGFramework
{
    public interface IColorSelector
    {
        Color PrimaryColor { get; set; }
        Color SecondaryColor { get; set; }
        bool UseSecondary { get; }
    }
}
