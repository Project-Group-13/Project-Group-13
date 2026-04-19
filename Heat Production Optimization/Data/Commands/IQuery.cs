using System.Collections.Generic;

namespace Heat_Production_Optimization.Data;

public interface IQuery<T>
{
    public T Execute(string query, Dictionary<string, object> parameters = null);
}
