
using Amazon.Lambda.Core;
using WordList.Common.Logging;

public class LambdaContextLogger : ILogger
{
    private readonly ILambdaLogger _lambdaLogger;
    private readonly string _prefix = string.Empty;

    public LambdaContextLogger(ILambdaContext context)
    {
        _lambdaLogger = context.Logger;
    }

    private LambdaContextLogger(ILambdaLogger logger, string prefix)
    {
        _lambdaLogger = logger;
        _prefix = prefix;
    }

    public void Info(string text) => _lambdaLogger.LogInformation($"{_prefix}{text}");
    public void Warning(string text) => _lambdaLogger.LogWarning($"{_prefix}{text}");
    public void Error(string text) => _lambdaLogger.LogError($"{_prefix}{text}");

    public ILogger WithPrefix(string text)
        => new LambdaContextLogger(_lambdaLogger, $"{_prefix}{text}");
}