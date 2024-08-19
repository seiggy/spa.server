using Microsoft.AspNetCore.Mvc;

namespace react.server.Model
{
    public class JavaScriptResult : ContentResult
    {
        public JavaScriptResult(string script)
        {
            Content = script;
            ContentType = "application/javascript";
        }
    }
}
