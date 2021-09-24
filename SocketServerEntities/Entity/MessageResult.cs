using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServerEntities.Entity
{
    public enum MessageResult
    {
        Success,
        Error,
        Canceled,
        AwaitingCancellation,
        TryLater,
        InProgress,
        NoResult
    }
}
