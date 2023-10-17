using System;

namespace FlowAvalonia.PublicAPI;

public class InnerAPI
{
    public event EventHandler<(string newQuery, bool requery)>? QueryTextChanged;

    public void ChangeQueryText(string newQuery, bool reQuery = false)
    {
        QueryTextChanged?.Invoke(this, (newQuery, reQuery));
    }
}