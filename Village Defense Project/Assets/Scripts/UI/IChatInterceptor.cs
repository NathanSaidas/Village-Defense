namespace Gem
{
    public interface IChatInterceptor
    {
        void OnSubmitMessage(string aMessage, UIChatbox aChatbox);
    }
}