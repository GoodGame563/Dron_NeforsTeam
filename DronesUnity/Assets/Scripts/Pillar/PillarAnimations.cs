using UnityEngine;

public class PillarAnimations : MonoBehaviour
{
    [SerializeField] private Animator _lampAnimator;
    [SerializeField] private Animator _pillarAnimator;

    public void BrokeLamp()
    {
        _lampAnimator.SetBool("Broken", true);
    }

    public void RepareLamp()
    {
        _lampAnimator.SetBool("Broken", false);
    }

    public void OpenDroneLocator()
    {
        _pillarAnimator.SetBool("LocatorOpened", true);
    }

    public void CloseDroneLocator()
    {
        _pillarAnimator.SetBool("LocatorOpened", false);
    }
}
