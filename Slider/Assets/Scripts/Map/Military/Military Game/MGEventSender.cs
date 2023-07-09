//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class MGEventSender
//{
//    public delegate void OnEvent(MGEvent e);

//    //public event OnEvent OnEventPublished;
//    //public event OnEvent OnEventSend;

//    private Queue<MGEvent> _eventQueue;
//    private List<MGEventListener> _listeners;

//    public MGEventSender()
//    {
//        _eventQueue = new Queue<MGEvent>();
//        _listeners = new List<MGEventListener>();
//    }

//    public void QueueEvent(MGEvent e)
//    {
//        _eventQueue.Enqueue(e);
//        //OnEventPublished?.Invoke(e);            
//        Debug.Log($"Queued event of type {e.GetType()}");
//    }

//    public IEnumerator ProcessQueue()
//    {
//        while (_eventQueue.Count > 0)
//        {
//            MGEvent e = _eventQueue.Dequeue();
//            Debug.Log($"Sent Event {e.GetType()}");

//            foreach (MGEventListener listener in _listeners)
//            {
//                listener.ProcessEvent(e);
//            }

//            //yield return new WaitUntil(() =>
//            //{
//            //    foreach (MGEventListener listener in _listeners)
//            //    {
//            //        if (!listener.EventFinishFlag)
//            //        {
//            //            return false;
//            //        }
//            //    }

//            //    return true;
//            //});
//        }

//        Debug.Log("Finished Processing Queue");
//    }

//    public void AddListener(MGEventListener listener)
//    {
//        _listeners.Add(listener);
//    }
//}
