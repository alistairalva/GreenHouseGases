using System;
using System.Linq;
using System.Xml;           // XmlDocument (DOM) class
using System.Xml.XPath; // XPathNavigator class



namespace GreenHouse
{
    class GreenhouseGases
    {
        private static string _XML_FILE = "ghg-canada.xml", xPath_ListExpression = ""; // File is in the bin\Debug\netcoreapp3.1 folder
        private static XmlDocument _doc = null;
        private static int startYear = 0, endYear = 0, chosen_number;
        private static string[] _regions = null, _sources = null;
        static void Main(string[] args)
        {


            _doc = new XmlDocument();
            _doc.Load(_XML_FILE);
            startYear = 2015;
            endYear = 2019;
            char choice;
            do
            {
                Console.WriteLine("Greenhouse Gas Emissions in Canada!");
                Console.WriteLine("===================================");
                Console.WriteLine("\n'Y' to adjust the range of years");
                Console.WriteLine("'R' to select a specific region");
                Console.WriteLine("'S' to select a specific GHG source");
                Console.WriteLine("'X' to exit the program");
                choice = Console.ReadLine().ToUpper().ElementAt(0);


                if (choice == 'Y')
                {
                    AdjustYears();
                }
                if (choice == 'R')
                {
                    Console.WriteLine("Your selection: R\n");
                    Console.WriteLine("Select a region by number as shown below...");

                    xPath_ListExpression = "//ghg-canada/region/@name";
                    GenerateList(xPath_ListExpression, 'R');

                    Console.WriteLine("\nEnter a region #: ");
                    chosen_number = Convert.ToInt32(Console.ReadLine());

                    if (isValidChoice(chosen_number, _regions))
                    {
                        RegionReport(chosen_number);
                        Console.WriteLine("\nSucess!");
                    }
                }
                if (choice == 'S')
                {
                    Console.WriteLine("Your selection: S\n");
                    Console.WriteLine("Select a source by number as shown below...");

                    xPath_ListExpression = "//ghg-canada/region[1]/source/@description";
                    GenerateList(xPath_ListExpression, 'S');

                    Console.WriteLine("\nEnter a source #: ");
                    chosen_number = Convert.ToInt32(Console.ReadLine());

                    if (isValidChoice(chosen_number, _sources))
                    {
                        SourceReport(chosen_number);
                        Console.WriteLine("\nSucess!");
                    }
                }
                if (choice == 'X')
                {
                    Console.WriteLine("\nAll done!\n");
                    Environment.Exit(0);
                }
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
                Console.WriteLine("\n\n");
            } while (choice == 'Y' || choice == 'R' || choice == 'S');

            Console.WriteLine("END OF PROGRAM!");
        }

        private static void AdjustYears()
        {
            try
            {
                bool valid = false;
                do
                {
                    Console.WriteLine("Starting year (1990 to 2019)");
                    startYear = Convert.ToInt32(Console.ReadLine());
                    if (startYear < 1990 || startYear > 2019)
                    {
                        Console.WriteLine("ERROR: Starting year must be an integer between 1990 and 2019");
                        continue;
                    }
                    Console.WriteLine($"Ending year ({startYear} to {startYear + 4})");
                    endYear = Convert.ToInt32(Console.ReadLine());
                    if (endYear < 1990 || endYear > 2019 || (endYear > (startYear + 5)))
                    {
                        Console.WriteLine($"ERROR: Ending year must be an integer between  {startYear} and {startYear + 4} please try again");
                        continue;
                    }
                    valid = true;
                } while (!valid);

            }
            catch (Exception err)
            {
                Console.WriteLine($"ERROR: {err.Message}");
            }
        }

        private static void GenerateList(string expression, char selection)
        {
            //ghg-canada/region[2]/source/@description
            //ghg-canada/region/@name
            try
            {
                XPathNavigator nav = _doc.CreateNavigator();
                XPathNodeIterator nodeIt = nav.Select(expression);
                int count = 0, printselection = 1;
                if (selection == 'R')
                    _regions = new string[nodeIt.Count];
                if (selection == 'S')
                    _sources = new string[nodeIt.Count];

                while (nodeIt.MoveNext())
                {
                    Console.WriteLine($"\t{printselection++}. {nodeIt.Current.Value}");
                    if (selection == 'R')
                        _regions[count++] = nodeIt.Current.Value;
                    if (selection == 'S')
                        _sources[count++] = nodeIt.Current.Value;
                }
            }
            catch (Exception err)
            {
                Console.WriteLine($"ERROR: {err.Message}");
            }
        }
        private static bool isValidChoice(int choice, string[] array)
        {
            if (choice >= 1 && choice <= array.Length + 1)
                return true;
            else
                return false;
        }

