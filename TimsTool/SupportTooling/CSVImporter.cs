using DataLib.DataModel;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SupportTooling
{
    public class CSVImporter
    {
        private FileService fs = new FileService();
        public void ImportCSV()
        {
            var dataFilePath = "D:\\projects\\source\\TimsTool\\TimsTool\\bin\\Debug\\net6.0-windows10.0.17763.0\\ResultsTreeData\\tree.bin";
            AllData data = fs.GetFileData(dataFilePath);
            var csvPath = @"C:\Users\Tim\OneDrive\PersonalScratchpad\CPP\Increment 2.5\ResultText.csv";
            using (TextFieldParser csvParser = new TextFieldParser(csvPath))
            {
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                //csvParser.ReadLine();
                var unmatchedCodes = new List<string>();
                var publishedPendingCodes = new List<string>();
                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();
                    string resultShortCode = fields[0];
                    string resultText = fields[1];

                    if (string.IsNullOrEmpty(resultShortCode))
                    {
                        break;
                    }

                    var matchedResult = data.ResultDefinitions.FirstOrDefault(x =>
                                                    (x.PublishedStatus == null || x.PublishedStatus == DataLib.PublishedStatus.Published || x.PublishedStatus == DataLib.PublishedStatus.PublishedPending) &&
                                                    !string.IsNullOrEmpty(x.ShortCode) &&
                                                    x.ShortCode.ToLowerInvariant() == resultShortCode.ToLowerInvariant()
                                                    );
                    if(matchedResult == null)
                    {
                        unmatchedCodes.Add(resultShortCode);
                    }
                    else
                    {
                        if (matchedResult.PublishedStatus == DataLib.PublishedStatus.PublishedPending)
                        {
                            publishedPendingCodes.Add(resultShortCode);
                        }
                        matchedResult.ResultTextTemplate = resultText;
                        matchedResult.LastModifiedDate = DateTime.Now;
                        matchedResult.LastModifiedUser = "tim.cooke@solirius.com";
                    }                    
                }
                var unmatched = string.Join(",", unmatchedCodes);
                var publishedPending = string.Join(",", publishedPendingCodes);
            }
            fs.WriteFile(dataFilePath, data);
        }
    }
}
