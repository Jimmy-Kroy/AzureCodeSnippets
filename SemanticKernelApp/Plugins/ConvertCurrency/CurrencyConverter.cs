using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.SemanticKernel;

namespace SemanticKernelApp.Plugins.ConvertCurrency
{
    public class CurrencyConverter
    {
        [KernelFunction, Description("Convert an amount from one currency to another")]
        public static string ConvertAmount(
            [Description("The target currency code")] string targetCurrencyCode,
            [Description("The amount to convert")] string amount,
            [Description("The starting currency code")] string baseCurrencyCode)
        {
            var currencyDictionary = Currency.Currencies;

            Currency targetCurrency = currencyDictionary[targetCurrencyCode];
            Currency baseCurrency = currencyDictionary[baseCurrencyCode];

            if (targetCurrency == null)
            {
                return targetCurrencyCode + " was not found";
            }
            else if (baseCurrency == null)
            {
                return baseCurrencyCode + " was not found";
            }
            else
            {
                double amountInUSD = Double.Parse(amount) * baseCurrency.USDPerUnit;
                double result = amountInUSD * targetCurrency.UnitsPerUSD;
                return @$"${amount} {baseCurrencyCode} is approximately 
            {result.ToString("C")} in {targetCurrency.Name}s ({targetCurrencyCode})";
            }
        }
    }
}
