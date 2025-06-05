
namespace WordList.Common.Logging;

public interface ILogger
{
    void Info(string text);
    void Warning(string text);
    void Error(string text);

    ILogger WithPrefix(string text);
}