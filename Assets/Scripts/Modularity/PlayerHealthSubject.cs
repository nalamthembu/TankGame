using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerHealthSubject : MonoBehaviour
{
    // a collection of all observers of this subject...
    private List<IPlayerHealthObserver> m_Observers = new();

    // Add the observer to the subjects collection.
    public void AddObserver(IPlayerHealthObserver observer) => m_Observers.Add(observer);

    //Remove the observer from the subjects collection.
    public void RemoveObserver(IPlayerHealthObserver obserber) => m_Observers.Remove(obserber);

    // Notifies all observers that an event has occurred.
    protected void NotifyObservers(float health, float armor)
    {
        m_Observers.ForEach((observer) =>
        {
            observer.OnNotify(health, armor);
        });
    }

    //Notify all observers about the players death.
    protected void NotifyObserversOfPlayerDeath()
    {
        m_Observers.ForEach((observer) =>
        {
            observer.OnNotifyAboutDeath();
        });
    }
}
