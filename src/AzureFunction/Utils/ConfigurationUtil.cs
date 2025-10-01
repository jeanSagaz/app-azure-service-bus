namespace AzureFunction.Utils
{
    public static class ConfigurationUtil
    {
        public static string GetConfiguration(string configuration) =>
            Convert.ToString(Environment.GetEnvironmentVariable(configuration))! 
            ?? throw new ArgumentNullException($"'{configuration}' configuration not found");
    }
}
