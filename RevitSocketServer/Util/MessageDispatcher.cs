using ExtensionLib.Util;
using SocketServerEntities.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitSocketServer.Util
{
    public class MessageDispatcher
    {
        private readonly SpinLock _sl = new SpinLock();
        private MessageData _lastDequeued = default;
        private readonly List<(string uid, Queue<MessageData> queue)> _dic = new List<(string uid, Queue<MessageData> queue)>();


        public void Push(string uid, MessageData message)
        {
            _sl.Lock();
            try
            {
                var item = _dic.FirstOrDefault(x => x.uid == uid);
                if (item == default)
                {
                    var q = new Queue<MessageData>();
                    q.Enqueue(message);
                    _dic.Add((uid, q));
                }
                else
                {
                    item.queue.Enqueue(message);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInfo($"{ex.Message}");
            }
            finally
            {
                _sl.Unlock();
            }
        }

        public MessageData Pop()
        {
            _sl.Lock();
            try
            {
                if (_dic.Count > 0)
                {
                    if (_dic[0].queue.Count > 0)
                    {
                        return _lastDequeued = _dic[0].queue.Dequeue();
                    }
                    else
                    {
                        _dic.RemoveAt(0);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex.ToString());
            }
            finally
            {
                _sl.Unlock();
            }
            return default;
        }

        public bool CancelRequest(Guid guid)
        {
            if (_lastDequeued?.Message.Guid == guid)
            {
                _lastDequeued.RequestCancellation.Cancel();
                Logger.WriteInfo($"Отменен {guid}");
                return true;
            }

            foreach (var item in _dic)
            {
                foreach (var el in item.queue)
                {
                    if (el.Message.Guid == guid)
                    {
                        el.RequestCancellation.Cancel();
                        Logger.WriteInfo($"Отменен {guid}");
                        return true;
                    }
                }
            }
            return false;
        }

        public void CancelConnection(int connectionId)
        {
            if (_lastDequeued?.ConnectionId == connectionId)
            {
                _lastDequeued.RequestCancellation.Cancel();
            }
            foreach (var item in _dic)
            {
                foreach (var el in item.queue)
                {
                    if (el.ConnectionId == connectionId)
                    {
                        el.RequestCancellation.Cancel();
                    }
                }
            }
        }

        public int RequestPosition(Guid key)
        {
            int pos = 1;
            foreach (var item in _dic)
            {
                foreach (var el in item.queue)
                {
                    if (el.Message.Guid == key)
                    {
                        return pos;
                    }
                    pos++;
                }
            }
            return -1;
        }
    }
}
