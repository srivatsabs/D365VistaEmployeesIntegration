
using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Configuration;


namespace D365VistaEmployeesIntegration
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            ProcessD365 processD365 = new ProcessD365();
			await processD365.ProcessRecordsAsync();
		}
    }
}
