using System.Collections.Generic;

namespace RiceTea.Core.Collections;

public class SyncList<T> : SyncList<T, IList<T>>
{
    public SyncList(IList<T> list) : base(list) { }
}
