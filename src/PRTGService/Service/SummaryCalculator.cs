using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace PRTGService.Service
{
    public class SummaryCalculator
    {
        private string ExtractKey(string input)
        {
            Regex regex = new Regex(@"value channel=\""(.*)\""");
            Match match = regex.Match(input);
            if (match.Success) {
                return match.Groups[1].Value;
            } else {
                return null;
            }
        }

        public float ExtractFloatValue(string input) {
            input = input.Replace("< ", "");
            //Console.WriteLine("input value to extract is " + input);

            Regex regex = new Regex(@"(.*)\s");

            Match match = regex.Match(input);

            var value = match.Groups[1].Value;
            //Console.WriteLine("value to convert to float is " + value);
            return float.Parse(value);
        }

        private void AddToSummary(StatsSummaryHolder holder, string key, float value) {

            var stats = holder._stats;

            List<float> sValue = new List<float>();
            bool exists = stats.TryGetValue(key, out sValue);

            if (exists && !sValue.Contains(value))
            {
                sValue.Add(value);
                stats[key] = sValue;
            }
            else if (!exists)
            {
                sValue = sValue ?? new List<float>();
                sValue.Add(value);
                stats.Add(key, sValue);
            }
        }

        public static string DoFormat(float myNumber)
        {
            var s = string.Format("{0:0.00}", myNumber);

            if (s.EndsWith("00", StringComparison.CurrentCulture))
            {
                return ((int)myNumber).ToString();
            }
            else
            {
                return s;
            }
        }


        public string GetAverageFromSummary(StatsSummaryHolder holder, string key) {

            var stats = holder._stats;

            List<float> results;
            results = stats[key];

            var result = results.Average();
            result = (float)Math.Round(result, 2);

            return DoFormat(result);
        }



        public StatsSummaryHolder GetDataXml(StatsSummaryHolder stats, IEnumerable<XElement> itemsList)
        {

            foreach (XElement item in itemsList)
            {
                var itemFieldList = from itemAttr in item.Elements() select itemAttr;

                //Console.WriteLine("* item.name " + item.Name);

                foreach (XElement itemField in itemFieldList)
                {
                    //Console.WriteLine("* itemField.Name " + itemField.Name + " value " + itemField.Value);

                    string key = ExtractKey(itemField.Name + " " + itemField.Attribute("channel"));
                    string rawValue = itemField.Value;
                    if (key != null && rawValue != null && rawValue != "") {
                        //Console.WriteLine("Key found is " + key);
                        //Console.WriteLine("rawValue " + rawValue);

                        // get value
                        float floatValue = ExtractFloatValue(rawValue);
                        //Console.WriteLine("floatValue is " + floatValue);
                        AddToSummary(stats, key, floatValue);
                    }


                }

            }

            return stats;
        }
    }
}
