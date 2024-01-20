public interface IPlayerHealthObserver
{
    //subject uses this method to communicate with the observer.
    public void OnNotify(float healthValue, float ArmorValue);

    public void OnNotifyAboutDeath();
}