        private static void RegionReport(int regionNum)
        {
            try
            {
                string regionName = _regions[regionNum - 1];
                //ghg-canada/region[@name = 'Alberta']/source[1]/emissions[@year >= 2015 and @year <= 2019]/.
                XmlNodeList regionNodes = _doc.GetElementsByTagName("region");
                int years = startYear;
                Console.WriteLine($"\nEmissions in {regionName} (Megatonnes)");
                Console.WriteLine("---------------------------------------\n");
                Console.Write(String.Format("{0,54}", "Source"));
                for (int i = 0; i <= (endYear - startYear); i++)
                {
                    Console.Write(String.Format("{0,10}", years));
                    years++;
                }
                Console.WriteLine("\n");
                foreach (XmlNode r in regionNodes)
                {

                    //finding the selected region
                    XmlAttributeCollection attrs = r.Attributes;
                    XmlNode nodeName = attrs.GetNamedItem("name");
                    if (nodeName != null && regionName.ToLower() == nodeName.InnerText.ToLower())
                    {
                        XmlNodeList childrenOfRegion = r.ChildNodes;
                        //loops through sources
                        foreach (XmlNode child in childrenOfRegion)
                        {
                            XmlAttributeCollection child_attrs = child.Attributes;
                            XmlNode descriptionName = child_attrs.GetNamedItem("description");

                            Console.Write(String.Format("{0, 54}", descriptionName.InnerText));

                            XmlNodeList emissions = child.ChildNodes;
                            XmlNodeList checkYearList = child.SelectNodes($"//ghg-canada/region[@name = '{regionName}']/source[@description = '{descriptionName.InnerText}']/emissions[@year >= {startYear} and @year <= {endYear}]/.");
                            if (checkYearList.Count < 1)
                            {
                                for (int i = 0; i <= (endYear - startYear); i++)
                                    Console.Write(String.Format("{0, 10}", "-"));
                            }
                            else
                            {
                                foreach (XmlNode e in emissions)
                                {
                                    XmlAttributeCollection emission_attrs = e.Attributes;
                                    XmlNode emission_year = emission_attrs.GetNamedItem("year");
                                    int year = Convert.ToInt32(emission_year.InnerText);
                                    if (year >= startYear && year <= endYear)
                                    {
                                        decimal? value = Decimal.Round(Convert.ToDecimal(e.InnerText), 3);
                                        if (!value.HasValue)
                                            Console.Write(String.Format("{0, 10}", "-"));
                                        else
                                            Console.Write(String.Format("{0, 10}", value));
                                    }
                                }
                            }
                            Console.WriteLine();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                Console.WriteLine($"ERROR: {err.Message}");
            }
        }

        public static void SourceReport(int sourceNum)
        {

            try
            {
                //ghg-canada//source[@description = 'Oil and Gas']/emissions[@year >= 2005 and @year <= 2009]
                string sourceName = _sources[sourceNum - 1];
                XmlNodeList regionNodes = _doc.GetElementsByTagName("region");

                Console.WriteLine($"\nEmissions from {sourceName} in (Megatonnes)");
                Console.WriteLine("----------------------------------------------\n");
                int years = startYear;
                Console.Write(String.Format("{0,34}", "Region"));
                for (int i = 0; i <= (endYear - startYear); i++)
                {
                    Console.Write(String.Format("{0,10}", years));
                    years++;
                }
                Console.WriteLine("\n");
                foreach (XmlNode r in regionNodes)
                {
                    XmlAttributeCollection attrs = r.Attributes;
                    XmlNode regionName = attrs.GetNamedItem("name");
                    XmlNodeList sourceNodes = r.ChildNodes;
                    Console.Write(String.Format("{0,34}", regionName.InnerText));
                    foreach (XmlNode s in sourceNodes)
                    {
                        XmlAttributeCollection source_attrs = s.Attributes;
                        XmlNode sourceNodeName = source_attrs.GetNamedItem("description");
                        if (sourceNodeName != null && sourceName.ToLower() == sourceNodeName.InnerText.ToLower())
                        {

                            XmlNodeList emissionNodes = s.ChildNodes;
                            foreach (XmlNode e in emissionNodes)
                            {
                                XmlAttributeCollection emission_attrs = e.Attributes;
                                XmlNode emission_year = emission_attrs.GetNamedItem("year");
                                int year = Convert.ToInt32(emission_year.InnerText);
                                if (year >= startYear && year <= endYear)
                                {
                                    decimal? value = Decimal.Round(Convert.ToDecimal(e.InnerText), 3);
                                    if (!value.HasValue)
                                        Console.Write(String.Format("{0, 10}", "-"));
                                    else
                                        Console.Write(String.Format("{0, 10}", value));
                                }
                            }

                        }
                    }
                    Console.WriteLine("");
                }
            }
            catch (Exception err)
            {
                Console.WriteLine($"ERROR: {err.Message}");
            }
        }

    }
}