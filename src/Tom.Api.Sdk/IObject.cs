using System;
using System.Collections.Generic;

namespace Tom.Api
{
    /// <summary>
    /// 基础对象。
    /// </summary>
    [Serializable]
    public abstract class IObject : Dictionary<string, string>
    {
    }
}
