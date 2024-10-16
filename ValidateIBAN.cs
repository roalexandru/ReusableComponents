using System;
using System.Collections.Generic;
using System.Data;
using Demo.ReusableComponents.SAP;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Orchestrator.Client.Models;
using UiPath.Testing;
using UiPath.Testing.Activities.TestData;
using UiPath.Testing.Activities.TestDataQueues.Enums;
using UiPath.Testing.Enums;
using UiPath.UIAutomationNext.API.Contracts;
using UiPath.UIAutomationNext.API.Models;
using UiPath.UIAutomationNext.Enums;

namespace Demo.ReusableComponents.SAP
{
    public class ValidateIBAN : CodedWorkflow
    {
        private const int MinIbanLength = 15; // Minimum IBAN length (varies by country)
        private const int Mod97 = 97;

        [Workflow]
        public bool Execute(string iban)
        {
            if (string.IsNullOrWhiteSpace(iban))
            {
                return false; // Null or empty check
            }

            iban = iban.Replace(" ", "").ToUpper(); // Remove spaces and convert to uppercase

            // IBAN basic format check
            if (iban.Length < MinIbanLength || !System.Text.RegularExpressions.Regex.IsMatch(iban, "^[A-Z0-9]+$"))
            {
                return false; // IBAN too short or invalid characters
            }

            // Rearrange the IBAN: move first 4 characters to the end
            string rearrangedIban = iban[4..] + iban[..4];

            // Convert letters to numbers (A = 10, B = 11, ..., Z = 35)
            var numericIbanBuilder = new System.Text.StringBuilder(rearrangedIban.Length);

            foreach (char c in rearrangedIban)
            {
                if (char.IsLetter(c))
                {
                    numericIbanBuilder.Append(c - 'A' + 10); // Convert letters to numbers
                }
                else
                {
                    numericIbanBuilder.Append(c); // Append digits directly
                }
            }

            try
            {
                // Parse the numeric string to a BigInteger
                System.Numerics.BigInteger numericIban = System.Numerics.BigInteger.Parse(numericIbanBuilder.ToString());

                // Perform the mod-97 check
                return numericIban % Mod97 == 1;
            }
            catch (FormatException)
            {
                // Handle cases where the numeric conversion fails
                return false;
            }
            catch (OverflowException)
            {
                // Handle cases where the number is too large
                return false;
            }
        }
    }
}